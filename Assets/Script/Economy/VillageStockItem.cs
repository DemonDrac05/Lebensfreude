using System;
using UnityEngine;

// ─────────────────────────────────────────
// VILLAGE STOCK ITEM  (cấu hình 1 món LÀNG BÁN cho player)
// ─────────────────────────────────────────
// Đối xứng với VillageItemConfig (hàng làng mua). Điền List trong Inspector trên VillageMarket.
// Mỗi entry: SO vật phẩm + nhóm co giãn + giá gốc + phase mở + hạn mức/ngày + xác suất xuất hiện.
// Dùng trong: VillageMarket (sellsToPlayer), MarketState (sellStock).
[Serializable]
public class VillageStockItem
{
    [Header("=== Tham chiếu vật phẩm ==========")]
    public BaseItem item;                                  // Reagent, Flux Powder, Coal, Legendary Recipe...

    [Header("=== Tham số kinh tế ==========")]
    public ElasticityTier tier = ElasticityTier.Metal;     // độ co giãn -> EconomicSimulator.CalculateBuyPrice
    [Tooltip("-1 = dùng buyingPrice của chính BaseItem")]
    public int basePriceOverride = -1;

    [Header("=== Mở khoá & khan hiếm ==========")]
    public VillagePhase availableFromPhase = VillagePhase.Partnership; // hàng đặc biệt thường mở Phase 2
    [Tooltip("Số lượng tối đa bán ra MỖI NGÀY")]
    public int dailyStock = 5;
    [Range(0f, 1f)]
    [Tooltip("Xác suất món XUẤT HIỆN để bán trong ngày (1 = luôn có)")]
    public float appearanceChance = 1f;

    // Giá gốc thực dùng (ưu tiên override, nếu không lấy buyingPrice của SO).
    public int BasePrice => basePriceOverride >= 0
        ? basePriceOverride
        : (item != null ? item.buyingPrice : 0);
}
