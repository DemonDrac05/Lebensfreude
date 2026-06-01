using UnityEngine;

// ─────────────────────────────────────────
// CONSUMABLE  (vật phẩm ăn được -> hồi stamina)
// ─────────────────────────────────────────
// Kế thừa BaseItem. Apple ~15, Stamina Tonic ~40 (theo Full Design Document mục 4).
// Stack được (MaxStackable mặc định 999 của BaseItem).
// Liên kết: FoodSystem (chuột phải để ăn), StaminaManager (Restore).
[CreateAssetMenu(menuName = "ScriptableObjects/Item/Consumable")]
public class Consumable : BaseItem
{
    [Header("=== Hồi phục stamina ==========")]
    public float staminaRestore = 15f;
}
