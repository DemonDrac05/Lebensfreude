using System.Collections;
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
    public Product[] startProducts;
    public Plant[] startPlants;
    public Tool[] startTools;
    public CraftingItem[] startCraftingItems;

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

    private void Awake()
    {
        token = 500;
        Instance = this;
    }

    private void Start()
    {
        ChangeSelectedSlot(0);
        AddItems(startProducts);
        AddItems(startPlants);
        AddItems(startTools);
        AddItems(startCraftingItems);
    }

    private void Update()
    {
        HandleInventoryInput();
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
        return item switch
        {
            Product product => product.MaxStackable,
            Plant plant => plant.MaxStackable,
            Tool tool => tool.MaxStackable,
            CraftingItem craftingItem => craftingItem.MaxStackable,
            _ => 999
        };
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
        if (toolbar.activeSelf)
        {
            MirrorToolbarToFirstRow();
        }
        else
        {
            MirrorFirstRowToToolbar();
        }
    }

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
            var sourceItem = sourceSlot.GetComponentInChildren<InventoryItem>();

            if (sourceItem != null)
            {
                var newItem = Instantiate(sourceItem.gameObject, targetSlot.transform);
                newItem.GetComponent<InventoryItem>().InitialiseItem(sourceItem.GetItem<BaseItem>());
                newItem.GetComponent<InventoryItem>().count = sourceItem.count;
                newItem.GetComponent<InventoryItem>().RefreshCount();

                newItem.name = sourceItem.gameObject.name.Replace("(Clone)", "");

                newItem.SetActive(true);
            }
        }
    }
}
