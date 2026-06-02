using System.Collections.Generic;
using TMPro;
using UnityEngine;

// ─────────────────────────────────────────
// VILLAGE MARKET UI  (1 panel dùng chung cho cả 3 làng)
// ─────────────────────────────────────────
// VillageMarketInteractor.Open(market) -> hiện 2 bảng: SELL (hàng làng MUA của player) + BUY (hàng làng BÁN),
// + dòng demand event. Bảng trống nếu phase chưa mở (Buys/Sells tự lọc theo phase). Giá là GIÁ ĐỘNG.
// Refresh sau mỗi giao dịch (OnSaleCompleted) và mỗi ngày mới (OnNewDay -> giá/đơn đổi).
//
// Liên kết: VillageMarket (Items/SellsToPlayer/Buys/Sells/GetSellPrice/GetBuyPrice/SellFromInventory/BuyFromVillage),
//           DemandEventManager (GetActiveEvent), MarketSellSlot/MarketBuySlot, InputManager (toolbar).
public class VillageMarketUI : MonoBehaviour
{
    [Header("=== Bảng SELL (làng mua của player) ==========")]
    [SerializeField] private Transform  sellContainer;
    [SerializeField] private GameObject sellSlotPrefab;

    [Header("=== Bảng BUY (làng bán cho player) ==========")]
    [SerializeField] private Transform  buyContainer;
    [SerializeField] private GameObject buySlotPrefab;

    [Header("=== Header / Demand ==========")]
    [SerializeField] private TextMeshProUGUI villageNameText;
    [SerializeField] private TextMeshProUGUI phaseText;
    [SerializeField] private TextMeshProUGUI coinsText;
    [SerializeField] private TextMeshProUGUI demandText;

    private VillageMarket _market;
    private readonly List<MarketSellSlot> _sellSlots = new();
    private readonly List<MarketBuySlot>  _buySlots  = new();

    public static VillageMarketUI CurrentOpen { get; private set; }
    public bool IsOpen => gameObject.activeSelf;

    // ─────────────────────────────────────────
    // OPEN / CLOSE
    // ─────────────────────────────────────────
    public void Open(VillageMarket market)
    {
        if (market == null) return;
        if (CurrentOpen != null && CurrentOpen != this) CurrentOpen.Close();
        _market = market; gameObject.SetActive(true);
    }
    public void Close() => gameObject.SetActive(false);
    public static void CloseIfOpen() { if (CurrentOpen != null && CurrentOpen.IsOpen) CurrentOpen.Close(); }
    public bool IsShowing(VillageMarket m) => _market == m;

    private void OnEnable()
    {
        CurrentOpen = this;
        if (InputManager.Instance != null) InputManager.Instance.toolBar.SetActive(false);
        if (_market != null) _market.OnSaleCompleted += OnSale;
        TimeManager.OnNewDay += RefreshAll;
        Populate();
    }
    private void OnDisable()
    {
        if (CurrentOpen == this) CurrentOpen = null;
        if (InputManager.Instance != null) InputManager.Instance.toolBar.SetActive(true);
        if (_market != null) _market.OnSaleCompleted -= OnSale;
        TimeManager.OnNewDay -= RefreshAll;
        ClearAll(); _market = null;
    }

    private void OnSale(VillageMarket m, BaseItem i, int q, int c) => RefreshAll();

    // ─────────────────────────────────────────
    // POPULATE / REFRESH
    // ─────────────────────────────────────────
    // Dựng lại 2 bảng (giá/stock đổi sau mỗi giao dịch nên build lại cho chắc). Dùng trong: OnEnable, RefreshAll.
    private void Populate()
    {
        ClearAll();
        if (_market == null) return;

        // SELL: các món làng MUA ở phase hiện tại
        if (sellContainer != null && sellSlotPrefab != null)
            foreach (var cfg in _market.Items)
            {
                if (cfg == null || cfg.item == null || !_market.Buys(cfg.item)) continue;
                var go = Instantiate(sellSlotPrefab, sellContainer);
                var slot = go.GetComponent<MarketSellSlot>();
                if (slot == null) { Destroy(go); continue; }
                slot.Setup(cfg.item, _market, this); _sellSlots.Add(slot);
            }

        // BUY: các món làng BÁN hôm nay (đúng phase + còn hàng)
        if (buyContainer != null && buySlotPrefab != null)
            foreach (var cfg in _market.GetItemsForSaleNow())
            {
                if (cfg == null || cfg.item == null) continue;
                var go = Instantiate(buySlotPrefab, buyContainer);
                var slot = go.GetComponent<MarketBuySlot>();
                if (slot == null) { Destroy(go); continue; }
                slot.Setup(cfg.item, _market, this); _buySlots.Add(slot);
            }

        RefreshHeader();
    }

    // Gọi từ slot sau mỗi mua/bán. Build lại để giá động + stock cập nhật chuẩn.
    public void RefreshAll()
    {
        Populate();
    }

    private void RefreshHeader()
    {
        if (_market == null) return;
        if (villageNameText != null) villageNameText.text = _market.displayName;
        if (phaseText != null)       phaseText.text = "Phase: " + _market.CurrentPhase;
        if (coinsText != null)       coinsText.text = InventoryManager.token.ToString();
        if (demandText != null)
        {
            var evt = DemandEventManager.Instance != null ? DemandEventManager.Instance.GetActiveEvent(_market.villageId) : null;
            demandText.text = (evt != null && evt.item != null)
                ? $"⭐ Đơn: {evt.item.name} ×{evt.requiredAmount} (đã {evt.filledAmount}) — thưởng {evt.bonusMultiplier:0.#}× — hạn ngày {evt.deadlineDay}"
                : "Hôm nay không có đơn đặc biệt.";
        }
    }

    private void ClearAll()
    {
        foreach (var s in _sellSlots) if (s != null) Destroy(s.gameObject);
        foreach (var s in _buySlots)  if (s != null) Destroy(s.gameObject);
        _sellSlots.Clear(); _buySlots.Clear();
    }
}
