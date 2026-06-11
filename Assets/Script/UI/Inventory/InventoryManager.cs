using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using TMPro;

/// <summary>
/// Manages the inventory, including items and slots.
/// </summary>
public class InventoryManager : MonoBehaviour
{
    [Header("=== Singleton Instance ==========")]
    public static InventoryManager Instance;

    [Header("=== Initial Items ==========")]
    public BaseItem[] initialItems;

    [Header("=== Inventory Slots ==========")]
    public ToolbarSlot[] ToolbarSlots;
    public InventorySlot[] MainInventorySlots;

    [Header("=== UI Elements ==========")]
    public GameObject toolbar;
    public GameObject mainInventory;
    public GameObject inventoryPrefab;
    [SerializeField] private TextMeshProUGUI tokenText;

    [Header("=== Inventory Properties ==========")]
    public int selectedSlot = -1;
    [HideInInspector] public static int token;

    private bool _invWasOpen = false;

    private void Awake()
    {
        token = 15000;
        Instance = this;
    }

    private void Start()
    {
        ChangeSelectedSlot(0);
        AddItems(initialItems);
    }

    private void Update()
    {
        HandleInventoryInput();
    }

    private void LateUpdate()
    {
        HandleSlotsMirror();
    }

    private void FixedUpdate() => tokenText.text = token.ToString();

    private void HandleInventoryInput()
    {
        HandleScrollInput();
        HandleNumberInput();
    }

    private void HandleScrollInput()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            scroll = -scroll;
            ChangeSelectedSlotWithScroll(scroll);
        }
    }

    private void HandleNumberInput()
    {
        if (int.TryParse(Input.inputString, out int number))
        {
            ChangeSelectedSlot(number == 0 ? 9 : number - 1);
            return;
        }
        switch (Input.inputString)
        {
            case "-":
            case "_":
                number = 10; break;
            case "=":
            case "+":
                number = 11; break;
            default:
                return;
        }
        ChangeSelectedSlot(number);
    }

    private void ChangeSelectedSlotWithScroll(float scrollValue)
    {
        int newSlot = selectedSlot;
        int toolBarLength = ToolbarSlots.Length;

        if (scrollValue > 0f)
        {
            newSlot = (selectedSlot + 1) % toolBarLength;
        }
        else if (scrollValue < 0f)
        {
            newSlot = (selectedSlot - 1 + toolBarLength) % toolBarLength;
        }

        ChangeSelectedSlot(newSlot);
    }

    public void ChangeSelectedSlot(int newValue)
    {
        if (selectedSlot >= 0)
        {
            ToolbarSlots[selectedSlot].DeSelect();
        }
        ToolbarSlots[newValue].Select();
        selectedSlot = newValue;
    }

    public bool AddItem<T>(T item) where T : BaseItem
    {
        if (AddItemToSlotArray(item, ToolbarSlots))
        {
            return true;
        }
        else
        {
            GameObject secondSlotObj = mainInventory.transform.Find("SecondSlots").gameObject;
            InventorySlot[] secondSlots = secondSlotObj.GetComponentsInChildren<InventorySlot>();
            if (AddItemToSlotArray(item, secondSlots))
            {
                return true;
            }
        }
        return false;
    }

    private bool AddItemToSlotArray<T>(T item, BaseSlot[] slots) where T : BaseItem
    {
        foreach (var slot in slots)
        {
            var itemInSlot = slot.GetComponentInChildren<InventoryItem>();
            if (itemInSlot != null && itemInSlot.GetItem<T>() == item && itemInSlot.count < GetMaxStackable(item))
            {
                itemInSlot.count++;
                itemInSlot.RefreshCount();
                return true;
            }
        }

        foreach (var slot in slots)
        {
            if (slot.GetComponentInChildren<InventoryItem>() == null)
            {
                SpawnNewItem(item, slot);
                return true;
            }
        }

        return false;
    }

    public void AddItems<T>(T[] items) where T : BaseItem
    {
        foreach (var item in items)
        {
            AddItem(item);
        }
    }

    private void SpawnNewItem<T>(T item, BaseSlot slot) where T : BaseItem
    {
        var newItemGameObject = Instantiate(inventoryPrefab, slot.transform);
        var inventoryItem = newItemGameObject.GetComponent<InventoryItem>();
        inventoryItem.InitialiseItem(item);
        newItemGameObject.SetActive(true);
    }

    private int GetMaxStackable<T>(T item) where T : BaseItem
    {
        return item.MaxStackable;
    }

    public T GetSelectedItem<T>(bool used) where T : BaseItem
    {
        var itemInSlot = ToolbarSlots[selectedSlot].GetComponentInChildren<InventoryItem>();
        if (itemInSlot != null)
        {
            var item = itemInSlot.GetItem<T>();
            if (used)
            {
                itemInSlot.count--;
                if (itemInSlot.count <= 0)
                {
                    Destroy(itemInSlot.gameObject);
                    GetSelectedItem<T>(false);
                }
                else
                {
                    itemInSlot.RefreshCount();
                }
            }
            return item;
        }
        return null;
    }

    private void HandleSlotsMirror()
    {
        if (mainInventory == null) return;
        bool invOpen = mainInventory.activeSelf;
        if (invOpen == _invWasOpen) return;

        if (invOpen)
            MirrorToolbarToFirstRow();
        else
            MirrorFirstRowToToolbar();

        _invWasOpen = invOpen;
    }

    public void SyncFirstRowFromToolbar() => MirrorToolbarToFirstRow();
    public void SyncToolbarFromFirstRow() => MirrorFirstRowToToolbar();

    private void MirrorToolbarToFirstRow()
    {
        for (int i = 0; i < ToolbarSlots.Length; i++)
        {
            var toolbarSlot = ToolbarSlots[i];
            var mainInventorySlot = MainInventorySlots[i];
            MirrorSlots(toolbarSlot, mainInventorySlot);
        }
    }

    private void MirrorFirstRowToToolbar()
    {
        for (int i = 0; i < ToolbarSlots.Length; i++)
        {
            var mainInventorySlot = MainInventorySlots[i];
            var toolbarSlot = ToolbarSlots[i];
            MirrorSlots(mainInventorySlot, toolbarSlot);
        }
    }

    public void MirrorSlots(BaseSlot sourceSlot, BaseSlot targetSlot)
    {
        if (targetSlot.transform.childCount > 0)
        {
            for (int i = targetSlot.transform.childCount - 1; i >= 0; i--)
            {
                Destroy(targetSlot.transform.GetChild(i).gameObject);
            }
        }
        if (sourceSlot.transform.childCount > 0)
        {
            var sourceItem = sourceSlot.GetComponentInChildren<InventoryItem>(true);

            if (sourceItem != null)
            {
                var newItem = Instantiate(sourceItem.gameObject, targetSlot.transform);
                newItem.GetComponent<InventoryItem>().InitialiseItem(sourceItem.GetItem<BaseItem>());
                newItem.GetComponent<InventoryItem>().count = sourceItem.count;
                newItem.GetComponent<InventoryItem>().RefreshCount();

                newItem.name = sourceItem.gameObject.name.Replace("(Clone)", "");

                newItem.SetActive(true);

                var clonedItem = newItem.GetComponent<InventoryItem>();
                if (clonedItem != null)
                {
                    clonedItem.enabled = true;
                    if (clonedItem.image != null) clonedItem.image.enabled = true;
                }
            }
        }
    }

    private List<BaseSlot> GetRealStorageSlots()
    {
        var list = new List<BaseSlot>();
        if (ToolbarSlots != null) list.AddRange(ToolbarSlots);
        var second = mainInventory != null ? mainInventory.transform.Find("SecondSlots") : null;
        if (second != null) list.AddRange(second.GetComponentsInChildren<InventorySlot>());
        return list;
    }

    public int CountItem(BaseItem item)
    {
        if (item == null) return 0;
        int total = 0;
        foreach (var slot in GetRealStorageSlots())
        {
            var it = slot.GetComponentInChildren<InventoryItem>();
            if (it != null && it.GetItem<BaseItem>() == item) total += it.count;
        }
        return total;
    }

    public bool RemoveItem(BaseItem item, int amount)
    {
        if (item == null || amount <= 0) return false;
        if (CountItem(item) < amount) return false;

        int remaining = amount;
        foreach (var slot in GetRealStorageSlots())
        {
            if (remaining <= 0) break;
            var it = slot.GetComponentInChildren<InventoryItem>();
            if (it == null || it.GetItem<BaseItem>() != item) continue;

            int take = Mathf.Min(it.count, remaining);
            it.count   -= take;
            remaining  -= take;
            if (it.count <= 0) Destroy(it.gameObject);
            else it.RefreshCount();
        }
        return true;
    }

    public static void AddToken(int amount) => token += Mathf.Max(0, amount);
    public static bool SpendToken(int amount)
    {
        Debug.Log("Start spending token");
        if (amount <= 0) return true;
        if (token < amount) return false;
        token -= amount;
        Debug.Log(token);
        return true;
    }

    public List<KeyValuePair<BaseItem, int>> GetAllStacks()
    {
        var list = new List<KeyValuePair<BaseItem, int>>();
        foreach (var slot in GetRealStorageSlots())
        {
            var it = slot.GetComponentInChildren<InventoryItem>(true);
            if (it != null) { var bi = it.GetItem<BaseItem>(); if (bi != null) list.Add(new KeyValuePair<BaseItem, int>(bi, it.count)); }
        }
        return list;
    }

    public void ClearAll()
    {
        foreach (var slot in GetRealStorageSlots())
        {
            var it = slot.GetComponentInChildren<InventoryItem>(true);
            if (it != null) Destroy(it.gameObject);
        }
    }
}
