using System;
using UnityEngine;

//
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

    private const string UNLOCK_KEY = "MerchantJournal_Unlocked";
    public bool IsUnlocked => PlayerPrefs.GetInt(UNLOCK_KEY, 0) == 1;

    public void Unlock()
    {
        PlayerPrefs.SetInt(UNLOCK_KEY, 1);
        PlayerPrefs.Save();
        OnJournalUpdated?.Invoke();
    }

    private int _earnedToday;
    private int _spentToday;
    private int _bestTradeToday;

    public event Action OnJournalUpdated;

    public void RecordIncome(int amount)
    {
        if (amount <= 0) return;
        _earnedToday += amount;
        if (amount > _bestTradeToday) _bestTradeToday = amount;
        OnJournalUpdated?.Invoke();
    }

    public void RecordExpense(int amount)
    {
        if (amount <= 0) return;
        _spentToday += amount;
        OnJournalUpdated?.Invoke();
    }

    private void ResetDaily()
    {
        _earnedToday = 0;
        _spentToday = 0;
        _bestTradeToday = 0;
        OnJournalUpdated?.Invoke();
    }

    // QUERY  (cho UI)
    public int EarnedToday    => _earnedToday;
    public int SpentToday     => _spentToday;
    public int NetToday       => _earnedToday - _spentToday;
    public int BestTradeToday => _bestTradeToday;
    public int DaysElapsed    => TimeManager.TotalDays;
    public int Coins          => InventoryManager.token;
    public int NetWorth       => InventoryManager.token + EstimateInventoryValue();
    public string EndingTierProjection => TierTitle(TimeManager.TotalDays);

    private int EstimateInventoryValue()
    {
        var inv = InventoryManager.Instance;
        if (inv == null) return 0;

        int total = SumSlots(inv.ToolbarSlots);
        if (inv.MainInventorySlots != null)
        {
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
        if (bi == null || bi.sellingPrice <= 0) return 0;
        return bi.sellingPrice * it.count;
    }

    private string TierTitle(int days)
    {
        if (days <= 60)  return "Legendary Merchant";
        if (days <= 100) return "Master Trader";
        if (days <= 150) return "Skilled Merchant";
        return "Wandering Merchant";
    }
}
