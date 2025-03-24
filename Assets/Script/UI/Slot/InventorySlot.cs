using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using UnityEditor.Rendering.LookDev;

public class InventorySlot : BaseSlot
{
    public override void OnPointerClick(PointerEventData eventData)
    {
        bool isShiftHeld = Input.GetKey(KeyCode.LeftShift);
        if (isShiftHeld)
        {
            HandleMouseInputWithLeftShift(eventData);
        }
        else
        {
            base.HandleMouseInput(eventData);
        }
    }

    public void HandleMouseInputWithLeftShift(PointerEventData eventData)
    {
        cachedItem = GetComponentInChildren<InventoryItem>();

        if (cachedItem != null)
        {
            QuickTransitionWithMouseInput(cachedItem, eventData);
        }
    }

    private void QuickTransitionWithMouseInput(InventoryItem itemInSlot, PointerEventData eventData)
    {
        int index = Array.IndexOf(InventoryManager.Instance.MainInventorySlots, itemInSlot.GetComponentInParent<InventorySlot>());
        int newIndex = CheckEmptySlot(eventData, itemInSlot, index);
        if (newIndex != -1)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                CreateNewObjectToSlot(itemInSlot, newIndex, itemInSlot.count);
                Destroy(itemInSlot.gameObject);
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                if (itemInSlot.count == 1)
                {
                    CreateNewObjectToSlot(itemInSlot, newIndex, itemInSlot.count);
                    Destroy(itemInSlot.gameObject);
                }
                else
                {
                    int newQuantity = itemInSlot.count / 2;
                    itemInSlot.count -= newQuantity;

                    CreateNewObjectToSlot(itemInSlot, newIndex, newQuantity);
                    itemInSlot.RefreshCount();
                }
            }
        }
    }

    private void CreateNewObjectToSlot(InventoryItem itemInSlot, int slotIndex, int quantity)
    {
        GameObject newTransaction
                = Instantiate(itemInSlot.gameObject, InventoryManager.Instance.MainInventorySlots[slotIndex].transform);
        InventoryItem newItem = newTransaction.GetComponent<InventoryItem>();
        newItem.count = quantity;
        newItem.RefreshCount();
    }

    private int CheckEmptySlot(PointerEventData eventData, InventoryItem sourceItem, int currentIndex)
    {
        const int MAX_FIRSTROW_SIZE  = 12;
        const int MAX_INVENTORY_SIZE = 36;

        int beginIndex = currentIndex < MAX_FIRSTROW_SIZE ? MAX_FIRSTROW_SIZE  : 0;
        int endIndex   = currentIndex < MAX_FIRSTROW_SIZE ? MAX_INVENTORY_SIZE : MAX_FIRSTROW_SIZE;

        if (ItemAvailableInSlots(eventData, sourceItem, beginIndex, endIndex))
        {
            for (int index = beginIndex; index < endIndex; index++)
            {
                if (InventoryManager.Instance.MainInventorySlots[index].GetComponentInChildren<InventoryItem>() == null)
                {
                    return index;
                }
            }
        }
        else
        {
            if (sourceItem.count == 0)
            {
                Destroy(sourceItem.gameObject);
            }
        }
        return -1;
    }

    private bool ItemAvailableInSlots(PointerEventData eventData, InventoryItem sourceItem, int beginIndex, int endIndex)
    {
        for (int index = 0; index < InventoryManager.Instance.MainInventorySlots.Length; index++)
        {
            InventoryItem itemInSlot = InventoryManager.Instance.MainInventorySlots[index].GetComponentInChildren<InventoryItem>();
            if (itemInSlot != null 
                && itemInSlot.count < itemInSlot.GetItem<BaseItem>().MaxStackable
                && itemInSlot.GetItem<BaseItem>() == sourceItem.GetItem<BaseItem>()
                && itemInSlot.GetComponentInParent<InventorySlot>() != sourceItem.GetComponentInParent<InventorySlot>())
            {
                int maxStack = itemInSlot.GetItem<BaseItem>().MaxStackable;
                if (sourceItem.count == 1)
                {
                    itemInSlot.count++;
                    sourceItem.count--;
                }
                else
                {
                    if (eventData.button == PointerEventData.InputButton.Left)
                    {
                        RefreshItemQuantity(itemInSlot, sourceItem.count);
                        sourceItem.count = 0;
                    }
                    else if (eventData.button == PointerEventData.InputButton.Right)
                    {
                        int newQuantiy = sourceItem.count / 2;
                        RefreshItemQuantity(itemInSlot, newQuantiy);
                        sourceItem.count -= newQuantiy;
                    }
                }
                itemInSlot.RefreshCount();
                sourceItem.RefreshCount();
                return false;
            }
        }
        return true;
    }

    private void RefreshItemQuantity(InventoryItem itemInSlot, int quantity)
    {
        int maxStack = itemInSlot.GetItem<BaseItem>().MaxStackable;
        if (itemInSlot.count + quantity <= maxStack)
        {
            itemInSlot.count += quantity;
        }
        else if (itemInSlot.count + quantity > maxStack)
        {
            quantity -= maxStack - itemInSlot.count;
            itemInSlot.count = maxStack;
        }
    }

    public override void OnLeftClickWithEmptyCursor(InventoryItem itemInSlot)
    {
        if (itemInSlot != null)
        {
            base.PickUpItemWithLeftClick(itemInSlot);
        }
    }
}

