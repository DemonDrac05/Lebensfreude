using NUnit.Framework;
using UnityEngine;


public class EconomicSimulatorTests
{
    [Test]
    public void SellPrice_AtFullDemand_IsAboutBasePrice()
    {
        int basePrice = 100;
        int price = EconomicSimulator.CalculateSellPrice(basePrice, ElasticityTier.Metal, 100, 100, 30);
        Assert.That(price, Is.InRange(basePrice - 1, basePrice));
    }

    [Test]
    public void SellPrice_NearEmpty_IsClampedToFloor()
    {
        int basePrice = 100;
        int price = EconomicSimulator.CalculateSellPrice(basePrice, ElasticityTier.Endgame, 1, 100, 30);
        Assert.AreEqual(Mathf.RoundToInt(basePrice * 0.1f), price); // = 10
    }

    [Test]
    public void SellPrice_IsMonotonic_NeverRewardsFlooding()
    {
        int basePrice = 200, prev = -1;
        for (int remaining = 0; remaining <= 100; remaining++)
        {
            int price = EconomicSimulator.CalculateSellPrice(basePrice, ElasticityTier.Alloy, remaining, 100, 0);
            Assert.GreaterOrEqual(price, prev);
            prev = price;
        }
    }

    [Test]
    public void RecoveryFactor_StartsAtPoint6_AndRecoversToOne()
    {
        Assert.That(EconomicSimulator.RecoveryFactor(0), Is.EqualTo(0.6f).Within(0.001f));
        Assert.Less(EconomicSimulator.RecoveryFactor(0), EconomicSimulator.RecoveryFactor(7));
        Assert.That(EconomicSimulator.RecoveryFactor(30), Is.EqualTo(1f).Within(0.01f));
    }

    [Test]
    public void Elasticity_Tiers_HaveDesignedValues()
    {
        Assert.AreEqual(0.2f, EconomicSimulator.GetElasticity(ElasticityTier.Basic),   0.0001f);
        Assert.AreEqual(0.5f, EconomicSimulator.GetElasticity(ElasticityTier.Metal),   0.0001f);
        Assert.AreEqual(0.8f, EconomicSimulator.GetElasticity(ElasticityTier.Alloy),   0.0001f);
        Assert.AreEqual(1.2f, EconomicSimulator.GetElasticity(ElasticityTier.Endgame), 0.0001f);
    }

    [Test]
    public void BuyPrice_StaysWithin100To300Percent()
    {
        int basePrice = 100;
        int full  = EconomicSimulator.CalculateBuyPrice(basePrice, ElasticityTier.Metal, 100, 100);
        int empty = EconomicSimulator.CalculateBuyPrice(basePrice, ElasticityTier.Metal, 0,   100);
        Assert.AreEqual(basePrice, full);
        Assert.That(empty, Is.InRange(basePrice, basePrice * 3));
    }
}

public class CraftingProbabilityTests
{
    [Test]
    public void SuccessRate_IsUnbiased_OverManyTrials()
    {
        float designedRate = 0.75f;
        int trials = 100000;
        var rng = new System.Random(12345);
        int success = 0;
        for (int i = 0; i < trials; i++)
            if (rng.NextDouble() <= designedRate) success++;

        float observed = (float)success / trials;
        Assert.That(observed, Is.EqualTo(designedRate).Within(0.01f));
    }
}
