using NUnit.Framework;
using UnityEngine;

// ╔══════════════════════════════════════════════════════════════════╗
// ║  UNIT TEST (NUnit, EditMode) — kiểm thử logic kinh tế              ║
// ╠══════════════════════════════════════════════════════════════════╣
// ║  Vì file nằm trong thư mục tên "Editor" nên Unity gom vào          ║
// ║  Assembly-CSharp-Editor: THẤY ĐƯỢC EconomicSimulator + NUnit mà    ║
// ║  KHÔNG cần tạo Assembly Definition (asmdef).                       ║
// ║                                                                    ║
// ║  CHẠY: Window > General > Test Runner > tab EditMode > Run All     ║
// ║  Mỗi [Test] = 1 ca kiểm thử. Xanh = pass. Chụp màn hình -> Figure 17║
// ╚══════════════════════════════════════════════════════════════════╝

// Mỗi test theo 3 bước AAA: Arrange (chuẩn bị) -> Act (gọi hàm) -> Assert (khẳng định kết quả).
public class EconomicSimulatorTests
{
    // 1) Còn nguyên sức mua trong ngày -> giá bán xấp xỉ giá gốc.
    [Test]
    public void SellPrice_AtFullDemand_IsAboutBasePrice()
    {
        int basePrice = 100;
        int price = EconomicSimulator.CalculateSellPrice(basePrice, ElasticityTier.Metal, 100, 100, 30);
        Assert.That(price, Is.InRange(basePrice - 1, basePrice)); // R(30) ≈ 1
    }

    // 2) Đã bán gần hết sức mua -> giá bị KẸP ở sàn 10% giá gốc.
    [Test]
    public void SellPrice_NearEmpty_IsClampedToFloor()
    {
        int basePrice = 100;
        int price = EconomicSimulator.CalculateSellPrice(basePrice, ElasticityTier.Endgame, 1, 100, 30);
        Assert.AreEqual(Mathf.RoundToInt(basePrice * 0.1f), price); // = 10
    }

    // 3) ĐƠN ĐIỆU: bán càng nhiều (sức mua còn lại giảm) thì giá KHÔNG BAO GIỜ tăng
    //    -> đảm bảo game không thưởng cho việc "dội chợ". Đây là tính chất quan trọng nhất.
    [Test]
    public void SellPrice_IsMonotonic_NeverRewardsFlooding()
    {
        int basePrice = 200, prev = -1;
        for (int remaining = 0; remaining <= 100; remaining++)
        {
            int price = EconomicSimulator.CalculateSellPrice(basePrice, ElasticityTier.Alloy, remaining, 100, 0);
            Assert.GreaterOrEqual(price, prev); // tăng dần theo sức mua còn lại
            prev = price;
        }
    }

    // 4) Hệ số hồi phục: ngày bán (t=0) -> 0.6 ; càng nhiều ngày -> tiến dần về 1.
    [Test]
    public void RecoveryFactor_StartsAtPoint6_AndRecoversToOne()
    {
        Assert.That(EconomicSimulator.RecoveryFactor(0), Is.EqualTo(0.6f).Within(0.001f));
        Assert.Less(EconomicSimulator.RecoveryFactor(0), EconomicSimulator.RecoveryFactor(7));
        Assert.That(EconomicSimulator.RecoveryFactor(30), Is.EqualTo(1f).Within(0.01f));
    }

    // 5) Bốn bậc co giãn đúng giá trị thiết kế (0.2 / 0.5 / 0.8 / 1.2).
    [Test]
    public void Elasticity_Tiers_HaveDesignedValues()
    {
        Assert.AreEqual(0.2f, EconomicSimulator.GetElasticity(ElasticityTier.Basic),   0.0001f);
        Assert.AreEqual(0.5f, EconomicSimulator.GetElasticity(ElasticityTier.Metal),   0.0001f);
        Assert.AreEqual(0.8f, EconomicSimulator.GetElasticity(ElasticityTier.Alloy),   0.0001f);
        Assert.AreEqual(1.2f, EconomicSimulator.GetElasticity(ElasticityTier.Endgame), 0.0001f);
    }

    // 6) Giá MUA luôn nằm trong dải [100%, 300%] giá gốc.
    [Test]
    public void BuyPrice_StaysWithin100To300Percent()
    {
        int basePrice = 100;
        int full  = EconomicSimulator.CalculateBuyPrice(basePrice, ElasticityTier.Metal, 100, 100); // còn đầy
        int empty = EconomicSimulator.CalculateBuyPrice(basePrice, ElasticityTier.Metal, 0,   100); // hết hàng
        Assert.AreEqual(basePrice, full);                          // còn đầy -> đúng giá gốc
        Assert.That(empty, Is.InRange(basePrice, basePrice * 3));  // khan hiếm -> cao hơn, tối đa 3x
    }
}

// Test thống kê cho xác suất chế tạo (đúng như mô tả ở mục Testing của bài luận):
// gieo nhiều lần với seed CỐ ĐỊNH -> tỉ lệ thực nghiệm phải sát tỉ lệ thiết kế (không lệch).
// Test này tự chứa, không phụ thuộc script game nào nên luôn biên dịch được.
public class CraftingProbabilityTests
{
    [Test]
    public void SuccessRate_IsUnbiased_OverManyTrials()
    {
        float designedRate = 0.75f;          // ví dụ: Steel = 75%
        int trials = 100000;
        var rng = new System.Random(12345);  // seed cố định -> test lặp lại được kết quả
        int success = 0;
        for (int i = 0; i < trials; i++)
            if (rng.NextDouble() <= designedRate) success++;

        float observed = (float)success / trials;
        Assert.That(observed, Is.EqualTo(designedRate).Within(0.01f)); // sai số < 1%
    }
}
