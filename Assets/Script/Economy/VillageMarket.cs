using System;
using System.Collections.Generic;
using UnityEngine;

// ─────────────────────────────────────────
// VILLAGE MARKET  (chợ của 1 làng — mua & bán đều giá động)
// ─────────────────────────────────────────
// Gắn lên ROOT GameObject của làng. Giữ 2 list điền trong Inspector:
//   • items        = hàng làng MUA của player (giá rớt khi bán nhiều).
//   • sellsToPlayer = hàng làng BÁN cho player (giá tăng khi khan hiếm, có hạn mức + % xuất hiện/ngày).
// Nghe TimeManager.OnNewDay để sang ngày. Tương thích ngược với shop clone sẵn có (RegisterSale).
//
// Liên kết: EconomicSimulator (giá 2 chiều), MarketState (tồn kho), DemandEventManager (thưởng demand),
//           VillageProgressionManager (phase + tiến độ), InventoryManager (token + kho), TimeManager (ngày).
public class VillageMarket : MonoBehaviour
{
    // ─────────────────────────────────────────
    // CONFIG  (điền trong Inspector)
    // ─────────────────────────────────────────
    [Header("=== Định danh làng ==========")]
    public VillageId villageId;
    public string displayName = "Sylvan";

    [Header("=== Hàng làng MUA của player (player bán) ==========")]
    [SerializeField] private List<VillageItemConfig> items = new();

    [Header("=== Hàng làng BÁN cho player (player mua) ==========")]
    [SerializeField] private List<VillageStockItem> sellsToPlayer = new();

    // ─────────────────────────────────────────
    // STATE
    // ─────────────────────────────────────────
    private MarketState _state;
    public MarketState State => _state;
    public IReadOnlyList<VillageItemConfig> Items => items;
    public IReadOnlyList<VillageStockItem>  SellsToPlayer => sellsToPlayer;

    // Sự kiện sau mỗi lần PLAYER BÁN thành công: (chợ, món, số lượng, tổng coins).
    // Nghe bởi: VillageProgressionManager, MerchantJournal.
    public event Action<VillageMarket, BaseItem, int, int> OnSaleCompleted;

    // ─────────────────────────────────────────
    // INIT
    // ─────────────────────────────────────────
    private void Awake()
    {
        _state = new MarketState { villageId = villageId };
        _state.Init(items);
        _state.InitSell(sellsToPlayer);
    }

    private void OnEnable()  => TimeManager.OnNewDay += HandleNewDay;
    private void OnDisable() => TimeManager.OnNewDay -= HandleNewDay;

    // Sang ngày mới: cập nhật tồn kho + hạn mức + roll lại hàng bán theo TimeManager.TotalDays.
    private void HandleNewDay() => _state.AdvanceDay(TimeManager.TotalDays, sellsToPlayer);

    // ─────────────────────────────────────────
    // PHASE  (gating theo tiến độ hồi sinh làng)
    // ─────────────────────────────────────────
    public VillagePhase CurrentPhase => VillageProgressionManager.Instance != null
        ? VillageProgressionManager.Instance.GetPhase(villageId)
        : VillagePhase.Trust;

    private VillageItemConfig FindConfig(BaseItem item)
        => items.Find(c => c != null && c.item == item);

    private VillageStockItem FindSellConfig(BaseItem item)
        => sellsToPlayer.Find(c => c != null && c.item == item);

    // ─────────────────────────────────────────
    // QUERY — CHIỀU PLAYER BÁN
    // ─────────────────────────────────────────
    // Làng có MUA món này ở phase hiện tại không? Dùng trong: ShopSlot/MarketUI.
    public bool Buys(BaseItem item)
    {
        var cfg = FindConfig(item);
        return cfg != null && CurrentPhase >= cfg.availableFromPhase;
    }

    // Giá làng TRẢ cho 1 đơn vị (động). Dùng trong: MarketOverviewUI, ShopSlot.
    public int GetSellPrice(BaseItem item)
    {
        var cfg = FindConfig(item);
        if (cfg == null || !_state.items.TryGetValue(item, out var data)) return 0;
        return EconomicSimulator.CalculateSellPrice(
            cfg.BasePrice, cfg.tier, data.currentStock, data.basketSize,
            _state.currentDay - data.lastSaleDay);
    }

    // ─────────────────────────────────────────
    // QUERY — CHIỀU LÀNG BÁN
    // ─────────────────────────────────────────
    // Làng có BÀY BÁN món này hôm nay (đúng phase + còn hàng) không? Dùng trong: Market/Shop buy UI.
    public bool Sells(BaseItem item)
    {
        var cfg = FindSellConfig(item);
        if (cfg == null || CurrentPhase < cfg.availableFromPhase) return false;
        return _state.RemainingSellStock(item) > 0;
    }

    // Giá player PHẢI TRẢ cho 1 đơn vị (động theo khan hiếm). Dùng trong: buy UI.
    public int GetBuyPrice(BaseItem item)
    {
        var cfg = FindSellConfig(item);
        if (cfg == null || !_state.sellStock.TryGetValue(item, out var s)) return 0;
        return EconomicSimulator.CalculateBuyPrice(cfg.BasePrice, cfg.tier, s.remainingToday, cfg.dailyStock);
    }

    // Danh sách món đang BÀY BÁN hôm nay (đã lọc theo phase + còn hàng). Dùng trong: buy UI để render.
    public List<VillageStockItem> GetItemsForSaleNow()
    {
        var list = new List<VillageStockItem>();
        foreach (var cfg in sellsToPlayer)
            if (cfg != null && cfg.item != null && Sells(cfg.item)) list.Add(cfg);
        return list;
    }

    // ─────────────────────────────────────────
    // SELL — đường 1: GHI NHẬN cho shop clone sẵn có (player bán)
    // ─────────────────────────────────────────
    // Tính tổng tiền (gồm demand bonus) + ghi nhận để giá rớt. KHÔNG cộng token/gỡ item (ShopSlot tự làm).
    // Trả 0 nếu không bán được. Dùng trong: ShopSlot.SellCachedItem.
    public int RegisterSale(BaseItem item, int qty)
    {
        if (item == null || qty <= 0) return 0;
        if (!Buys(item)) return 0;
        if (!_state.CanSell(item, qty)) return 0;   // vượt hạn mức ngày

        int unit = GetSellPrice(item);
        int baseCoins = unit * qty;

        int bonus = DemandEventManager.Instance != null
            ? DemandEventManager.Instance.ApplyDemandBonus(villageId, item, qty, unit)
            : 0;

        _state.RecordSale(item, qty);
        int total = baseCoins + bonus;

        OnSaleCompleted?.Invoke(this, item, qty, total);

        // Báo tiến độ hồi sinh làng — gọi trực tiếp để KHÔNG miss sự kiện qua scene.
        VillageProgressionManager.Instance?.RecordSale(villageId, item, qty);
        MerchantJournal.Instance?.RecordIncome(total); // ghi sổ thu nhập
        return total;
    }

    // ─────────────────────────────────────────
    // SELL — đường 2: bán TRỰC TIẾP từ kho (player bán, cho Market UI mới)
    // ─────────────────────────────────────────
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

    // ─────────────────────────────────────────
    // BUY — player MUA hàng làng bán
    // ─────────────────────────────────────────
    // Kiểm tra phase + còn hàng + đủ token -> trừ kho ngày + trừ token + thêm item vào kho. True nếu mua xong.
    // Dùng trong: Market/Shop buy UI (nút Mua).
    public bool BuyFromVillage(BaseItem item, int qty)
    {
        if (InventoryManager.Instance == null || item == null || qty <= 0) return false;
        if (!Sells(item)) return false;
        if (_state.RemainingSellStock(item) < qty) return false;

        int cost = GetBuyPrice(item) * qty;
        if (InventoryManager.token < cost) return false;

        if (!_state.TryConsumeSellStock(item, qty)) return false;   // trừ kho trước
        InventoryManager.SpendToken(cost);
        MerchantJournal.Instance?.RecordExpense(cost); // ghi sổ chi tiêu
        for (int i = 0; i < qty; i++) InventoryManager.Instance.AddItem(item);
        return true;
    }
}
