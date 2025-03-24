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
    [TextArea] public string instructionText;
    public bool placeable;

    [Header("=== Crafting Materials =========")]
    public List<MaterialRequirement> materialBox;

    [Header("=== Max Stackable Amount ==========")]
    public int stackableAmount;

    public override int MaxStackable => stackableAmount;
}
