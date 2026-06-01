using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Item/Crafting")]
public class CraftingItem : BaseItem
{
    [Header("=== GameObject Reference ==========")]
    public GameObject gameObj;

    [Header("=== Tile Occupation ==========")]
    public int column;
    public int row;

    [Header("=== Properties Settings ==========")]
    public bool placeable;

    [Header("=== Max Stackable Amount ==========")]
    public int stackableAmount;

    public override int MaxStackable => stackableAmount;
}
