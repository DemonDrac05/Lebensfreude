using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Manages inventory items and their interactions.
/// </summary>
public class InventoryItem : MonoBehaviour, IPointerClickHandler
{
    [Header("UI Components")]
    public Image image;
    public Slider slider;
    public Text countText;

    [Header("Item Properties")]
    [HideInInspector] public BaseItem item;
    [HideInInspector] public int count = 1;
    [HideInInspector] public Transform parentAfterMove;
    [HideInInspector] public ToolUsedManager tools;

    [Header("Movement Properties")]
    public bool isMoving = false;
    public Canvas canvas;
    private RectTransform canvasRectTransform;
    private RectTransform rectTransform;

    public static InventoryItem CurrentMovingItem { get; set; }

    private void Awake()
    {
        tools = FindObjectOfType<ToolUsedManager>();
        canvas = GetComponentInParent<Canvas>();
        canvasRectTransform = canvas.GetComponent<RectTransform>();
        rectTransform = GetComponent<RectTransform>();

        isMoving = false;
    }

    /// <summary>
    /// Initializes the item with the specified base item.
    /// </summary>
    /// <typeparam name="T">The type of the base item.</typeparam>
    /// <param name="newItem">The new item to initialize.</param>
    public void InitialiseItem<T>(T newItem) where T : BaseItem
    {
        item = newItem;
        switch (newItem)
        {
            case Product product:
                image.sprite = product.image;
                break;
            case Plant plant:
                image.sprite = plant.image;
                break;
            case Tool tool:
                image.sprite = tool.image;
                break;
            case CraftingItem craftingItem:
                image.sprite= craftingItem.image;
                break;
        }
        ActivateSlider(newItem);
    }

    private void ActivateSlider(ScriptableObject scriptableObject)
    {
        if (scriptableObject is Tool toolItem)
        {
            slider.gameObject.SetActive(toolItem.actionType == ActionType.Water);
            if (toolItem.actionType == ActionType.Water)
            {
                tools.waterSlider = slider;
            }
        }
        else
        {
            slider.gameObject.SetActive(false);
        }
        RefreshCount();
    }

    /// <summary>
    /// Refreshes the item count display.
    /// </summary>
    public void RefreshCount()
    {
        countText.text = count.ToString();
        countText.gameObject.SetActive(count > 1);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        BaseSlot inventorySlot = GetComponentInParent<BaseSlot>();
        if (inventorySlot != null)
        {
            inventorySlot.OnPointerClick(eventData);
        }
    }

    /// <summary>
    /// Picks up the specified inventory item.
    /// </summary>
    /// <param name="item">The item to pick up.</param>
    public void PickUpItem(InventoryItem item)
    {
        item.isMoving = true;
        CurrentMovingItem = item;
        item.parentAfterMove = item.transform.parent;
        item.transform.SetParent(item.canvas.transform);
        item.image.raycastTarget = false;
    }

    /// <summary>
    /// Drops the specified inventory item.
    /// </summary>
    /// <param name="item">The item to drop.</param>
    public void DropItem(InventoryItem item)
    {
        item.isMoving = false;
        CurrentMovingItem = null;
        item.transform.SetParent(item.parentAfterMove);
        item.transform.localPosition = Vector3.zero;
        item.image.raycastTarget = true;
    }

    /// <summary>
    /// Swaps two inventory items.
    /// </summary>
    /// <param name="item1">The first item.</param>
    /// <param name="item2">The second item.</param>
    public void SwapItems(InventoryItem item1, InventoryItem item2)
    {
        if (item1.GetItem<ScriptableObject>() == item2.GetItem<ScriptableObject>() &&
            (item1.item.MaxStackable > 1 || item2.item.MaxStackable > 1))
        {
            if (item2.count < item2.item.MaxStackable)
            {
                bool canStackMore = item1.count + item2.count < item2.item.MaxStackable;
                if (canStackMore)
                {
                    item2.count += item1.count;
                    Destroy(item1.gameObject);
                }
                else
                {
                    item1.count -= (item2.item.MaxStackable - item2.count);
                    item2.count = item2.item.MaxStackable;
                }
                item1.RefreshCount();
                item2.RefreshCount();
            }
        }
        else
        {
            SwapParents(item1, item2);
            DropItem(item1);
            PickUpItem(item2);
        }
    }

    private void SwapParents(InventoryItem item1, InventoryItem item2)
    {
        Transform tempParent = item1.parentAfterMove;
        item1.parentAfterMove = item2.transform.parent;
        item2.parentAfterMove = tempParent;

        item1.transform.SetParent(item1.parentAfterMove);
        item2.transform.SetParent(item2.parentAfterMove);
    }

    private void Update()
    {
        if (isMoving)
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRectTransform,
                Input.mousePosition,
                canvas.worldCamera,
                out Vector2 localPoint))
            {
                rectTransform.sizeDelta = new(70f, 70f);
                rectTransform.localPosition = localPoint;
            }
        }
    }

    /// <summary>
    /// Gets the item as the specified type.
    /// </summary>
    /// <typeparam name="T">The type to cast the item to.</typeparam>
    /// <returns>The item as the specified type.</returns>
    public T GetItem<T>() where T : ScriptableObject => item as T;
}
