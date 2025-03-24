using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopItemManager : MonoBehaviour
{
    [Header("=== GameObjects Requirement ==========")]
    public GameObject shopSlot;
    public GameObject mainInventory;
    private GameObject copyMainInventory = null;

    [Header("=== Inventory Slots ==========")]
    private ShopSlot[] mainSlots;
    private ShopSlot[] secondSlots;
    private ShopSlot[] copySlots;

    private void OnEnable()
    {
        if (copyMainInventory == null)
        {
            copyMainInventory = Instantiate(mainInventory, transform);
            copyMainInventory.SetActive(true);

            RectTransform rectTransform = copyMainInventory.GetComponent<RectTransform>();
            rectTransform.localPosition = new Vector2(240f, -240f);

            GameObject firstRow = copyMainInventory.transform.Find("MainSlots").gameObject;
            GameObject otherRow = copyMainInventory.transform.Find("SecondSlots").gameObject;

            mainSlots = ReplaceInventorySlots(firstRow);
            secondSlots = ReplaceInventorySlots(otherRow);

            List<ShopSlot> combinedSlotsList = new List<ShopSlot>(mainSlots);
            combinedSlotsList.AddRange(secondSlots);
            copySlots = combinedSlotsList.ToArray();

            ClearSlots(copySlots);
            StartCoroutine(CopyInventory());
        }
    }

    private ShopSlot[] ReplaceInventorySlots(GameObject slotParent)
    {
        InventorySlot[] oldSlots = slotParent.GetComponentsInChildren<InventorySlot>();
        foreach (var slot in oldSlots)
        {
            Destroy(slot.gameObject);
            Instantiate(shopSlot, slotParent.transform);
        }
        return slotParent.GetComponentsInChildren<ShopSlot>();
    }

    private IEnumerator CopyInventory()
    {
        yield return new WaitForEndOfFrame();

        for (int i = 0; i < InventoryManager.Instance.MainInventorySlots.Length; i++)
        {
            var copySlot = copySlots[i];
            var mainInventorySlot = InventoryManager.Instance.MainInventorySlots[i];
            InventoryManager.Instance.MirrorSlots(mainInventorySlot, copySlot);
        }

        for (int i = 0; i < copySlots.Length; i++)
        {
            var itemInSlot = copySlots[i].GetComponentInChildren<InventoryItem>();
            RefreshItemImage(itemInSlot);
        }
    }

    private void RefreshItemImage(InventoryItem cachedItem)
    {
        if (cachedItem != null)
        {
            Shop shopType = GetComponent<Shop>();
            if (!shopType.CanSellItem(cachedItem.GetItem<BaseItem>()))
            {
                Color color = cachedItem.image.color;
                color.a = 0.5f;
                cachedItem.image.color = color;
            }
        }
    }

    private void OnDisable()
    {
        MirrorSlotsToToolBar();
        MirrorSlotsToMainInventory();

        ClearSlots(secondSlots);
        ClearSlots(mainSlots);
        ClearSlots(copySlots);

        Destroy(copyMainInventory?.gameObject);
        copyMainInventory = null;
    }

    public void ClearSlots(ShopSlot[] slots)
    {
        foreach (var slot in slots)
        {
            for (int i = slot.transform.childCount - 1; i >= 0; i--)
            {
                Destroy(slot.transform.GetChild(i).gameObject);
            }
        }
    }

    private void MirrorSlotsToToolBar()
    {
        for (int i = 0; i < InventoryManager.Instance.ToolbarSlots.Length; i++)
        {
            var copySlot = copySlots[i];
            var toolbarSlot = InventoryManager.Instance.ToolbarSlots[i];
            InventoryManager.Instance.MirrorSlots(copySlot, toolbarSlot);
        }
    }

    private void MirrorSlotsToMainInventory()
    {
        for (int i = 0; i < InventoryManager.Instance.MainInventorySlots.Length; i++)
        {
            var copySlot = copySlots[i];
            var mainInventorySlot = InventoryManager.Instance.MainInventorySlots[i];
            InventoryManager.Instance.MirrorSlots(copySlot, mainInventorySlot);
        }
    }
}
