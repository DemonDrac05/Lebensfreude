using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/BaseItem")]
public class BaseItem : ScriptableObject
{
    [Header("=== Visual Settings ==========")]
    public Sprite image;

    [Header("=== Pricing Information ==========")]
    public int buyingPrice;
    public int sellingPrice;

    [Header("=== Status Flags ==========")]
    [HideInInspector] public bool isPurchaseable;

    private void OnEnable() => isPurchaseable = buyingPrice != -1 || sellingPrice != -1;

    public virtual int MaxStackable => 999;
}

public class Farming    : BaseItem { }
public class Mining     : BaseItem { }
public class Foraging   : BaseItem { }
public class Fishing    : BaseItem { }
public class Combat     : BaseItem { }


