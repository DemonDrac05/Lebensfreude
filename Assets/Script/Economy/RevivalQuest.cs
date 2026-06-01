using System;
using System.Collections.Generic;
using UnityEngine;

// ─────────────────────────────────────────
// ITEM AMOUNT  (cặp vật phẩm + số lượng)
// ─────────────────────────────────────────
// Dùng chung cho ngưỡng phase và revival quest. Điền trong Inspector.
[Serializable]
public class ItemAmount
{
    public BaseItem item;
    public int amount;
}

// ─────────────────────────────────────────
// REVIVAL QUEST  (đơn hàng hồi sinh làng — phase cuối)
// ─────────────────────────────────────────
// Dữ liệu: danh sách món cần giao + theo dõi tiến độ. Khi đủ -> làng được hồi sinh, thưởng Artifact.
// Dùng trong: VillageProgressionManager (gate Partnership -> Revival).
[Serializable]
public class RevivalQuest
{
    public List<ItemAmount> requiredItems = new();

    [Tooltip("Phí tiền (token) phải trả khi hồi sinh, NGOÀI nguyên liệu")]
    public int requiredCoins = 0;

    // Tiến độ đã giao cho từng món (key theo BaseItem). Không hiện trong Inspector.
    [NonSerialized] public Dictionary<BaseItem, int> delivered = new();

    // Cộng tiến độ giao 1 món. Dùng trong: VillageProgressionManager.RecordSale().
    public void AddDelivery(BaseItem item, int qty)
    {
        if (item == null) return;
        delivered.TryGetValue(item, out int cur);
        delivered[item] = cur + qty;
    }

    // Đã giao đủ toàn bộ đơn chưa? Dùng trong: VillageProgressionManager.TryAdvance().
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
