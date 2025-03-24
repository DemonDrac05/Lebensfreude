using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ToolbarSlot : BaseSlot
{
    [Header("=== UI Properties ==========")]
    public Color selectedColor, originalColor;
    public Image image;

    private void Awake() => DeSelect();
    public void Select() => image.color = selectedColor;
    public void DeSelect() => image.color = originalColor;
    public override void OnPointerClick(PointerEventData eventData) { }
    public void ChangeSelectedSLotWithClick(ToolbarSlot currentSlot)
    {
        int index = InventoryManager.Instance.selectedSlot;
        InventoryManager.Instance.ToolbarSlots[index].DeSelect();

        currentSlot.Select();
        index = System.Array.IndexOf(InventoryManager.Instance.ToolbarSlots, currentSlot);
        InventoryManager.Instance.selectedSlot = index;
    }
}
