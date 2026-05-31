using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ItemCategory : MonoBehaviour
{
    [Header(
        "// ─────────────────────────────────────────\n    // TOOLS\n    // ─────────────────────────────────────────")]
    public Tool[] tools;

    [Header(
        "// ─────────────────────────────────────────\n    // PLANTS\n    // ─────────────────────────────────────────")]
    public Plant[] plants;

    
    [Header(
        "// ─────────────────────────────────────────\n    // PRODUCTS\n    // ─────────────────────────────────────────")]
    public Product[] products;
}
