using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class VillagePhaseConfig
{
    public VillageId villageId;

    [Header("Trust -> Partnership (Phase 1 Threshold)")]
    public List<ItemAmount> phase1Threshold = new();   // vd: 25 Wood Plank + 20 Stone Brick

    [Header("Partnership -> unlocks Revival (Phase 2 Threshold)")]
    public List<ItemAmount> phase2Threshold = new();   // vd: 10 Iron Bar + 8 Treated Lumber
    public int demandEventsRequired = 3;

    [Header("Revival Quest (complete to revive -> Artifact)")]
    public RevivalQuest revivalQuest = new();
    public Artifact artifactItem;
}

//
public class VillageProgressionManager : MonoBehaviour
{
    // SINGLETON
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
        if (DemandEventManager.Instance != null)
            DemandEventManager.Instance.OnEventFulfilled += HandleDemandFulfilled;
    }

    private void OnDisable()
    {
        if (DemandEventManager.Instance != null)
            DemandEventManager.Instance.OnEventFulfilled -= HandleDemandFulfilled;
    }

    [SerializeField] private List<VillagePhaseConfig> villageConfigs = new();
    private readonly Dictionary<VillageId, VillagePhaseConfig> _cfg = new();

    private void BuildLookup()
    {
        _cfg.Clear();
        foreach (var c in villageConfigs)
            if (c != null) _cfg[c.villageId] = c;
    }

    // RUNTIME STATE
    private readonly Dictionary<VillageId, VillagePhase> _phase = new();
    private readonly Dictionary<VillageId, Dictionary<BaseItem, int>> _tradedThisPhase = new();
    private readonly Dictionary<VillageId, int> _demandDoneThisPhase = new();
    private readonly Dictionary<VillageId, bool> _revivalUnlocked = new();

    public event Action<VillageId, VillagePhase> OnPhaseAdvanced;
    public event Action<VillageId>               OnRevivalUnlocked;
    public event Action<ArtifactType>            OnArtifactEarned;

    // PUBLIC QUERY
    public VillagePhase GetPhase(VillageId id)
        => _phase.TryGetValue(id, out var p) ? p : VillagePhase.Abandoned;

    public bool IsRevivalUnlocked(VillageId id)
        => _revivalUnlocked.TryGetValue(id, out var b) && b;

    public void Discover(VillageId id)
    {
        if (GetPhase(id) != VillagePhase.Abandoned) return;
        SetPhase(id, VillagePhase.Trust);
        EnsureBuckets(id);
    }

    public void RecordSale(VillageId id, BaseItem item, int qty)
    {
        if (item == null || qty <= 0) return;
        if (GetPhase(id) == VillagePhase.Abandoned) Discover(id);
        EnsureBuckets(id);

        var bucket = _tradedThisPhase[id];
        bucket.TryGetValue(item, out int cur);
        bucket[item] = cur + qty;

        if (IsRevivalUnlocked(id) && _cfg.TryGetValue(id, out var c))
            c.revivalQuest.AddDelivery(item, qty);

        TryAdvance(id);
    }

    private void HandleDemandFulfilled(DemandEvent evt, int bonus)
    {
        if (evt == null) return;
        EnsureBuckets(evt.villageId);
        _demandDoneThisPhase[evt.villageId] = _demandDoneThisPhase[evt.villageId] + 1;
        TryAdvance(evt.villageId);
    }

    // ADVANCE LOGIC
    private void TryAdvance(VillageId id)
    {
        if (!_cfg.TryGetValue(id, out var c)) return;
        var phase = GetPhase(id);
        var traded = _tradedThisPhase[id];

        switch (phase)
        {
            case VillagePhase.Trust:
                if (Meets(traded, c.phase1Threshold))
                {
                    AdvanceTo(id, VillagePhase.Partnership);
                }
                break;

            case VillagePhase.Partnership:
                if (!IsRevivalUnlocked(id))
                {
                    if (_demandDoneThisPhase[id] >= c.demandEventsRequired && Meets(traded, c.phase2Threshold))
                    {
                        _revivalUnlocked[id] = true;
                        OnRevivalUnlocked?.Invoke(id);
                    }
                }
                else
                {
                    if (c.revivalQuest.IsComplete()
                        && InventoryManager.token >= c.revivalQuest.requiredCoins)
                    {
                        if (c.revivalQuest.requiredCoins > 0)
                            InventoryManager.SpendToken(c.revivalQuest.requiredCoins);
                        AdvanceTo(id, VillagePhase.Revival);
                        ArtifactType at = c.artifactItem != null ? c.artifactItem.type : ArtifactType.None;
                        if (c.artifactItem != null)
                            InventoryManager.Instance?.AddItem(c.artifactItem);
                        OnArtifactEarned?.Invoke(at);
                        ArtifactManager.Instance?.Grant(at);
                    }
                }
                break;
        }
    }

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

    // HELPERS
    private static bool Meets(Dictionary<BaseItem, int> traded, List<ItemAmount> req)
    {
        if (req == null || req.Count == 0) return true;
        foreach (var r in req)
        {
            if (r == null || r.item == null) continue;
            traded.TryGetValue(r.item, out int have);
            if (have < r.amount) return false;
        }
        return true;
    }

    private void EnsureBuckets(VillageId id)
    {
        if (!_phase.TryGetValue(id, out _)) _phase[id] = VillagePhase.Abandoned;
        if (!_tradedThisPhase.ContainsKey(id)) _tradedThisPhase[id] = new Dictionary<BaseItem, int>();
        if (!_demandDoneThisPhase.ContainsKey(id)) _demandDoneThisPhase[id] = 0;
        if (!_revivalUnlocked.ContainsKey(id)) _revivalUnlocked[id] = false;
    }

    public void ForceSetPhase(VillageId id, VillagePhase phase)
    {
        EnsureBuckets(id);
        AdvanceTo(id, phase);
    }

    public void LoadState(VillageId id, VillagePhase phase, bool revivalUnlocked)
    {
        EnsureBuckets(id);
        _phase[id] = phase;
        _revivalUnlocked[id] = revivalUnlocked;
        OnPhaseAdvanced?.Invoke(id, phase);
    }
}
