using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    public Item[] startItems;

    public int maxStackableItem = 999;

    public InventorySlot[] InventorySlots;

    public GameObject inventoryPrefab;

    public int selectedSlot = -1;

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        ChangeSelectedSlot(0);
        foreach (var item in startItems)
        {
            AddItem(item);
        }
    }

    private void Update()
    {
        if(Input.inputString != null)
        {
            bool isNumber = int.TryParse(Input.inputString, out int number);
            if(isNumber &&  number > 0 && number < 11)
            {
                ChangeSelectedSlot(number - 1);
            }
            else if(isNumber && number ==0)
            {
                ChangeSelectedSlot(9);
            }
        }
    }
    public void ChangeSelectedSlot(int newValue)
    {
        if (selectedSlot >= 0) InventorySlots[selectedSlot].DeSelect();
        InventorySlots[newValue].Select();
        selectedSlot = newValue;
    }
    public bool AddItem(Item item)
    {
        for (int i = 0; i < InventorySlots.Length; i++)
        {
            InventorySlot slot = InventorySlots[i];
            InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
            if (itemInSlot != null &&
               itemInSlot.item == item &&
               itemInSlot.count < maxStackableItem &&
               itemInSlot.item.stackable == true)
            {
                itemInSlot.count++;
                itemInSlot.RefreshCount();
                return true;
            }
        }

        for (int i = 0; i < InventorySlots.Length; i++)
        {
            InventorySlot slot = InventorySlots[i];
            InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
            if(itemInSlot == null)
            {
                SpawnNewItem(item, slot);
                return true;
            }
        }

        return false;
    }

    public void SpawnNewItem(Item item, InventorySlot slot)
    {
        GameObject newItemGameObject = Instantiate(inventoryPrefab, slot.transform);
        InventoryItem inventoryItem = newItemGameObject.GetComponent<InventoryItem>();
        inventoryItem.InitialiseItem(item);
    }

    public Item GetSelectedItem(bool used)
    {
        InventorySlot slot = InventorySlots[selectedSlot];
        InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
        if (itemInSlot != null) 
        { 
            Item item = itemInSlot.item;
            if (used == true)
            {
                itemInSlot.count--;
                if (itemInSlot.count <= 0)
                {
                    Destroy(itemInSlot.gameObject);
                    GetSelectedItem(false);
                }
                else itemInSlot.RefreshCount();
            }
            return item; 
        }
        return null;
    }
}
