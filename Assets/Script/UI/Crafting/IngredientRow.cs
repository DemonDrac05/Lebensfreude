using TMPro;
using UnityEngine;
using UnityEngine.UI;

// ─────────────────────────────────────────
// INGREDIENT ROW  (1 dòng nguyên liệu trong CraftingSlot)
// ─────────────────────────────────────────
// Hiển thị: icon (tuỳ chọn) + text "MaterialName × qty  (có: X)".
// Màu text trắng/đỏ theo đủ/thiếu trong kho.
// materialIcon và materialText gán trong prefab — chỉ materialText bắt buộc.
//
// Liên kết: InventoryManager.CountItem (đếm số có trong kho),
//           CraftingSlot.SpawnRow/BuildIngredientRows (spawn row này),
//           CraftingSlot.Refresh (gọi Refresh ở đây để cập nhật màu).
public class IngredientRow : MonoBehaviour
{
    // ─────────────────────────────────────────
    // INSPECTOR — gán trong prefab
    // ─────────────────────────────────────────
    [Header("=== UI References ==========")]
    [SerializeField] private Image           materialIcon; // optional — null thì bỏ qua
    [SerializeField] private TextMeshProUGUI materialText; // "Iron Ore × 2  (có: 1)"

    // ─────────────────────────────────────────
    // RUNTIME
    // ─────────────────────────────────────────
    private BaseItem _material;
    private int      _quantity;
    private bool     _isFuel;

    // ─────────────────────────────────────────
    // SETUP  (gọi 1 lần từ CraftingSlot.SpawnRow)
    // ─────────────────────────────────────────
    // Điền dữ liệu + gọi Refresh lần đầu. Dùng trong: CraftingSlot.SpawnRow().
    public void Setup(BaseItem material, int quantity, bool isFuel,
                      Color colorEnough, Color colorNotEnough)
    {
        _material = material;
        _quantity = quantity;
        _isFuel   = isFuel;

        // Icon (optional)
        if (materialIcon != null)
        {
            bool hasSprite = material.image != null;
            materialIcon.gameObject.SetActive(hasSprite);
            if (hasSprite) materialIcon.sprite = material.image;
        }

        Refresh(colorEnough, colorNotEnough);
    }

    // ─────────────────────────────────────────
    // REFRESH  (cập nhật text + màu theo kho hiện tại)
    // ─────────────────────────────────────────
    // Đọc InventoryManager.CountItem mỗi lần Refresh → không cache count (kho hay thay đổi).
    // Dùng trong: CraftingSlot.Refresh(), Setup().
    public void Refresh(Color colorEnough, Color colorNotEnough)
    {
        if (_material == null || materialText == null) return;

        int have   = InventoryManager.Instance != null
            ? InventoryManager.Instance.CountItem(_material)
            : 0;
        bool enough = have >= _quantity;

        // Nhãn fuel giúp phân biệt với nguyên liệu chính
        string fuelTag = _isFuel ? " [fuel]" : "";
        materialText.text  = $"{_material.name} × {_quantity}  (có: {have}){fuelTag}";
        materialText.color  = enough ? colorEnough : colorNotEnough;
    }
}
