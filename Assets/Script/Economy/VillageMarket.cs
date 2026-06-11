using System;
using System.Collections.Generic;
using UnityEngine;

//
public class VillageMarket : MonoBehaviour
{
    [Header("=== Village identity ==========")]
    public VillageId villageId;
    public string displayName => villageId.ToString();

    [Header("=== Goods the village BUYS from the player (player sells) ==========")]
    [SerializeField] private List<VillageItemConfig> items = new();

    [Header("=== Goods the village SELLS to the player (player buys) ==========")]
    [SerializeField] private List<VillageStockItem> sellsToPlayer = new();

    // STATE
    private MarketState _state;
    public MarketState State => _state;
    public IReadOnlyList<VillageItemConfig> Items => items;
    public IReadOnlyList<VillageStockItem>  SellsToPlayer => sellsToPlayer;

    public event Action<VillageMarket, BaseItem, int, int> OnSaleCompleted;

    // INIT
    private void Awake()
    {
        _state = new MarketState { villageId = villageId };
        _state.Init(items);
        _state.InitSell(sellsToPlayer);
    }

    private void OnEnable()  => TimeManager.OnNewDay += HandleNewDay;
    private void OnDisable() => TimeManager.OnNewDay -= HandleNewDay;

    private void HandleNewDay() => _state.AdvanceDay(TimeManager.TotalDays, sellsToPlayer);

    public VillagePhase CurrentPhase => VillageProgressionManager.Instance != null
        ? VillageProgressionManager.Instance.GetPhase(villageId)
        : VillagePhase.Trust;

    private VillageItemConfig FindConfig(BaseItem item)
        => items.Find(c => c != null && c.item == item);

    private VillageStockItem FindSellConfig(BaseItem item)
        => sellsToPlayer.Find(c => c != null && c.item == item);

    public bool Buys(BaseItem item)
    {
        var cfg = FindConfig(item);
        return cfg != null && CurrentPhase >= cfg.availableFromPhase;
    }

    public int GetSellPrice(BaseItem item)
    {
        var cfg = FindConfig(item);
        if (cfg == null || !_state.items.TryGetValue(item, out var data)) return 0;
        return EconomicSimulator.CalculateSellPrice(
            cfg.SpecialisedBasePrice, cfg.tier, data.currentStock, data.basketSize,
            _state.currentDay - data.lastSaleDay);
    }

    public bool Sells(BaseItem item)
    {
        var cfg = FindSellConfig(item);
        if (cfg == null || CurrentPhase < cfg.availableFromPhase) return false;
        return _state.RemainingSellStock(item) > 0;
    }

    public int GetBuyPrice(BaseItem item)
    {
        var cfg = FindSellConfig(item);
        if (cfg == null || !_state.sellStock.TryGetValue(item, out var s)) return 0;
        return EconomicSimulator.CalculateBuyPrice(cfg.BasePrice, cfg.tier, s.remainingToday, cfg.dailyStock);
    }

    public List<VillageStockItem> GetItemsForSaleNow()
    {
        var list = new List<VillageStockItem>();
        foreach (var cfg in sellsToPlayer)
            if (cfg != null && cfg.item != null && Sells(cfg.item)) list.Add(cfg);
        return list;
    }

    public int RegisterSale(BaseItem item, int qty)
    {
        if (item == null || qty <= 0) return 0;
        if (!Buys(item)) return 0;
        if (!_state.CanSell(item, qty)) return 0;

        int unit = GetSellPrice(item);
        int baseCoins = unit * qty;

        int bonus = DemandEventManager.Instance != null
            ? DemandEventManager.Instance.ApplyDemandBonus(villageId, item, qty, unit)
            : 0;

        _state.RecordSale(item, qty);
        int total = baseCoins + bonus;

        OnSaleCompleted?.Invoke(this, item, qty, total);

        VillageProgressionManager.Instance?.RecordSale(villageId, item, qty);
        MerchantJournal.Instance?.RecordIncome(total);
        return total;
    }

    public bool SellFromInventory(BaseItem item, int qty)
    {
        if (InventoryManager.Instance == null || item == null || qty <= 0) return false;
        if (InventoryManager.Instance.CountItem(item) < qty) return false;

        int coins = RegisterSale(item, qty);
        if (coins <= 0) return false;

        InventoryManager.Instance.RemoveItem(item, qty);
        InventoryManager.AddToken(coins);
        return true;
    }

    public bool BuyFromVillage(BaseItem item, int qty)
    {
        if (InventoryManager.Instance == null || item == null || qty <= 0) return false;
        if (!Sells(item)) return false;
        if (_state.RemainingSellStock(item) < qty) return false;

        int cost = Mathf.Abs(GetBuyPrice(item) * qty);
        if (InventoryManager.token < cost) return false;

        if (!_state.TryConsumeSellStock(item, qty)) return false;
        InventoryManager.SpendToken(cost);
        MerchantJournal.Instance?.RecordExpense(cost);
        for (int i = 0; i < qty; i++) InventoryManager.Instance.AddItem(item);
        return true;
    }
}
