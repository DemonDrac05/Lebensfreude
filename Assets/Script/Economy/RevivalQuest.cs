using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ItemAmount
{
    public BaseItem item;
    public int amount;
}

[Serializable]
public class RevivalQuest
{
    public List<ItemAmount> requiredItems = new();

    [Tooltip("Token cost paid to revive, IN ADDITION to materials")]
    public int requiredCoins = 0;

    [NonSerialized] public Dictionary<BaseItem, int> delivered = new();

    public void AddDelivery(BaseItem item, int qty)
    {
        if (item == null) return;
        delivered.TryGetValue(item, out int cur);
        delivered[item] = cur + qty;
    }

    public bool IsComplete()
    {
        foreach (var req in requiredItems)
        {
            if (req == null || req.item == null) continue;
            delivered.TryGetValue(req.item, out int have);
            if (have < req.amount) return false;
        }
        return requiredItems.Count > 0;
    }
}
