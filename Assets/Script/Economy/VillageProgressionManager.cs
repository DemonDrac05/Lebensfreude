using System;
using System.Collections.Generic;
using UnityEngine;

// ─────────────────────────────────────────
// VILLAGE PHASE CONFIG  (ngưỡng tiến hoá 1 làng — điền Inspector)
// ─────────────────────────────────────────
// Toàn bộ con số theo Full Design Document (bảng từng làng). Dùng SO BaseItem thay JSON.
[Serializable]
public class VillagePhaseConfig
{
    public VillageId villageId;

    [Header("Trust -> Partnership (Phase 1 Threshold)")]
    public List<ItemAmount> phase1Threshold = new();   // vd: 25 Wood Plank + 20 Stone Brick

    [Header("Partnership -> mở Revival (Phase 2 Threshold)")]
    public List<ItemAmount> phase2Threshold = new();   // vd: 10 Iron Bar + 8 Treated Lumber
    public int demandEventsRequired = 3;

    [Header("Revival Quest (giao để hồi sinh -> Artifact)")]
    public RevivalQuest revivalQuest = new();
    public Artifact artifactItem;   // SO artifact: trao cho player + chứa ArtifactType
}

// ─────────────────────────────────────────
// VILLAGE PROGRESSION MANAGER  (quản lý phase & hồi sinh 3 làng)
// ─────────────────────────────────────────
// Singleton SỐNG QUA SCENE. Nhận RecordSale từ VillageMarket, cộng dồn giao dịch theo từng phase,
// kiểm tra ngưỡng để nâng phase, mở revival quest, và thưởng Artifact khi hoàn thành.
// Đếm demand event hoàn thành bằng cách nghe DemandEventManager.OnEventFulfilled.
//
// Liên kết: VillageMarket (RecordSale + gating phase), DemandEventManager (đếm event),
//           ArtifactManager (OnArtifactEarned -> trao mảnh artifact), MarketUI (hiển thị tiến độ).
public class VillageProgressionManager : MonoBehaviour
{
    // ─────────────────────────────────────────
    // SINGLETON
    // ─────────────────────────────────────────
    public static VillageProgressionManager Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
        BuildLookup();
    }

    private void OnEnable()
    {
        // Trễ 1 frame để DemandEventManager kịp tạo Instance, rồi mới đăng ký.
        if (DemandEventManager.Instance != null)
            DemandEventManager.Instance.OnEventFulfilled += HandleDemandFulfilled;
    }

    private void OnDisable()
    {
        if (DemandEventManager.Instance != null)
            DemandEventManager.Instance.OnEventFulfilled -= HandleDemandFulfilled;
    }

    // ─────────────────────────────────────────
    // CONFIG  (điền Inspector — 1 entry / làng)
    // ─────────────────────────────────────────
    [SerializeField] private List<VillagePhaseConfig> villageConfigs = new();
    private readonly Dictionary<VillageId, VillagePhaseConfig> _cfg = new();

    private void BuildLookup()
    {
        _cfg.Clear();
        foreach (var c in villageConfigs)
            if (c != null) _cfg[c.villageId] = c;
    }

    // ─────────────────────────────────────────
    // RUNTIME STATE
    // ─────────────────────────────────────────
    private readonly Dictionary<VillageId, VillagePhase> _phase = new();
    private readonly Dictionary<VillageId, Dictionary<BaseItem, int>> _tradedThisPhase = new();
    private readonly Dictionary<VillageId, int> _demandDoneThisPhase = new();
    private readonly Dictionary<VillageId, bool> _revivalUnlocked = new();

    // Sự kiện cho UI / hệ thống khác.
    public event Action<VillageId, VillagePhase> OnPhaseAdvanced;
    public event Action<VillageId>               OnRevivalUnlocked;
    public event Action<ArtifactType>            OnArtifactEarned;

    // ─────────────────────────────────────────
    // PUBLIC QUERY
    // ─────────────────────────────────────────
    // Phase hiện tại của 1 làng (mặc định Abandoned nếu chưa khởi tạo).
    // Dùng trong: VillageMarket.CurrentPhase, MarketUI.
    public VillagePhase GetPhase(VillageId id)
        => _phase.TryGetValue(id, out var p) ? p : VillagePhase.Abandoned;

    public bool IsRevivalUnlocked(VillageId id)
        => _revivalUnlocked.TryGetValue(id, out var b) && b;

    // ─────────────────────────────────────────
    // DISCOVERY  — gọi khi người chơi lần đầu tiếp xúc làng
    // ─────────────────────────────────────────
    // Đưa làng từ Abandoned -> Trust (mở shop cơ bản). Dùng trong: VillageMarket/CaveEntrance/trigger.
    public void Discover(VillageId id)
    {
        if (GetPhase(id) != VillagePhase.Abandoned) return;
        SetPhase(id, VillagePhase.Trust);
        EnsureBuckets(id);
    }

    // ─────────────────────────────────────────
    // RECORD SALE  — cộng tiến độ giao dịch (gọi từ VillageMarket.RegisterSale)
    // ─────────────────────────────────────────
    public void RecordSale(VillageId id, BaseItem item, int qty)
    {
        if (item == null || qty <= 0) return;
        if (GetPhase(id) == VillagePhase.Abandoned) Discover(id); // bán được nghĩa là đã gặp làng
        EnsureBuckets(id);

        var bucket = _tradedThisPhase[id];
        bucket.TryGetValue(item, out int cur);
        bucket[item] = cur + qty;

        // Nếu revival đã mở -> đồng thời tính vào đơn revival.
        if (IsRevivalUnlocked(id) && _cfg.TryGetValue(id, out var c))
            c.revivalQuest.AddDelivery(item, qty);

        TryAdvance(id);
    }

    // ─────────────────────────────────────────
    // DEMAND EVENTS  — đếm số demand đã hoàn thành (nghe DemandEventManager)
    // ─────────────────────────────────────────
    private void HandleDemandFulfilled(DemandEvent evt, int bonus)
    {
        if (evt == null) return;
        EnsureBuckets(evt.villageId);
        _demandDoneThisPhase[evt.villageId] = _demandDoneThisPhase[evt.villageId] + 1;
        TryAdvance(evt.villageId);
    }

    // ─────────────────────────────────────────
    // ADVANCE LOGIC
    // ─────────────────────────────────────────
    // Kiểm tra điều kiện để nâng phase / mở revival / trao artifact. Dùng trong: RecordSale, HandleDemandFulfilled.
    private void TryAdvance(VillageId id)
    {
        if (!_cfg.TryGetValue(id, out var c)) return;
        var phase = GetPhase(id);
        var traded = _tradedThisPhase[id];

        switch (phase)
        {
            case VillagePhase.Trust:
                // Đủ ngưỡng Phase 1 -> Partnership (mở shop hiếm).
                if (Meets(traded, c.phase1Threshold))
                {
                    AdvanceTo(id, VillagePhase.Partnership);
                }
                break;

            case VillagePhase.Partnership:
                if (!IsRevivalUnlocked(id))
                {
                    // Đủ Phase 2 (giao dịch) + đủ số demand event -> MỞ revival quest.
                    if (_demandDoneThisPhase[id] >= c.demandEventsRequired && Meets(traded, c.phase2Threshold))
                    {
                        _revivalUnlocked[id] = true;
                        OnRevivalUnlocked?.Invoke(id);
                    }
                }
                else
                {
                    // Đã mở revival -> giao đủ ĐƠN + đủ TIỀN -> Revival + Artifact.
                    if (c.revivalQuest.IsComplete()
                        && InventoryManager.token >= c.revivalQuest.requiredCoins)
                    {
                        if (c.revivalQuest.requiredCoins > 0)
                            InventoryManager.SpendToken(c.revivalQuest.requiredCoins); // trả phí hồi sinh
                        AdvanceTo(id, VillagePhase.Revival);
                        ArtifactType at = c.artifactItem != null ? c.artifactItem.type : ArtifactType.None;
                        if (c.artifactItem != null)
                            InventoryManager.Instance?.AddItem(c.artifactItem); // trao MÓN artifact thật vào kho
                        OnArtifactEarned?.Invoke(at);
                        ArtifactManager.Instance?.Grant(at);                    // ghi mốc "đã kiếm" (cho dream hint)
                    }
                }
                break;
        }
    }

    // Nâng phase và reset bộ đếm "theo phase" để ngưỡng phase sau tính lại từ đầu.
    private void AdvanceTo(VillageId id, VillagePhase next)
    {
        SetPhase(id, next);
        _tradedThisPhase[id] = new Dictionary<BaseItem, int>();
        _demandDoneThisPhase[id] = 0;
    }

    private void SetPhase(VillageId id, VillagePhase p)
    {
        _phase[id] = p;
        OnPhaseAdvanced?.Invoke(id, p);
    }

    // ─────────────────────────────────────────
    // HELPERS
    // ─────────────────────────────────────────
    // Đã giao đủ MỌI món trong yêu cầu chưa?
    private static bool Meets(Dictionary<BaseItem, int> traded, List<ItemAmount> req)
    {
        if (req == null || req.Count == 0) return true; // không cấu hình -> coi như đạt
        foreach (var r in req)
        {
            if (r == null || r.item == null) continue;
            traded.TryGetValue(r.item, out int have);
            if (have < r.amount) return false;
        }
        return true;
    }

    // Bảo đảm các bucket runtime tồn tại cho 1 làng.
    private void EnsureBuckets(VillageId id)
    {
        if (!_phase.TryGetValue(id, out _)) _phase[id] = VillagePhase.Abandoned;
        if (!_tradedThisPhase.ContainsKey(id)) _tradedThisPhase[id] = new Dictionary<BaseItem, int>();
        if (!_demandDoneThisPhase.ContainsKey(id)) _demandDoneThisPhase[id] = 0;
        if (!_revivalUnlocked.ContainsKey(id)) _revivalUnlocked[id] = false;
    }

    // ─────────────────────────────────────────
    // DEBUG / TESTING — ép phase bất kỳ (dùng cho PhaseDebugPanel)
    // ─────────────────────────────────────────
    // Reset bucket đếm để tránh lên phase ngay lập tức sau khi ép.
    // Dùng trong: PhaseDebugPanel.cs (dropdown UI).
    public void ForceSetPhase(VillageId id, VillagePhase phase)
    {
        EnsureBuckets(id);
        AdvanceTo(id, phase);
    }

    // Khôi phục phase + revival cho 1 làng khi load. Dùng trong: SaveManager.Load().
    public void LoadState(VillageId id, VillagePhase phase, bool revivalUnlocked)
    {
        EnsureBuckets(id);
        _phase[id] = phase;
        _revivalUnlocked[id] = revivalUnlocked;
        OnPhaseAdvanced?.Invoke(id, phase);
    }
}
