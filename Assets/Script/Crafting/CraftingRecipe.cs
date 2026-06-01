using System;
using System.Collections.Generic;
using UnityEngine;

// ─────────────────────────────────────────
// CRAFT STATION  (loại bàn chế tạo)
// ─────────────────────────────────────────
public enum CraftStation { None, Workbench, Smelter, Forge, AlchemyTable, Hand }

// ─────────────────────────────────────────
// CRAFTING RECIPE  (công thức GẮN TRÊN item SO output)
// ─────────────────────────────────────────
// Theo lựa chọn: recipe nằm trên CHÍNH item làm ra nó (BaseItem.recipe). Station chỉ liệt kê outputs.
// Dùng MaterialRequirement có sẵn của main (material + quantity).
//
// Liên kết: BaseItem.recipe (chứa nó), CraftingStation (instant), ProcessingStation (timed).
[Serializable]
public class CraftingRecipe
{
    [Header("=== Chế tạo ở station nào ==========")]
    public CraftStation station = CraftStation.None;   // None = không craft được

    [Header("=== Nguyên liệu & sản lượng ==========")]
    public List<MaterialRequirement> inputs = new();
    public int outputAmount = 1;

    [Header("=== Timed (Smelter/Forge) — instant thì để 0 ==========")]
    public float craftTimeSeconds = 0f;
    public BaseItem fuel;                 // coal / charcoal (cố định, bỏ heat)
    public int fuelAmount = 0;

    [Header("=== Tỉ lệ thành công & thụ phẩm khi fail ==========")]
    [Range(0f, 1f)] public float successRate = 1f;   // 1 = luôn thành công (Workbench/Alchemy)
    public BaseItem failByproduct;                    // slag / scrap
    public int failByproductAmount = 1;

    [Header("=== Đặc biệt ==========")]
    public bool craftOnce = false;             // craft 1 lần rồi xóa khỏi list (vd Merchant Journal)
    public bool unlocksMerchantJournal = false; // craft lần đầu -> mở khóa Journal vĩnh viễn

    // Có craft được ở station này không? Dùng trong: CraftingStation / ProcessingStation.
    public bool IsCraftableAt(CraftStation s) => station != CraftStation.None && station == s;
}
