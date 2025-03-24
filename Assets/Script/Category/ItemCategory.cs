using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ItemCategory : MonoBehaviour
{
    [Header("Tools")]
    public Tool[] tools;

    [Header("Plants")]
    public Plant[] plants;

    [Header("Products")]
    public Product[] products;

    [Header("Others")]
    public CraftingItem[] others;
}
