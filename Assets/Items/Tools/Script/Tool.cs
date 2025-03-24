using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "ScriptableObjects/Item/Tool")]
public class Tool : BaseItem
{
    [Header("=== UI Components ==========")]
    public Slider slider;

    [Header("=== Game Object References ==========")]
    public GameObject gameObj;

    [Header("=== Properties =========")]
    public float efficiency;

    [Header("=== Tool Action Settings ==========")]
    public ActionType actionType;

    [Header("=== Crafting Materials ==========")]
    public int feeToUpgrade;
    public float timeToUpgrade;

    public MaterialRequirement materialBox;
    public Tool requiredTool;
    public Tool upgradedTool;

    public override int MaxStackable => 1;
}

[System.Serializable]
public class MaterialRequirement
{
    public BaseItem material;
    public int quantity;
}

public enum ActionType
{
    Water,
    Chop,
    Cultivate,
    Sow,
    Harvest,
    Mine,
    Attack
}
