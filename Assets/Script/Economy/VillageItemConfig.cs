using System;
using UnityEngine;

//
[Serializable]
public class VillageItemConfig
{
    [Header("=== Item reference ==========")]
    public BaseItem item;

    [Header("=== Economic parameters ==========")]
    public ElasticityTier tier = ElasticityTier.Basic;
    [Tooltip("-1 = use the BaseItem's own sellingPrice")]
    public int basePriceOverride = -1;
    [Tooltip("Reference basket: starting stock; smaller = more price-sensitive")]
    public int basketSize = 10;

    [Header("=== Village specialisation (comparative advantage) ==========")]
    [Tooltip("Price multiplier for THIS good at THIS village. >1 = specialty (pays more), <1 = outside specialty (pays less). Makes the same good worth different amounts at different villages.")]
    [Range(0.25f, 3f)] public float priceMultiplier = 1f;

    [Header("=== Phase gating ==========")]
    public VillagePhase availableFromPhase = VillagePhase.Trust;

    public int BasePrice => basePriceOverride >= 0
        ? basePriceOverride
        : (item != null ? item.sellingPrice : 0);

    // Base price AFTER the village specialisation multiplier. Used by VillageMarket.GetSellPrice().
    public int SpecialisedBasePrice => Mathf.Max(1, Mathf.RoundToInt(BasePrice * priceMultiplier));
}
