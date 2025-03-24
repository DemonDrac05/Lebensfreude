using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class InputUpgradeSlot : SmithingSlots
{
    [Header("=== UI Components ==========")]
    public TextMeshProUGUI feeToUpgradeText;
    private const string UpgradeButtonText = "Upgrade";

    [Header("=== Required Slots ==========")]
    public GameObject outputSlot;
    public GameObject materialSlot;

    private void UpdateGUI()
    {
        var itemInSlot = GetComponentInChildren<InventoryItem>();
        if (itemInSlot != null)
        {
            UpdateSlot(outputSlot, itemInSlot.GetItem<Tool>()?.upgradedTool);
            UpdateSlot(materialSlot, itemInSlot.GetItem<Tool>()?.upgradedTool?.materialBox?.material);
        }
        feeToUpgradeText.text = 
            itemInSlot?.GetItem<Tool>()?.upgradedTool?.feeToUpgrade.ToString() ?? UpgradeButtonText;

        MaterialUpgradeSlot.Instance.UpdateGUI(materialSlot.GetComponentInChildren<InventoryItem>());
    }

    private void UpdateSlot(GameObject slot, BaseItem newItem)
    {
        if (newItem != null)
        {
            InitialiseItem(slot.transform, newItem);
        }
    }

    private void InitialiseItem(Transform slot, BaseItem itemToInitialise)
    {
        InventoryItem newItem = CreateNewItem(slot.transform, itemToInitialise);
        InitialiseColorOfItem(newItem);
    }

    private void InitialiseColorOfItem(InventoryItem item)
    {
        var color = item.image.color;
        color.a = 0.5f;
        item.image.color = color;
    }

    private void ClearSlots()
    {
        if (materialSlot.GetComponentInChildren<InventoryItem>()?.count > 1)
        {
            MaterialUpgradeSlot.Instance.RefreshMaterialsNotUsed(materialSlot);
        }
        DestroySlotItem(materialSlot);
        DestroySlotItem(outputSlot);
    }

    private void DestroySlotItem(GameObject slot)
    {
        var itemInSlot = slot.GetComponentInChildren<InventoryItem>();
        if (itemInSlot != null)
        {
            DestroyImmediate(itemInSlot.gameObject);
        }
    }


    #region ==== OVERRIDE METHODS ===========
    public override void HandleMouseInput(PointerEventData eventData)
    {
        if (UpgradeButton.Instance.enableToInteract)
        {
            var itemInSlot = GetComponentInChildren<InventoryItem>();
            base.OnLeftClick(itemInSlot);
            UpdateGUI();
        }
    }

    public override void OnLeftClickWithOccupiedCursor(InventoryItem itemInSlot)
    {
        if(InventoryItem.CurrentMovingItem.GetItem<Tool>() != null)
        {
            base.OnLeftClickWithOccupiedCursor(itemInSlot);
        }
    }

    public override void SwapItemsFromSlot(InventoryItem itemOnCursor, InventoryItem itemInSlot)
    {
        ClearSlots();
        base.SwapItemsFromSlot(itemOnCursor, itemInSlot);
    }

    public override void PickUpItemWithLeftClick(InventoryItem inventoryItem)
    {
        base.PickUpItemWithLeftClick(inventoryItem);
        ClearSlots();
    }
    #endregion
}
