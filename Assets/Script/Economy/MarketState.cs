using System;
using System.Collections.Generic;
using UnityEngine;

// ─────────────────────────────────────────
// ITEM MARKET DATA  (trạng thái tồn kho 1 món — chiều PLAYER BÁN cho làng)
// ─────────────────────────────────────────
[Serializable]
public class ItemMarketData
{
    public int currentStock;   // tồn kho hiện tại (người chơi bán vào -> tăng)
    public int basketSize;     // rổ tham chiếu cố định
    public int lastSaleDay;    // ngày (TotalDays) của lần bán gần nhất
    public int dailySold;      // số đã bán trong NGÀY hiện tại (giới hạn giao dịch)
}

// ─────────────────────────────────────────
// SELL STOCK DATA  (trạng thái 1 món — chiều LÀNG BÁN cho player)
// ─────────────────────────────────────────
[Serializable]
public class SellStockData
{
    public bool inStockToday;     // hôm nay có bày bán không (random theo appearanceChance)
    public int  remainingToday;   // số còn lại để bán hôm nay (giảm dần khi player mua)
    public int  dailyStock;       // hạn mức nạp lại mỗi ngày
}

// ─────────────────────────────────────────
// MARKET STATE  (trạng thái chợ runtime của 1 làng — cả 2 chiều)
// ─────────────────────────────────────────
// Giữ tồn kho theo từng BaseItem (key tham chiếu SO). KHÔNG phải MonoBehaviour: do VillageMarket sở hữu.
// Dùng trong: VillageMarket (Awake init, RegisterSale, BuyFromVillage, AdvanceDay).
public class MarketState
{
    // ─────────────────────────────────────────
    // DATA
    // ─────────────────────────────────────────
    public VillageId villageId;
    public int currentDay;                  // đồng bộ từ TimeManager.TotalDays
    public int dailyTransactionLimit = 20;  // trần giao dịch / ngày (chống farm 1 món)
    public readonly Dictionary<BaseItem, ItemMarketData> items     = new(); // chiều player BÁN
    public readonly Dictionary<BaseItem, SellStockData>  sellStock = new(); // chiều làng BÁN

    // ─────────────────────────────────────────
    // INIT (chiều player bán) — tồn kho khởi điểm = basketSize
    // ─────────────────────────────────────────
    public void Init(List<VillageItemConfig> configs)
    {
        items.Clear();
        foreach (var cfg in configs)
        {
            if (cfg == null || cfg.item == null) continue; // bỏ entry trống -> tránh null
            items[cfg.item] = new ItemMarketData
            {
                currentStock = cfg.basketSize,
                basketSize   = cfg.basketSize,
                lastSaleDay  = -10,   // âm để lần bán đầu có R(t) ≈ 1 (giá đầy đủ)
                dailySold    = 0
            };
        }
    }

    // ─────────────────────────────────────────
    // INIT SELL (chiều làng bán) — tạo ô tồn + roll cho ngày đầu
    // ─────────────────────────────────────────
    public void InitSell(List<VillageStockItem> configs)
    {
        sellStock.Clear();
        if (configs == null) return;
        foreach (var cfg in configs)
        {
            if (cfg == null || cfg.item == null) continue;
            sellStock[cfg.item] = new SellStockData
            {
                inStockToday   = false,
                remainingToday = 0,
                dailyStock     = cfg.dailyStock
            };
        }
        RollDailySellStock(configs);
    }

    // ─────────────────────────────────────────
    // ROLL DAILY SELL STOCK — mỗi ngày random món xuất hiện + nạp lại hạn mức
    // ─────────────────────────────────────────
    // Dùng trong: InitSell (ngày đầu) và AdvanceDay (mỗi ngày mới).
    public void RollDailySellStock(List<VillageStockItem> configs)
    {
        if (configs == null) return;
        foreach (var cfg in configs)
        {
            if (cfg == null || cfg.item == null) continue;
            if (!sellStock.TryGetValue(cfg.item, out var s)) continue;
            s.inStockToday   = UnityEngine.Random.value <= cfg.appearanceChance; // % xuất hiện
            s.dailyStock     = cfg.dailyStock;
            s.remainingToday = s.inStockToday ? cfg.dailyStock : 0;
        }
    }

    // ─────────────────────────────────────────
    // TRADE (chiều player bán)
    // ─────────────────────────────────────────
    // Còn hạn mức bán trong ngày không? Dùng trong: VillageMarket.RegisterSale().
    public bool CanSell(BaseItem item, int amount)
    {
        if (item == null || !items.TryGetValue(item, out var data)) return false;
        return data.dailySold + amount <= dailyTransactionLimit;
    }

    // Ghi nhận bán -> tồn kho tăng, giá giảm. Dùng trong: VillageMarket.RegisterSale().
    public void RecordSale(BaseItem item, int amount)
    {
        if (item == null || !items.TryGetValue(item, out var data)) return;
        data.currentStock += amount;
        data.dailySold    += amount;
        data.lastSaleDay   = currentDay;
    }

    // ─────────────────────────────────────────
    // TRADE (chiều làng bán)
    // ─────────────────────────────────────────
    // Số còn bán được của 1 món hôm nay (0 nếu không bày bán). Dùng trong: VillageMarket.GetBuyPrice/Sells.
    public int RemainingSellStock(BaseItem item)
        => sellStock.TryGetValue(item, out var s) && s.inStockToday ? s.remainingToday : 0;

    // Trừ kho khi player mua. True nếu đủ. Dùng trong: VillageMarket.BuyFromVillage().
    public bool TryConsumeSellStock(BaseItem item, int amount)
    {
        if (item == null || !sellStock.TryGetValue(item, out var s)) return false;
        if (!s.inStockToday || s.remainingToday < amount) return false;
        s.remainingToday -= amount;
        return true;
    }

    // ─────────────────────────────────────────
    // ADVANCE DAY — sang ngày mới: reset hạn mức bán, hồi tồn kho, roll lại hàng làng bán
    // ─────────────────────────────────────────
    // Gọi từ VillageMarket.HandleNewDay() (nghe TimeManager.OnNewDay).
    public void AdvanceDay(int newDay, List<VillageStockItem> sellConfigs = null)
    {
        currentDay = newDay;

        // Chiều player bán: reset hạn mức + hồi tồn kho 15%/ngày -> giá dần phục hồi.
        foreach (var data in items.Values)
        {
            data.dailySold = 0;
            int recovery = Mathf.RoundToInt(data.basketSize * 0.15f);
            data.currentStock = Mathf.Max(0, data.currentStock - recovery);
        }

        // Chiều làng bán: roll lại món xuất hiện + nạp lại hạn mức ngày.
        if (sellConfigs != null) RollDailySellStock(sellConfigs);
    }
}
