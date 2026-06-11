using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProcessingSlot : MonoBehaviour
{
    [Header("=== UI ==========")]
    [SerializeField] private Image slotBackground, itemIcon;
    [SerializeField] private TextMeshProUGUI itemNameText, outputAmountText;
    [SerializeField] private Transform  ingredientsContainer;
    [SerializeField] private GameObject ingredientRowPrefab;
    [SerializeField] private Button addButton;
    [SerializeField] private TextMeshProUGUI addButtonText;

    [Header("=== Colors ==========")]
    [SerializeField] private Color colorSlotEnough = Color.white, colorSlotNotEnough = new Color(.55f,.55f,.55f,1f);
    [SerializeField] private Color colorBtnEnough  = Color.white, colorBtnLocked     = new Color(.5f,.5f,.5f,1f);
    [SerializeField] private Color colorIngEnough  = Color.white, colorIngNotEnough  = Color.red;
    [SerializeField] private string labelAdd = "Smelt", labelLocked = "Không thể";

    private BaseItem _output; private ProcessingStation _station;
    private readonly List<IngredientRow> _rows = new();

    public void Setup(BaseItem output, ProcessingStation station)
    {
        _output = output; _station = station;
        if (itemIcon != null) { itemIcon.sprite = output.image; itemIcon.enabled = output.image != null; }
        if (itemNameText != null) itemNameText.text = output.name;
        int outAmt = output.recipe != null ? Mathf.Max(1, output.recipe.outputAmount) : 1;
        if (outputAmountText != null) outputAmountText.text = $"× {outAmt}";
        if (addButton != null) { addButton.onClick.RemoveAllListeners(); addButton.onClick.AddListener(OnAdd); }
        BuildRows(); Refresh();
    }

    public void Refresh()
    {
        if (_output == null || _station == null) return;
        bool can = _station.CanAdd(_output);
        if (slotBackground != null) slotBackground.color = can ? colorSlotEnough : colorSlotNotEnough;
        if (addButton != null)
        {
            addButton.interactable = can;
            var img = addButton.GetComponent<Image>();
            if (img != null) img.color = can ? colorBtnEnough : colorBtnLocked;
        }
        if (addButtonText != null) addButtonText.text = can ? labelAdd : labelLocked;
        foreach (var r in _rows) r.Refresh(colorIngEnough, colorIngNotEnough);
    }

    private void BuildRows()
    {
        ClearRows();
        if (_output == null || _output.recipe == null) return;
        if (ingredientsContainer == null || ingredientRowPrefab == null) return;
        var r = _output.recipe;
        foreach (var inp in r.inputs) { if (inp == null || inp.material == null) continue; Spawn(inp.material, inp.quantity, false); }
        if (r.fuel != null && r.fuelAmount > 0) Spawn(r.fuel, r.fuelAmount, true);
    }
    private void Spawn(BaseItem m, int q, bool fuel)
    {
        var go = Instantiate(ingredientRowPrefab, ingredientsContainer);
        var row = go.GetComponent<IngredientRow>();
        if (row == null) { Destroy(go); return; }
        row.Setup(m, q, fuel, colorIngEnough, colorIngNotEnough); _rows.Add(row);
    }
    private void ClearRows() { foreach (var r in _rows) if (r != null) Destroy(r.gameObject); _rows.Clear(); }
    private void OnAdd() { if (_station != null && _output != null) { _station.Add(_output); Refresh(); } }
}
