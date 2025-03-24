using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Experimental.XR.Interaction;

public class MaterialUpgradeSlot : SmithingSlots
{
    [Header("=== Singleton Instance ==========")]
    public static MaterialUpgradeSlot Instance;

    [Header("=== Required Slots ==========")]
    public GameObject outputSlot;

    [Header("=== UI Components ==========")]
    public TextMeshProUGUI materialCountText;

    [Header("=== Default Prefab ==========")]
    public GameObject inventoryPrefab;

    public int materialQuantity;
    public bool reachQuantiy;

    private void Awake() => Instance = this;

    private void Update() 
        => materialCountText.gameObject.SetActive(GetComponentInChildren<InventoryItem>() != null);

    public void RefreshMaterialsNotUsed(GameObject materialSlot)
    {
        var materialNeeded = materialSlot.GetComponentInChildren<InventoryItem>();
        for(int i = 0; i < materialNeeded.count - 1; i++)
        {
            InventoryManager.Instance.AddItem(materialNeeded.GetItem<Materials>());
        }
    }

    public void UpdateGUI(InventoryItem itemInSlot)
    {
        if (itemInSlot != null)
        {
            UpdateMaterialQuantity(itemInSlot);
            UpdateColorOfItem(itemInSlot);
            UpdateColorOfText(itemInSlot);
        }
    }

    private void UpdateMaterialQuantity(InventoryItem itemInSlot)
    {
        var outputTool = outputSlot.GetComponentInChildren<InventoryItem>().GetItem<Tool>();
        materialQuantity = outputTool.materialBox.quantity;
        reachQuantiy = (itemInSlot.count - 1) >= materialQuantity;
    }

    private void UpdateColorOfItem(InventoryItem itemInSlot)
    {
        var itemImage = itemInSlot.image;
        Color color = itemImage.color;
        color.a = reachQuantiy ? 1f : 0.5f;
        itemImage.color = color;
    }

    private void UpdateColorOfText(InventoryItem itemInSlot)
    {
        var materialInSlot = itemInSlot.count - 1;
        materialCountText.text = $"{materialInSlot} / {materialQuantity}";
        materialCountText.color = reachQuantiy ? Color.white : Color.red;
    }

    #region === OVERRIDE METHODS =========
    public override void HandleMouseInput(PointerEventData eventData)
    {
        if (UpgradeButton.Instance.enableToInteract)
        {
            base.HandleMouseInput(eventData);
            UpdateGUI(GetComponentInChildren<InventoryItem>());
        }
    }

    #region === LEFT CLICK =========
    public override void OnLeftClickWithOccupiedCursor(InventoryItem itemInSlot)
    {
        if (itemInSlot != null)
        {
            var itemInHand = InventoryItem.CurrentMovingItem;
            if (itemInHand.GetItem<Materials>() == itemInSlot.GetItem<Materials>())
            {
                itemInSlot.count += itemInHand.count;
                Destroy(itemInHand.gameObject);
            }
        }
    }

    public override void PickUpItemWithLeftClick(InventoryItem itemInSlot)
    {
        if (itemInSlot.count > 1)
        {
            InventoryItem newItem = CreateNewItem(transform, itemInSlot.GetItem<Materials>());
            newItem.count = itemInSlot.count - 1;
            itemInSlot.count -= newItem.count;

            newItem.PickUpItem(newItem);
            newItem.RefreshCount();
        }
    }
    #endregion

    #region === RIGHT CLICK =========
    public override void OnRightClickWithOccupiedCursor(InventoryItem itemInSlot)
    {
        InventoryItem inventoryItem = InventoryItem.CurrentMovingItem;
        if (itemInSlot != null)
        {
            base.MergeItemsWithRightClick(inventoryItem, itemInSlot);
        }
        base.RefreshInventory(inventoryItem);
    }

    public override void OnRightClickWithEmptyCursor(InventoryItem itemInSlot)
    {
        if (itemInSlot != null)
        {
            base.SplitItemWithRightClick(itemInSlot);
        }
    }

    public override void SplitItemWithRightClick(InventoryItem itemInSlot)
    {
        if(itemInSlot.count > 2)
        {
            base.SplitItemWithRightClick(itemInSlot);
        }
    }
    #endregion

    #endregion
}
