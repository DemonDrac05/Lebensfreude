using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//
//
public class CraftingSlot : MonoBehaviour
{
    [Header("=== UI References ==========")]
    [SerializeField] private Image              slotBackground;
    [SerializeField] private Image              itemIcon;
    [SerializeField] private TextMeshProUGUI    itemNameText;
    [SerializeField] private TextMeshProUGUI    outputAmountText;
    [SerializeField] private Transform          ingredientsContainer; // parent spawn IngredientRow
    [SerializeField] private GameObject         ingredientRowPrefab;
    [SerializeField] private Button             craftButton;
    [SerializeField] private TextMeshProUGUI    craftButtonText;

    [Header("=== Colors for enough / missing materials ==========")]
    [SerializeField] private Color colorSlotEnough    = Color.white;
    [SerializeField] private Color colorSlotNotEnough = new Color(0.55f, 0.55f, 0.55f, 1f);
    [SerializeField] private Color colorBtnEnough     = Color.white;
    [SerializeField] private Color colorBtnLocked     = new Color(0.5f, 0.5f, 0.5f, 1f);
    [SerializeField] private Color colorIngEnough     = Color.white;
    [SerializeField] private Color colorIngNotEnough  = Color.red;

    [Header("=== Craft button label ==========")]
    [SerializeField] private string labelCraft  = "Craft";
    [SerializeField] private string labelLocked = "Thiếu nguyên liệu";

    // RUNTIME
    private BaseItem                   _output;
    private CraftingStation            _station;
    private readonly List<IngredientRow> _rows = new();

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

        if (itemNameText != null) itemNameText.text = output.name;

        int outAmt = (output.recipe != null) ? Mathf.Max(1, output.recipe.outputAmount) : 1;
        if (outputAmountText != null) outputAmountText.text = $"× {outAmt}";

        if (craftButton != null)
        {
            craftButton.onClick.RemoveAllListeners();
            craftButton.onClick.AddListener(OnCraftClicked);
        }

        BuildIngredientRows();

        Refresh();
    }

    public void Refresh()
    {
        if (_output == null || _station == null) return;

        bool canCraft = _station.CanCraft(_output);

        if (slotBackground != null)
            slotBackground.color = canCraft ? colorSlotEnough : colorSlotNotEnough;

        if (craftButton != null)
        {
            craftButton.interactable = canCraft;
            var btnImg = craftButton.GetComponent<Image>();
            if (btnImg != null)
                btnImg.color = canCraft ? colorBtnEnough : colorBtnLocked;
        }
        if (craftButtonText != null)
            craftButtonText.text = canCraft ? labelCraft : labelLocked;

        foreach (var row in _rows)
            row.Refresh(colorIngEnough, colorIngNotEnough);
    }

    // INGREDIENT ROWS
    private void BuildIngredientRows()
    {
        ClearRows();
        if (_output == null || _output.recipe == null) return;
        if (ingredientsContainer == null || ingredientRowPrefab == null) return;

        var r = _output.recipe;

        foreach (var inp in r.inputs)
        {
            if (inp == null || inp.material == null) continue;
            SpawnRow(inp.material, inp.quantity, isFuel: false);
        }

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

    // CRAFT
    private void OnCraftClicked()
    {
        if (_station == null || _output == null) return;
        bool success = _station.Craft(_output);
        if (success) Refresh();
    }
}
