using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class OutputUpgradeSlot : SmithingSlots
{
    private bool CheckCompleteUpgrade(InventoryItem itemInSlot)
    {
        Color color = itemInSlot.image.color;
        return color.a == 1f;
    }
    public override void HandleMouseInput(PointerEventData eventData)
    {
        var itemInSlot = GetComponentInChildren<InventoryItem>();
        if (itemInSlot != null)
        {
            if (CheckCompleteUpgrade(itemInSlot) && InventoryItem.CurrentMovingItem == null)
            {
                base.PickUpItemWithLeftClick(itemInSlot);
            }
        }
    }
}
