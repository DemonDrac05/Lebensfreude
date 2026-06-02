using System;
using System.IO;
using UnityEngine;

// ─────────────────────────────────────────
// SAVE MANAGER  (lưu/đọc state cốt lõi qua JSON file)
// ─────────────────────────────────────────
// Lưu: token, ngày, stamina, inventory, phase 3 làng, artifacts (earned+inserted).
// Singleton bền qua scene. Tự load lúc Start (nếu autoLoad) + tự save mỗi ngày mới (nếu autoSave).
// Cần gán ItemDatabase (allItems) để load lại item từ tên.
public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }
    [SerializeField] private ItemDatabase itemDatabase;
    [SerializeField] private bool autoLoadOnStart = true;
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

    // ─────────────────────────────────────────
    // SAVE
    // ─────────────────────────────────────────
    public void Save()
    {
        var d = new GameSaveData
        {
            token     = InventoryManager.token,
            totalDays = TimeManager.TotalDays,
            stamina   = StaminaManager.Instance != null ? StaminaManager.Instance.Current : 100f
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

    // ─────────────────────────────────────────
    // LOAD
    // ─────────────────────────────────────────
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
        Debug.Log("[Load] OK");
    }

    // Xóa save (cho nút New Game). Dùng trong: Menu.
    public void DeleteSave() { if (HasSave()) File.Delete(Path); }
}
