using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ItemMarketData
{
    public int currentStock;
    public int basketSize;
    public int lastSaleDay;
    public int dailySold;
}

[Serializable]
public class SellStockData
{
    public bool inStockToday;
    public int  remainingToday;
    public int  dailyStock;
}

public class MarketState
{
    // DATA
    public VillageId villageId;
    public int currentDay;
    public int dailyTransactionLimit = 20;
    public readonly Dictionary<BaseItem, ItemMarketData> items     = new();
    public readonly Dictionary<BaseItem, SellStockData>  sellStock = new();

    public void Init(List<VillageItemConfig> configs)
    {
        items.Clear();
        foreach (var cfg in configs)
        {
            if (cfg == null || cfg.item == null) continue;
            items[cfg.item] = new ItemMarketData
            {
                currentStock = cfg.basketSize,
                basketSize   = cfg.basketSize,
                lastSaleDay  = -10,
                dailySold    = 0
            };
        }
    }

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

    public void RollDailySellStock(List<VillageStockItem> configs)
    {
        if (configs == null) return;
        foreach (var cfg in configs)
        {
            if (cfg == null || cfg.item == null) continue;
            if (!sellStock.TryGetValue(cfg.item, out var s)) continue;
            s.inStockToday   = UnityEngine.Random.value <= cfg.appearanceChance;
            s.dailyStock     = cfg.dailyStock;
            s.remainingToday = s.inStockToday ? cfg.dailyStock : 0;
        }
    }

    public bool CanSell(BaseItem item, int amount)
    {
        if (item == null || !items.TryGetValue(item, out var data)) return false;
        return data.dailySold + amount <= dailyTransactionLimit;
    }

    public void RecordSale(BaseItem item, int amount)
    {
        if (item == null || !items.TryGetValue(item, out var data)) return;
        data.currentStock  = Mathf.Max(0, data.currentStock - amount);
        data.dailySold    += amount;
        data.lastSaleDay   = currentDay;
    }

    public int RemainingSellStock(BaseItem item)
        => sellStock.TryGetValue(item, out var s) && s.inStockToday ? s.remainingToday : 0;

    public bool TryConsumeSellStock(BaseItem item, int amount)
    {
        if (item == null || !sellStock.TryGetValue(item, out var s)) return false;
        if (!s.inStockToday || s.remainingToday < amount) return false;
        s.remainingToday -= amount;
        return true;
    }

    public void AdvanceDay(int newDay, List<VillageStockItem> sellConfigs = null)
    {
        currentDay = newDay;

        foreach (var data in items.Values)
        {
            data.dailySold = 0;
            int recovery = Mathf.Max(1, Mathf.RoundToInt(data.basketSize * 0.15f));
            data.currentStock = Mathf.Min(data.basketSize, data.currentStock + recovery);
        }

        if (sellConfigs != null) RollDailySellStock(sellConfigs);
    }
}
