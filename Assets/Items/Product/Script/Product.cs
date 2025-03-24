using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Plant/Product")]
public class Product : BaseItem
{
    [Header("=== Game Object References ==========")]
    public GameObject gameObj;

    [Header("=== Product Types Settings ==========")]
    public ProductType productType;
}

public enum ProductType
{
    Crop,
    Material
}
