using System;
using System.Collections.Generic;
using UnityEngine;

//
public class DemandEventManager : MonoBehaviour
{
    // SINGLETON
    public static DemandEventManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()  => TimeManager.OnNewDay += HandleNewDay;
    private void OnDisable() => TimeManager.OnNewDay -= HandleNewDay;

    // DATA
    private readonly Dictionary<VillageId, DemandEvent> _active = new();

    public event Action<DemandEvent>      OnEventGenerated;
    public event Action<DemandEvent, int> OnEventFulfilled;
    public event Action<DemandEvent>      OnEventExpired;

    // DAY CYCLE
    private void HandleNewDay()
    {
        ExpireOld();
        GenerateNew();
    }

    private void ExpireOld()
    {
        int day = TimeManager.TotalDays;
        var toRemove = new List<VillageId>();
        foreach (var kvp in _active)
        {
            if (kvp.Value.IsExpired(day))
            {
                OnEventExpired?.Invoke(kvp.Value);
                toRemove.Add(kvp.Key);
            }
        }
        foreach (var id in toRemove) _active.Remove(id);
    }

    private void GenerateNew()
    {
        int day = TimeManager.TotalDays;
        var markets = UnityEngine.Object.FindObjectsByType<VillageMarket>(FindObjectsSortMode.None);

        foreach (var market in markets)
        {
            if (_active.ContainsKey(market.villageId)) continue;
            if (market.Items == null || market.Items.Count == 0) continue;

            var buyable = new List<BaseItem>();
            foreach (var cfg in market.Items)
                if (cfg != null && cfg.item != null && market.Buys(cfg.item))
                    buyable.Add(cfg.item);
            if (buyable.Count == 0) continue;

            BaseItem pick = buyable[UnityEngine.Random.Range(0, buyable.Count)];
            int amount = UnityEngine.Random.Range(3, 10);

            var evt = new DemandEvent
            {
                villageId       = market.villageId,
                item            = pick,
                requiredAmount  = amount,
                bonusMultiplier = 2f,
                deadlineDay     = day + 1,
                filledAmount    = 0
            };
            _active[market.villageId] = evt;
            OnEventGenerated?.Invoke(evt);
        }
    }

    public int ApplyDemandBonus(VillageId villageId, BaseItem item, int amount, int unitPrice)
    {
        if (!_active.TryGetValue(villageId, out var evt)) return 0;
        if (evt.item != item || evt.IsExpired(TimeManager.TotalDays)) return 0;

        int applied = Mathf.Min(amount, evt.Remaining);
        if (applied <= 0) return 0;

        evt.filledAmount += applied;
        int bonus = Mathf.RoundToInt(applied * unitPrice * (evt.bonusMultiplier - 1f));

        if (evt.IsFulfilled)
        {
            OnEventFulfilled?.Invoke(evt, bonus);
            _active.Remove(villageId);
        }
        return bonus;
    }

    // QUERY
    public DemandEvent GetActiveEvent(VillageId villageId)
        => _active.TryGetValue(villageId, out var e) ? e : null;

    public List<DemandEvent> GetAllActive() => new(_active.Values);
}
