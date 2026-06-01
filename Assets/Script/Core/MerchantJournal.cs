using System;
using UnityEngine;

// ─────────────────────────────────────────
// MERCHANT JOURNAL  (sổ kế toán: P&L / net worth / tiến độ)
// ─────────────────────────────────────────
// Singleton SỐNG QUA SCENE. Ghi nhận thu/chi (VillageMarket gọi trực tiếp -> không miss),
// reset theo ngày (TimeManager.OnNewDay), tính net worth bằng cách quét kho.
// MỞ KHÓA 1 LẦN DUY NHẤT & MÃI MÃI qua lần đầu craft (PlayerPrefs). UI do bạn tự build,
// nghe event OnJournalUpdated để cập nhật.
//
// Liên kết: VillageMarket (RecordIncome/RecordExpense), TimeManager (ngày), InventoryManager (token + kho).
public class MerchantJournal : MonoBehaviour
{
    public static MerchantJournal Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()  => TimeManager.OnNewDay += ResetDaily;
    private void OnDisable() => TimeManager.OnNewDay -= ResetDaily;

    // ─────────────────────────────────────────
    // UNLOCK  (1 lần, vĩnh viễn — qua lần đầu craft Journal)
    // ─────────────────────────────────────────
    private const string UNLOCK_KEY = "MerchantJournal_Unlocked";
    public bool IsUnlocked => PlayerPrefs.GetInt(UNLOCK_KEY, 0) == 1;

    // Gọi 1 lần khi craft Journal lần đầu (CraftingManager xóa luôn công thức sau đó). Dùng trong: crafting.
    public void Unlock()
    {
        PlayerPrefs.SetInt(UNLOCK_KEY, 1);
        PlayerPrefs.Save();
        OnJournalUpdated?.Invoke();
    }

    // ─────────────────────────────────────────
    // P&L STATE  (theo NGÀY)
    // ─────────────────────────────────────────
    private int _earnedToday;
    private int _spentToday;
    private int _bestTradeToday;

    public event Action OnJournalUpdated;

    // Ghi thu nhập 1 lần bán. Dùng trong: VillageMarket.RegisterSale().
    public void RecordIncome(int amount)
    {
        if (amount <= 0) return;
        _earnedToday += amount;
        if (amount > _bestTradeToday) _bestTradeToday = amount;
        OnJournalUpdated?.Invoke();
    }

    // Ghi chi tiêu 1 lần mua. Dùng trong: VillageMarket.BuyFromVillage().
    public void RecordExpense(int amount)
    {
        if (amount <= 0) return;
        _spentToday += amount;
        OnJournalUpdated?.Invoke();
    }

    // Sang ngày -> reset P&L ngày. Dùng trong: TimeManager.OnNewDay.
    private void ResetDaily()
    {
        _earnedToday = 0;
        _spentToday = 0;
        _bestTradeToday = 0;
        OnJournalUpdated?.Invoke();
    }

    // ─────────────────────────────────────────
    // QUERY  (cho UI)
    // ─────────────────────────────────────────
    public int EarnedToday    => _earnedToday;
    public int SpentToday     => _spentToday;
    public int NetToday       => _earnedToday - _spentToday;
    public int BestTradeToday => _bestTradeToday;
    public int DaysElapsed    => TimeManager.TotalDays;
    public int Coins          => InventoryManager.token;
    public int NetWorth       => InventoryManager.token + EstimateInventoryValue();
    public string EndingTierProjection => TierTitle(TimeManager.TotalDays);

    // Ước lượng giá trị kho = Σ count × sellingPrice (bỏ qua món không bán được, vd Artifact giá -1).
    private int EstimateInventoryValue()
    {
        var inv = InventoryManager.Instance;
        if (inv == null) return 0;

        int total = SumSlots(inv.ToolbarSlots);
        if (inv.MainInventorySlots != null)
        {
            // Chỉ tính SecondSlots (index >= 12); 0..11 là bản mirror của toolbar -> tránh đếm trùng.
            for (int i = 12; i < inv.MainInventorySlots.Length; i++)
                total += SlotValue(inv.MainInventorySlots[i]);
        }
        return total;
    }

    private int SumSlots(BaseSlot[] slots)
    {
        int total = 0;
        if (slots == null) return 0;
        foreach (var s in slots) total += SlotValue(s);
        return total;
    }

    private int SlotValue(BaseSlot slot)
    {
        if (slot == null) return 0;
        var it = slot.GetComponentInChildren<InventoryItem>();
        if (it == null) return 0;
        var bi = it.GetItem<BaseItem>();
        if (bi == null || bi.sellingPrice <= 0) return 0; // bỏ món không bán được
        return bi.sellingPrice * it.count;
    }

    // Tier dự kiến theo số ngày (khớp EndingManager). Dùng trong: EndingTierProjection.
    private string TierTitle(int days)
    {
        if (days <= 60)  return "Legendary Merchant";
        if (days <= 100) return "Master Trader";
        if (days <= 150) return "Skilled Merchant";
        return "Wandering Merchant";
    }
}
