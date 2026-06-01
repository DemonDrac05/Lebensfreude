using UnityEngine;

// ─────────────────────────────────────────
// ECONOMIC SIMULATOR  (lõi tính giá động)
// ─────────────────────────────────────────
// Class TĨNH, thuần toán học, KHÔNG phụ thuộc scene/GameObject -> không bao giờ crash runtime.
// Đây là công thức gốc đã được hội đồng hỏi kỹ (vì sao 10%-300%) nên GIỮ NGUYÊN, chỉ bọc comment.
//
// Công thức:
//   Price(t) = BasePrice × (Stock / Basket)^ε × R(t)
//   R(t)     = 1 − 0.4 × e^(−0.3 × daysSinceSale)
//   Kết quả clamp trong [10% , 300%] của BasePrice.
//
// Vì sao clamp [10%, 300%]?
//   - Sàn 10%: nếu bán quá nhiều khiến tồn kho phình to, giá vẫn không rớt về 0
//     -> tránh "lỗ vô nghĩa" và giữ game còn chơi được (vẫn có động lực bán).
//   - Trần 300%: khi khan hiếm hoặc dính Demand Event, giá không tăng vô hạn
//     -> tránh người chơi farm 1 món duy nhất phá vỡ cân bằng kinh tế.
//
// Dùng trong: VillageMarket (GetSellPrice / RegisterSale), MarketOverviewUI (hiển thị giá).
public static class EconomicSimulator
{
    // ─────────────────────────────────────────
    // ELASTICITY TIERS  (ε theo nhóm hàng)
    // ─────────────────────────────────────────
    // Trả về hệ số co giãn ε. Số càng lớn -> bán nhiều giá rớt càng mạnh.
    // Dùng trong: CalculatePrice().
    public static float GetElasticity(ElasticityTier tier) => tier switch
    {
        ElasticityTier.Basic   => 0.2f,
        ElasticityTier.Metal   => 0.5f,
        ElasticityTier.Alloy   => 0.8f,
        ElasticityTier.Endgame => 1.2f,
        _                      => 0.5f
    };

    // ─────────────────────────────────────────
    // RECOVERY FACTOR  R(t)
    // ─────────────────────────────────────────
    // Hệ số hồi giá theo số ngày KỂ TỪ lần bán gần nhất.
    // daysSinceSale = 0 -> R = 0.6 (vừa bán xong, giá còn thấp);
    // càng nhiều ngày không bán -> R tiến dần về 1.0 (giá hồi phục).
    // Dùng trong: CalculatePrice().
    public static float RecoveryFactor(int daysSinceSale)
        => 1f - 0.4f * Mathf.Exp(-0.3f * daysSinceSale);

    // ─────────────────────────────────────────
    // PRICE CALCULATION  (giá 1 đơn vị)
    // ─────────────────────────────────────────
    // Tính giá hiện tại của 1 món dựa trên: giá gốc, tồn kho hiện tại của làng,
    // kích thước rổ tham chiếu, độ co giãn và số ngày từ lần bán gần nhất.
    // Dùng trong: CalculateSellPrice().
    public static float CalculatePrice(float basePrice, int currentStock,
                                       int basketSize, float epsilon, int daysSinceSale)
    {
        if (basketSize <= 0) return basePrice; // phòng chia 0

        float stockRatio = Mathf.Max(0.001f, (float)currentStock / basketSize);
        float raw = basePrice * Mathf.Pow(stockRatio, epsilon) * RecoveryFactor(daysSinceSale);

        // Clamp [10%, 300%] — xem giải thích ở đầu file.
        return Mathf.Clamp(raw, basePrice * 0.1f, basePrice * 3.0f);
    }

    // ─────────────────────────────────────────
    // SELL PRICE  (giá làng trả cho người chơi, đã làm tròn)
    // ─────────────────────────────────────────
    // Hàm tiện ích: nhận cấu hình món + trạng thái tồn kho -> ra giá nguyên (int).
    // Dùng trong: VillageMarket.GetSellPrice() và RegisterSale().
    public static int CalculateSellPrice(int basePrice, ElasticityTier tier,
                                         int currentStock, int basketSize, int daysSinceSale)
    {
        float epsilon = GetElasticity(tier);
        float price = CalculatePrice(basePrice, currentStock, basketSize, epsilon, daysSinceSale);
        return Mathf.RoundToInt(price);
    }

    // ─────────────────────────────────────────
    // BUY PRICE  (làng BÁN cho player — khan hiếm thì giá tăng)
    // ─────────────────────────────────────────
    // Ngược chiều giá bán: kho làng càng cạn (player mua nhiều trong ngày) thì giá càng cao (luật cung).
    //   scarcity = 1 − remaining/dailyStock   (0 khi đầy kho → tiến tới 1 khi gần hết)
    //   Price    = BasePrice × (1 + scarcity)^ε
    // Clamp [100%, 300%]: mua KHÔNG bao giờ rẻ hơn giá gốc, tối đa gấp 3 — đối xứng với trần bán.
    // Dùng trong: VillageMarket.GetBuyPrice() / BuyFromVillage().
    public static int CalculateBuyPrice(int basePrice, ElasticityTier tier, int remaining, int dailyStock)
    {
        if (dailyStock <= 0) return basePrice;
        float scarcity = 1f - Mathf.Clamp01((float)remaining / dailyStock);
        float epsilon  = GetElasticity(tier);
        float raw      = basePrice * Mathf.Pow(1f + scarcity, epsilon);
        return Mathf.RoundToInt(Mathf.Clamp(raw, basePrice, basePrice * 3f));
    }
}
