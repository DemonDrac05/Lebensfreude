using UnityEngine;
using UnityEngine.EventSystems;

public class ChestSlot : BaseSlot
{
    private GameObject chestUIParent;
    private GameObject chestInventoryParent;

    private void InitiateSlotsParent()
    {
        GameObject chestList = transform.parent?.parent?.parent.gameObject;
        chestUIParent = chestList?.transform.Find("[Chest]_UIBoard").gameObject;
        chestInventoryParent = chestList?.transform.Find("[Chest]_CopyMainInventory").gameObject;
    }

    private void Start() => InitiateSlotsParent();

    public override void HandleMouseInput(PointerEventData eventData)
    {
        cachedItem = GetComponentInChildren<InventoryItem>();

        if ((eventData.button == PointerEventData.InputButton.Left || eventData.button == PointerEventData.InputButton.Right)
            && cachedItem != null)
        {
            QuickItemInput(cachedItem, eventData);
        }
    }

    private void QuickItemInput(InventoryItem itemInSlot, PointerEventData eventData)
    {
        Transform grandParent = transform.parent?.parent;
        ChestSlot[] slots = null;

        if (grandParent.gameObject == chestUIParent)
        {
            slots = chestInventoryParent.GetComponentsInChildren<ChestSlot>();
        }
        else if (grandParent.gameObject == chestInventoryParent)
        {
            slots = chestUIParent.GetComponentsInChildren<ChestSlot>();
        }
        Debug.Log(grandParent.gameObject.name);

        AddItem(itemInSlot, slots, eventData);
    }

    private void AddItem(InventoryItem sourceItem, ChestSlot[] slots, PointerEventData eventData)
    {
        if (AddItemToSlotArray(sourceItem, slots, eventData) || sourceItem.count == 0)
        {
            Destroy(sourceItem.gameObject);
        }
        else
        {
            sourceItem.RefreshCount();
        }
    }

    private bool AddItemToSlotArray(InventoryItem sourceItem, ChestSlot[] slots, PointerEventData eventData)
    {
        foreach (var slot in slots)
        {
            var itemInSlot = slot.GetComponentInChildren<InventoryItem>();
            if (itemInSlot != null
                && itemInSlot.count < itemInSlot.GetItem<BaseItem>().MaxStackable
                && itemInSlot.GetItem<BaseItem>() == sourceItem.GetItem<BaseItem>())
            {
                return AllocateItemToSlot(sourceItem, itemInSlot, eventData);
            }
        }
        foreach (var slot in slots)
        {
            if (slot.GetComponentInChildren<InventoryItem>() == null)
            {
                return SpawnNewItemToSlot(sourceItem, slot.transform, eventData);
            }
        }
        return false;
    }

    private bool AllocateItemToSlot(InventoryItem sourceItem, InventoryItem itemInSlot, PointerEventData eventData)
    {
        int maxStack = itemInSlot.GetItem<BaseItem>().MaxStackable;
        bool isLeftClick = eventData.button == PointerEventData.InputButton.Left;
        bool isRightClick = eventData.button == PointerEventData.InputButton.Right;
        bool isShiftHeld = Input.GetKey(KeyCode.LeftShift);

        if (isLeftClick)
        {
            return HandleLeftClickAllocation(sourceItem, itemInSlot, maxStack);
        }
        else if (isRightClick)
        {
            return HandleRightClickAllocation(sourceItem, itemInSlot, maxStack, isShiftHeld);
        }
        return false;
    }

    private bool HandleLeftClickAllocation(InventoryItem sourceItem, InventoryItem itemInSlot, int maxStack)
    {
        if (itemInSlot.count + sourceItem.count <= maxStack)
        {
            itemInSlot.count += sourceItem.count;
            itemInSlot.RefreshCount();
            return true;
        }
        else
        {
            sourceItem.count = maxStack - itemInSlot.count;
            itemInSlot.count = maxStack;
            itemInSlot.RefreshCount();
            return false;
        }
    }

    private bool HandleRightClickAllocation(InventoryItem sourceItem, InventoryItem itemInSlot, int maxStack, bool isShiftHeld)
    {
        if (isShiftHeld)
        {
            int quantityToAllocate = sourceItem.count / 2;
            if (sourceItem.count == 1)
            {
                itemInSlot.count++;
                sourceItem.count = 0;
                itemInSlot.RefreshCount();
                return true;
            }
            else if (itemInSlot.count + quantityToAllocate <= maxStack)
            {
                itemInSlot.count += quantityToAllocate;
                sourceItem.count -= quantityToAllocate;
                itemInSlot.RefreshCount();
                return false;
            }
            else if (itemInSlot.count + quantityToAllocate > maxStack)
            {
                sourceItem.count = maxStack - itemInSlot.count;
                itemInSlot.count = maxStack;
                itemInSlot.RefreshCount();
                return false;
            }
        }
        else
        {
            if (sourceItem.count == 1)
            {
                itemInSlot.count++;
                sourceItem.count = 0;
                itemInSlot.RefreshCount();
                return true;
            }
            else if (itemInSlot.count++ <= maxStack)
            {
                itemInSlot.count++;
                sourceItem.count--;
                itemInSlot.RefreshCount();
                return false;
            }
            else if (itemInSlot.count++ > maxStack)
            {
                sourceItem.count--;
                itemInSlot.count = maxStack;
                itemInSlot.RefreshCount();
                return false;
            }
        }
        return false;
    }

    private bool SpawnNewItemToSlot(InventoryItem sourceItem, Transform transform, PointerEventData eventData)
    {
        GameObject newItemInChest = Instantiate(sourceItem.gameObject, transform);
        var checkItem = newItemInChest.GetComponent<InventoryItem>();

        bool isLeftClick = eventData.button == PointerEventData.InputButton.Left;
        bool isRightClick = eventData.button == PointerEventData.InputButton.Right;
        bool isShiftHeld = Input.GetKey(KeyCode.LeftShift);

        if (isLeftClick)
        {
            checkItem.count = sourceItem.count;
        }
        else if (isRightClick)
        {
            if (isShiftHeld)
            {
                int quantityToAllocate = sourceItem.count / 2;
                checkItem.count = quantityToAllocate;
                sourceItem.count -= quantityToAllocate;
            }
            else
            {
                checkItem.count = 1;
                sourceItem.count--;
            }
            checkItem.RefreshCount();
            return false;
        }
        checkItem.RefreshCount();
        return true;
    }
}
