using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// ─────────────────────────────────────────
// CRAFTING SLOT  (1 card trong danh sách craft của instant station)
// ─────────────────────────────────────────
// Setup(item, station): điền icon/tên/sản lượng + spawn IngredientRow cho mỗi input + fuel.
// Refresh(): cập nhật màu nền, màu text nguyên liệu, trạng thái nút theo CraftingStation.CanCraft().
// Bấm Craft → CraftingStation.Craft() → tự Refresh() không cần event thêm.
//
// Màu và label text để SerializeField → Tank điền trong Inspector của prefab.
//
// Liên kết: CraftingStation.CanCraft/Craft (kiểm tra + thực hiện craft),
//           InventoryManager.CountItem (đếm nguyên liệu trong kho qua IngredientRow),
//           CraftingRecipe.inputs/fuel (danh sách nguyên liệu),
//           CraftingStationUI.PopulateSlots (spawn slot này).
public class CraftingSlot : MonoBehaviour
{
    // ─────────────────────────────────────────
    // INSPECTOR — gán trong prefab
    // ─────────────────────────────────────────
    [Header("=== UI References ==========")]
    [SerializeField] private Image              slotBackground;       // nền slot đổi màu khi thiếu
    [SerializeField] private Image              itemIcon;             // sprite của item output
    [SerializeField] private TextMeshProUGUI    itemNameText;         // tên item
    [SerializeField] private TextMeshProUGUI    outputAmountText;     // "× N" sản lượng
    [SerializeField] private Transform          ingredientsContainer; // parent spawn IngredientRow
    [SerializeField] private GameObject         ingredientRowPrefab;  // prefab có IngredientRow component
    [SerializeField] private Button             craftButton;
    [SerializeField] private TextMeshProUGUI    craftButtonText;

    [Header("=== Màu khi đủ / thiếu nguyên liệu ==========")]
    [SerializeField] private Color colorSlotEnough    = Color.white;
    [SerializeField] private Color colorSlotNotEnough = new Color(0.55f, 0.55f, 0.55f, 1f); // xám
    [SerializeField] private Color colorBtnEnough     = Color.white;
    [SerializeField] private Color colorBtnLocked     = new Color(0.5f, 0.5f, 0.5f, 1f);
    [SerializeField] private Color colorIngEnough     = Color.white;   // màu text nguyên liệu đủ
    [SerializeField] private Color colorIngNotEnough  = Color.red;     // màu text nguyên liệu thiếu

    [Header("=== Label nút Craft ==========")]
    [SerializeField] private string labelCraft  = "Craft";
    [SerializeField] private string labelLocked = "Thiếu nguyên liệu";

    // ─────────────────────────────────────────
    // RUNTIME
    // ─────────────────────────────────────────
    private BaseItem                   _output;
    private CraftingStation            _station;
    private readonly List<IngredientRow> _rows = new();

    // ─────────────────────────────────────────
    // SETUP  (gọi 1 lần từ CraftingStationUI.PopulateSlots)
    // ─────────────────────────────────────────
    // Điền icon/tên/sản lượng, spawn ingredient rows, gắn listener nút Craft. Dùng trong: CraftingStationUI.
    public void Setup(BaseItem output, CraftingStation station)
    {
        _output  = output;
        _station = station;

        // Icon
        if (itemIcon != null)
        {
            itemIcon.sprite  = output.image;
            itemIcon.enabled = output.image != null;
        }

        // Tên (tên SO asset = tên item hiển thị trong game)
        if (itemNameText != null) itemNameText.text = output.name;

        // Sản lượng output
        int outAmt = (output.recipe != null) ? Mathf.Max(1, output.recipe.outputAmount) : 1;
        if (outputAmountText != null) outputAmountText.text = $"× {outAmt}";

        // Nút Craft: 1 listener, RemoveAllListeners tránh stacking nếu Setup gọi lại
        if (craftButton != null)
        {
            craftButton.onClick.RemoveAllListeners();
            craftButton.onClick.AddListener(OnCraftClicked);
        }

        // Spawn dòng nguyên liệu
        BuildIngredientRows();

        // Refresh màu lần đầu
        Refresh();
    }

    // ─────────────────────────────────────────
    // REFRESH  (cập nhật toàn bộ màu + text)
    // ─────────────────────────────────────────
    // Gọi sau mỗi craft, sau OnCraftListChanged (qua CraftingStationUI), khi mở panel.
    // Dùng trong: Setup(), OnCraftClicked(), CraftingStationUI.OnCraftListChanged().
    public void Refresh()
    {
        if (_output == null || _station == null) return;

        bool canCraft = _station.CanCraft(_output);

        // Nền slot
        if (slotBackground != null)
            slotBackground.color = canCraft ? colorSlotEnough : colorSlotNotEnough;

        // Nút Craft: interactable + màu + label
        if (craftButton != null)
        {
            craftButton.interactable = canCraft;
            var btnImg = craftButton.GetComponent<Image>();
            if (btnImg != null)
                btnImg.color = canCraft ? colorBtnEnough : colorBtnLocked;
        }
        if (craftButtonText != null)
            craftButtonText.text = canCraft ? labelCraft : labelLocked;

        // Refresh màu từng dòng nguyên liệu
        foreach (var row in _rows)
            row.Refresh(colorIngEnough, colorIngNotEnough);
    }

    // ─────────────────────────────────────────
    // INGREDIENT ROWS
    // ─────────────────────────────────────────
    // Spawn 1 IngredientRow cho mỗi input trong recipe + fuel (nếu có). Dùng trong: Setup().
    private void BuildIngredientRows()
    {
        ClearRows();
        if (_output == null || _output.recipe == null) return;
        if (ingredientsContainer == null || ingredientRowPrefab == null) return;

        var r = _output.recipe;

        // Nguyên liệu chính
        foreach (var inp in r.inputs)
        {
            if (inp == null || inp.material == null) continue;
            SpawnRow(inp.material, inp.quantity, isFuel: false);
        }

        // Fuel (chỉ Smelter/Forge — Workbench thường không có)
        if (r.fuel != null && r.fuelAmount > 0)
            SpawnRow(r.fuel, r.fuelAmount, isFuel: true);
    }

    private void SpawnRow(BaseItem material, int quantity, bool isFuel)
    {
        var go  = Instantiate(ingredientRowPrefab, ingredientsContainer);
        var row = go.GetComponent<IngredientRow>();
        if (row == null)
        {
            Debug.LogWarning("[CraftingSlot] ingredientRowPrefab thiếu component IngredientRow.", this);
            Destroy(go);
            return;
        }
        row.Setup(material, quantity, isFuel, colorIngEnough, colorIngNotEnough);
        _rows.Add(row);
    }

    private void ClearRows()
    {
        foreach (var row in _rows)
            if (row != null) Destroy(row.gameObject);
        _rows.Clear();
    }

    // ─────────────────────────────────────────
    // CRAFT
    // ─────────────────────────────────────────
    // Gọi CraftingStation.Craft() → nếu thành công → Refresh() ngay.
    // CraftingStation.Craft() tự gọi OnCraftListChanged nếu craftOnce → CraftingStationUI rebuild.
    // Dùng trong: craftButton.onClick.
    private void OnCraftClicked()
    {
        if (_station == null || _output == null) return;
        bool success = _station.Craft(_output);
        if (success) Refresh(); // cập nhật màu/nút ngay sau khi trừ nguyên liệu
    }
}
