using UnityEngine;

//
//
//
public static class EconomicSimulator
{
    public static float GetElasticity(ElasticityTier tier) => tier switch
    {
        ElasticityTier.Basic   => 0.2f,
        ElasticityTier.Metal   => 0.5f,
        ElasticityTier.Alloy   => 0.8f,
        ElasticityTier.Endgame => 1.2f,
        _                      => 0.5f
    };

    // RECOVERY FACTOR  R(t)
    public static float RecoveryFactor(int daysSinceSale)
        => 1f - 0.4f * Mathf.Exp(-0.3f * daysSinceSale);

    public static float CalculatePrice(float basePrice, int currentStock,
                                       int basketSize, float epsilon, int daysSinceSale)
    {
        if (basketSize <= 0) return basePrice;

        float stockRatio = Mathf.Max(0.001f, (float)currentStock / basketSize);
        float raw = basePrice * Mathf.Pow(stockRatio, epsilon) * RecoveryFactor(daysSinceSale);

        return Mathf.Clamp(raw, basePrice * 0.1f, basePrice * 3.0f);
    }

    public static int CalculateSellPrice(int basePrice, ElasticityTier tier,
                                         int currentStock, int basketSize, int daysSinceSale)
    {
        float epsilon = GetElasticity(tier);
        float price = CalculatePrice(basePrice, currentStock, basketSize, epsilon, daysSinceSale);
        return Mathf.RoundToInt(price);
    }

    public static int CalculateBuyPrice(int basePrice, ElasticityTier tier, int remaining, int dailyStock)
    {
        if (dailyStock <= 0) return basePrice;
        float scarcity = 1f - Mathf.Clamp01((float)remaining / dailyStock);
        float epsilon  = GetElasticity(tier);
        float raw      = basePrice * Mathf.Pow(1f + scarcity, epsilon);
        return Mathf.RoundToInt(Mathf.Clamp(raw, basePrice, basePrice * 3f));
    }
}
