using System;
using System.Collections.Generic;
using UnityEngine;

public enum CraftStation { None, Workbench, Smelter, Forge, AlchemyTable, Hand }

//
[Serializable]
public class CraftingRecipe
{
    [Header("=== Which station crafts this ==========")]
    public CraftStation station = CraftStation.None;

    [Header("=== Ingredients & output ==========")]
    public List<MaterialRequirement> inputs = new();
    public int outputAmount = 1;

    [Header("=== Timed (Smelter/Forge) - set 0 for instant ==========")]
    public float craftTimeSeconds = 0f;
    public BaseItem fuel;
    public int fuelAmount = 0;

    [Header("=== Success rate & by-product on fail ==========")]
    [Range(0f, 1f)] public float successRate = 1f;
    public BaseItem failByproduct;                    // slag / scrap
    public int failByproductAmount = 1;

    [Header("=== Special ==========")]
    public bool craftOnce = false;
    public bool unlocksMerchantJournal = false;

    public bool IsCraftableAt(CraftStation s) => station != CraftStation.None && station == s;
}
