using System;
using UnityEngine;

[Serializable]
public class VillageStockItem
{
    [Header("=== Item reference ==========")]
    public BaseItem item;                                  // Reagent, Flux Powder, Coal, Legendary Recipe...

    [Header("=== Economic parameters ==========")]
    public ElasticityTier tier = ElasticityTier.Metal;
    [Tooltip("-1 = use the BaseItem's own buyingPrice")]
    public int basePriceOverride = -1;

    [Header("=== Unlock & scarcity ==========")]
    public VillagePhase availableFromPhase = VillagePhase.Partnership;
    [Tooltip("Maximum quantity sold PER DAY")]
    public int dailyStock = 5;
    [Range(0f, 1f)]
    [Tooltip("Probability the item APPEARS for sale on a given day (1 = always)")]
    public float appearanceChance = 1f;

    public int BasePrice => basePriceOverride >= 0
        ? basePriceOverride
        : (item != null ? item.buyingPrice : 0);
}
