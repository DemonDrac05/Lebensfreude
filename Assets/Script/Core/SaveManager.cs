using System;
using System.Collections;
using System.IO;
using UnityEngine;

/// <summary>
/// Owns the single-slot save file and restores world state when a game is loaded.
///
/// Design notes (why this is structured the way it is):
///  * It lives in the gameplay (Overworld) scene and is recreated every time that scene
///    loads. It is intentionally NOT DontDestroyOnLoad - a persistent manager would keep
///    references to objects from a previous scene instance (destroyed sliders, generators,
///    etc.) and throw while loading. A fresh instance always has live references.
///  * The main menu never needs an instance: it asks the static SaveFileExists() whether a
///    save is present, and sets GameSession.IsLoadingSave before it loads the gameplay
///    scene. This manager's Start() then performs the restore.
///  * Every subsystem in Load() is wrapped in its own try/catch so a failure in one area
///    (e.g. a missing artifact manager) can never abort restoring the inventory or position.
/// </summary>
public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    [SerializeField] private ItemDatabase itemDatabase;
    [SerializeField] private bool autoLoadOnStart = false;
    [SerializeField] private bool autoSaveOnNewDay = true;

    // Static so the menu (which has no SaveManager) can still check for a save.
    private static string SavePath => Application.persistentDataPath + "/lebensfreude_save.json";
    public static bool SaveFileExists() => File.Exists(SavePath);
    public static void DeleteSaveFile() { if (SaveFileExists()) File.Delete(SavePath); }

    private ItemDatabase _fallbackDb;

    private void Awake()
    {
        // One live manager per scene load. No persistence, no duplicate-guard needed.
        Instance = this;
    }

    private void Start()
    {
        if (GameSession.IsLoadingSave)
            StartCoroutine(LoadWhenReady());
        else if (autoLoadOnStart && SaveFileExists())
            Load();
    }

    private void OnEnable()  { if (autoSaveOnNewDay) TimeManager.OnNewDay += Save; }
    private void OnDisable() { if (autoSaveOnNewDay) TimeManager.OnNewDay -= Save; }

    // Wait one frame so that every other object's Awake AND Start have already run
    // (singletons created, inventory built, player spawned) before we overwrite their
    // state from disk. Loading too early is exactly what made saved data disappear.
    private IEnumerator LoadWhenReady()
    {
        yield return null;
        Load();
        GameSession.IsLoadingSave = false;
    }

    public bool HasSave() => SaveFileExists();

    public void Save()
    {
        try
        {
            Player player = FindObjectOfType<Player>();
            Vector3 playerPos = player != null ? player.transform.position : Vector3.zero;

            var d = new GameSaveData
            {
                token        = InventoryManager.token,
                totalDays    = TimeManager.TotalDays,
                stamina      = StaminaManager.Instance != null ? StaminaManager.Instance.Current : 100f,
                playerX      = playerPos.x,
                playerY      = playerPos.y,
                playerZ      = playerPos.z,
                currentDepth = DungeonManager.Instance != null ? DungeonManager.Instance.currentDepth : 0
            };

            if (InventoryManager.Instance != null)
                foreach (var kv in InventoryManager.Instance.GetAllStacks())
                    if (kv.Key != null && kv.Value > 0)
                        d.inventory.Add(new ItemStackSave { item = kv.Key.name, count = kv.Value });

            if (VillageProgressionManager.Instance != null)
                foreach (VillageId v in Enum.GetValues(typeof(VillageId)))
                    d.villages.Add(new VillageSave
                    {
                        villageId       = (int)v,
                        phase           = (int)VillageProgressionManager.Instance.GetPhase(v),
                        revivalUnlocked = VillageProgressionManager.Instance.IsRevivalUnlocked(v)
                    });

            if (ArtifactManager.Instance != null)
                foreach (ArtifactType a in Enum.GetValues(typeof(ArtifactType)))
                {
                    if (a == ArtifactType.None) continue;
                    if (ArtifactManager.Instance.HasArtifact(a)) d.artifactsEarned.Add((int)a);
                    if (ArtifactManager.Instance.IsInserted(a))  d.artifactsInserted.Add((int)a);
                }

            File.WriteAllText(SavePath, JsonUtility.ToJson(d, true));
            Debug.Log($"[Save] {d.inventory.Count} stacks, token={d.token}, day={d.totalDays}, depth={d.currentDepth} -> {SavePath}");
        }
        catch (Exception e)
        {
            Debug.LogError("[Save] Failed to write save: " + e);
        }
    }

    public void Load()
    {
        if (!SaveFileExists()) { Debug.LogWarning("[Load] No save file to load."); return; }

        GameSaveData d;
        try { d = JsonUtility.FromJson<GameSaveData>(File.ReadAllText(SavePath)); }
        catch (Exception e) { Debug.LogError("[Load] Could not parse save file: " + e); return; }
        if (d == null) { Debug.LogError("[Load] Save file parsed to null."); return; }

        // --- Currency + calendar: plain statics, cannot fail ---
        InventoryManager.token = d.token;
        TimeManager.TotalDays  = d.totalDays;

        // --- Inventory ---
        try
        {
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.ClearAll();
                foreach (var s in d.inventory)
                {
                    if (s == null || string.IsNullOrEmpty(s.item) || s.count <= 0) continue;
                    BaseItem item = ResolveItem(s.item);
                    if (item == null)
                    {
                        Debug.LogWarning($"[Load] Item '{s.item}' is not in the ItemDatabase - skipped. Add it to ItemDataBase.allItems so it can be restored.");
                        continue;
                    }
                    for (int i = 0; i < s.count; i++) InventoryManager.Instance.AddItem(item);
                }
            }
        }
        catch (Exception e) { Debug.LogError("[Load] Inventory restore failed: " + e); }

        // --- Player position + camera ---
        try
        {
            Player player = FindObjectOfType<Player>();
            if (player != null)
            {
                Vector3 savedPos = new Vector3(d.playerX, d.playerY, d.playerZ);
                player.transform.position = savedPos;
                var rb = player.GetComponent<Rigidbody2D>();
                if (rb != null) rb.position = savedPos;     // movement is kinematic (MovePosition), so move the body too
                if (Camera.main != null)
                    Camera.main.transform.position = new Vector3(savedPos.x, savedPos.y, Camera.main.transform.position.z);
            }
        }
        catch (Exception e) { Debug.LogError("[Load] Player position restore failed: " + e); }

        // --- Stamina ---
        try { if (StaminaManager.Instance != null) StaminaManager.Instance.LoadStamina(d.stamina); }
        catch (Exception e) { Debug.LogError("[Load] Stamina restore failed: " + e); }

        // --- Dungeon depth (rebuild the floor if the save was made underground) ---
        try
        {
            DungeonManager dm = DungeonManager.Instance;
            if (dm != null)
            {
                dm.currentDepth = d.currentDepth;
                if (d.currentDepth > 0)
                {
                    if (dm.dungeonGenerator == null) dm.dungeonGenerator = FindObjectOfType<DungeonGenerator>();
                    if (dm.dungeonGenerator != null) dm.dungeonGenerator.GenerateFloor(d.currentDepth, dm.dungeonOffset);
                }
            }
        }
        catch (Exception e) { Debug.LogError("[Load] Dungeon restore failed: " + e); }

        // --- Village progression ---
        try
        {
            if (VillageProgressionManager.Instance != null)
                foreach (var v in d.villages)
                    VillageProgressionManager.Instance.LoadState((VillageId)v.villageId, (VillagePhase)v.phase, v.revivalUnlocked);
        }
        catch (Exception e) { Debug.LogError("[Load] Village restore failed: " + e); }

        // --- Artifacts ---
        try
        {
            if (ArtifactManager.Instance != null)
            {
                foreach (var a in d.artifactsEarned)   ArtifactManager.Instance.Grant((ArtifactType)a);
                foreach (var a in d.artifactsInserted) ArtifactManager.Instance.Insert((ArtifactType)a);
            }
        }
        catch (Exception e) { Debug.LogError("[Load] Artifact restore failed: " + e); }

        Debug.Log($"[Load] Done - {d.inventory.Count} stacks, token={d.token}, pos=({d.playerX:0.0},{d.playerY:0.0}), depth={d.currentDepth}.");
    }

    // Name -> BaseItem. Primary source is the assigned ItemDatabase. If that field was left
    // unassigned, fall back to any ItemDatabase currently loaded in memory so a restore is
    // never silently empty just because of one missing inspector reference.
    private BaseItem ResolveItem(string itemName)
    {
        if (itemDatabase != null)
        {
            var found = itemDatabase.Find(itemName);
            if (found != null) return found;
        }
        if (_fallbackDb == null)
        {
            var all = Resources.FindObjectsOfTypeAll<ItemDatabase>();
            if (all != null && all.Length > 0) _fallbackDb = all[0];
        }
        return _fallbackDb != null ? _fallbackDb.Find(itemName) : null;
    }

    // Kept for any existing callers / inspector buttons.
    public void DeleteSave() => DeleteSaveFile();
}
