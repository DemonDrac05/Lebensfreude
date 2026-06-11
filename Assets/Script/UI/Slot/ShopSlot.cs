using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ShopSlot : BaseSlot
{
    [HideInInspector] public Shop shopType;

    private void Awake() => shopType = GetComponentInParent<Shop>();

    private void Start() => RefreshItemImage(cachedItem);

    public override void OnPointerClick(PointerEventData eventData)
    {
        var itemInHand = InventoryItem.CurrentMovingItem;
        if (itemInHand != null)
        {
            DropItemWithLeftClick(itemInHand);
        }
        else
        {
            cachedItem = GetComponentInChildren<InventoryItem>();
            if (cachedItem != null)
            {
                if (shopType.CanSellItem(cachedItem.GetItem<BaseItem>()))
                {
                    SellCachedItem(cachedItem);
                }
            }
        }
        RefreshItemImage(cachedItem);
    }

    public override void DropItemWithLeftClick(InventoryItem itemInHand)
    {
        var itemInSlot = GetComponentInChildren<InventoryItem>();
        if (itemInSlot == null)
        {
            base.DropItemWithLeftClick(itemInHand);
        }
        else if (itemInSlot != null
                && itemInSlot.count < itemInSlot.GetItem<BaseItem>().MaxStackable
                && itemInSlot.GetItem<BaseItem>() == itemInHand.GetItem<BaseItem>())
        {
            int maxStack = itemInSlot.GetItem<BaseItem>().MaxStackable;
            if (itemInSlot.count + itemInHand.count <= maxStack)
            {
                itemInSlot.count += itemInHand.count;
                Destroy(itemInHand.gameObject);
            }
            else if (itemInSlot.count + itemInHand.count > maxStack)
            {
                itemInHand.count -= (maxStack - itemInSlot.count);
                itemInSlot.count = maxStack;
                itemInHand.RefreshCount();
            }
            itemInSlot.RefreshCount();
        }
    }

    private void RefreshItemImage(InventoryItem cachedItem)
    {
        if (cachedItem != null)
        {
            if (!shopType.CanSellItem(cachedItem.GetItem<BaseItem>()))
            {
                Color color = cachedItem.image.color;
                color.a = 0.5f;
                cachedItem.image.color = color;
            }
        }
    }

    private void SellCachedItem(InventoryItem soldSlotItem)
    {
        BaseItem soldItem = soldSlotItem.GetItem<BaseItem>();
        var market = GetComponentInParent<VillageMarket>();
        if (market != null)
        {
            int coins = market.RegisterSale(soldItem, soldSlotItem.count);
            if (coins <= 0) return;
            InventoryManager.token += coins;
        }
        else
        {
            InventoryManager.token += soldItem.sellingPrice * soldSlotItem.count;
        }
        Destroy(soldSlotItem.gameObject);
    }
}
