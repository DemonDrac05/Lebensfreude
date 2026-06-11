using TMPro;
using UnityEngine;
using UnityEngine.UI;

//
public class IngredientRow : MonoBehaviour
{
    [Header("=== UI References ==========")]
    [SerializeField] private Image           materialIcon;
    [SerializeField] private TextMeshProUGUI materialText;

    // RUNTIME
    private BaseItem _material;
    private int      _quantity;
    private bool     _isFuel;

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

    public void Refresh(Color colorEnough, Color colorNotEnough)
    {
        if (_material == null || materialText == null) return;

        int have   = InventoryManager.Instance != null
            ? InventoryManager.Instance.CountItem(_material)
            : 0;
        bool enough = have >= _quantity;

        string fuelTag = _isFuel ? " [fuel]" : "";
        materialText.text  = $"{_material.name} × {_quantity} \n(have: {have}){fuelTag}";
        materialText.color  = enough ? colorEnough : colorNotEnough;
    }
}
