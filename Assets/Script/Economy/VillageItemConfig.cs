using System;
using UnityEngine;

// ─────────────────────────────────────────
// VILLAGE ITEM CONFIG  (cấu hình 1 món hàng của làng)
// ─────────────────────────────────────────
// Lớp [Serializable] để điền TRỰC TIẾP trong Inspector dưới dạng List trên VillageMarket
// (đúng hướng "tạo list/array để bỏ vô" — main dùng ScriptableObject thay cho JSON của ref).
//
// Mỗi entry nối 1 BaseItem (SO có sẵn của main) với các tham số kinh tế.
// Dùng trong: VillageMarket (danh sách hàng làng mua/bán), MarketState (khởi tạo tồn kho).
[Serializable]
public class VillageItemConfig
{
    [Header("=== Tham chiếu vật phẩm ==========")]
    public BaseItem item;                               // SO vật phẩm trong main (Wood Plank, Iron Bar...)

    [Header("=== Tham số kinh tế ==========")]
    public ElasticityTier tier = ElasticityTier.Basic;  // nhóm co giãn -> EconomicSimulator.GetElasticity
    [Tooltip("-1 = dùng sellingPrice của chính BaseItem")]
    public int basePriceOverride = -1;                  // cho phép ghi đè giá gốc nếu muốn
    [Tooltip("Rổ tham chiếu: tồn kho ban đầu, càng nhỏ giá càng nhạy")]
    public int basketSize = 10;

    [Header("=== Mở khoá theo phase ==========")]
    public VillagePhase availableFromPhase = VillagePhase.Trust; // làng mua/bán món này từ phase nào

    // Giá gốc thực dùng: ưu tiên override, nếu không thì lấy sellingPrice của SO.
    // Dùng trong: MarketState, VillageMarket.GetSellPrice().
    public int BasePrice => basePriceOverride >= 0
        ? basePriceOverride
        : (item != null ? item.sellingPrice : 0);
}
