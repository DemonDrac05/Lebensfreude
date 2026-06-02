using System;
using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }
    [SerializeField] private ItemDatabase itemDatabase;
    [SerializeField] private bool autoLoadOnStart = false; // Tắt tự động nạp để Menu điều hướng chuẩn xác hơn
    [SerializeField] private bool autoSaveOnNewDay = true;

    private string Path => Application.persistentDataPath + "/lebensfreude_save.json";

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this; transform.SetParent(null); DontDestroyOnLoad(gameObject);
    }
    private void Start() { if (autoLoadOnStart && HasSave()) Load(); }
    private void OnEnable()  { if (autoSaveOnNewDay) TimeManager.OnNewDay += Save; }
    private void OnDisable() { if (autoSaveOnNewDay) TimeManager.OnNewDay -= Save; }

    public bool HasSave() => File.Exists(Path);

    public void Save()
    {
        Player player = FindObjectOfType<Player>();
        Vector3 playerPos = player != null ? player.transform.position : Vector3.zero;

        var d = new GameSaveData
        {
            token     = InventoryManager.token,
            totalDays = TimeManager.TotalDays,
            stamina   = StaminaManager.Instance != null ? StaminaManager.Instance.Current : 100f,
            playerX   = playerPos.x,
            playerY   = playerPos.y,
            playerZ   = playerPos.z,
            currentDepth = DungeonManager.Instance != null ? DungeonManager.Instance.currentDepth : 0
        };

        if (InventoryManager.Instance != null)
            foreach (var kv in InventoryManager.Instance.GetAllStacks())
                if (kv.Key != null) d.inventory.Add(new ItemStackSave { item = kv.Key.name, count = kv.Value });

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

        File.WriteAllText(Path, JsonUtility.ToJson(d, true));
        Debug.Log("[Save] -> " + Path);
    }

    public void Load()
    {
        if (!HasSave()) return;
        var d = JsonUtility.FromJson<GameSaveData>(File.ReadAllText(Path));
        if (d == null) return;

        InventoryManager.token = d.token;
        TimeManager.TotalDays  = d.totalDays;
        StaminaManager.Instance?.LoadStamina(d.stamina);

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.ClearAll();
            foreach (var s in d.inventory)
            {
                var item = itemDatabase != null ? itemDatabase.Find(s.item) : null;
                if (item != null) for (int i = 0; i < s.count; i++) InventoryManager.Instance.AddItem(item);
            }
        }

        if (VillageProgressionManager.Instance != null)
            foreach (var v in d.villages)
                VillageProgressionManager.Instance.LoadState((VillageId)v.villageId, (VillagePhase)v.phase, v.revivalUnlocked);

        if (ArtifactManager.Instance != null)
        {
            foreach (var a in d.artifactsEarned)   ArtifactManager.Instance.Grant((ArtifactType)a);
            foreach (var a in d.artifactsInserted) ArtifactManager.Instance.Insert((ArtifactType)a);
        }

        // Khôi phục trạng thái Hầm ngục và vị trí người chơi
        DungeonManager dm = DungeonManager.Instance;
        Player player = FindObjectOfType<Player>();
        
        if (dm != null)
        {
            dm.currentDepth = d.currentDepth;
            if (d.currentDepth > 0)
            {
                // Nếu người chơi thoát game khi đang ở hầm ngục, tự động dựng lại tầng hầm ngục đó
                dm.dungeonGenerator?.GenerateFloor(d.currentDepth, dm.dungeonOffset);
            }
        }

        if (player != null)
        {
            Vector3 savedPos = new Vector3(d.playerX, d.playerY, d.playerZ);
            player.transform.position = savedPos;
            Camera.main.transform.position = new Vector3(savedPos.x, savedPos.y, Camera.main.transform.position.z);
        }

        Debug.Log("[Load] Nạp file lưu thành công.");
    }

    public void DeleteSave() { if (HasSave()) File.Delete(Path); }
}