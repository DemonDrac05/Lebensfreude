using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShopItemSlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("UI Controller")]
    public Image backGroundImage;
    public Image itemImage;

    public Color hoverColor;
    public Color originalColor;
    public TextMeshProUGUI sellPriceText;

    [Header("Item (ScriptableObject)")]
    public BaseItem sellItem;
    public GameObject inventoryPrefab;

    private bool leftMouseHeld = false;
    private bool rightMouseHeld = false;
    private float timerHeld = 0f;

    [HideInInspector] public ItemCategory category;

    private void Awake() => category = FindObjectOfType<ItemCategory>();

    private void Start() => CategorizeItem(itemImage);

    public void OnPointerEnter(PointerEventData eventData) => backGroundImage.color = hoverColor;

    public void OnPointerExit(PointerEventData eventData) => backGroundImage.color = originalColor;

    private void Update()
    {
        if (leftMouseHeld)
        {
            timerHeld += Time.deltaTime;
            if (timerHeld > 1f)
            {
                PurchaseItem(1);
            }
        }
        else if (rightMouseHeld)
        {
            timerHeld += Time.deltaTime;
            if (timerHeld > 1f)
            {
                PurchaseItem(5);
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            leftMouseHeld = true;
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            rightMouseHeld = true;
        }
        timerHeld = 0f;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            leftMouseHeld = false;
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            rightMouseHeld = false;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            PurchaseItem(1);
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            PurchaseItem(5);
        }
    }

    private void PurchaseItem(int quantity)
    {
        if (sellItem != null && InventoryManager.token >= (quantity * sellItem.buyingPrice))
        {
            if (InventoryItem.CurrentMovingItem != null)
            {
                if (InventoryItem.CurrentMovingItem.image.sprite == sellItem.image)
                {
                    InventoryItem.CurrentMovingItem.count += quantity;
                    InventoryItem.CurrentMovingItem.RefreshCount();
                }
            }
            else if (InventoryItem.CurrentMovingItem == null)
            {
                GameObject purchaseItem = Instantiate(inventoryPrefab, this.transform);
                var inventoryItem = purchaseItem.GetComponent<InventoryItem>();

                inventoryItem.InitialiseItem(sellItem);
                inventoryItem.count = quantity;
                inventoryItem.RefreshCount();

                inventoryItem.PickUpItem(inventoryItem);
            }
            InventoryManager.token -= (quantity * sellItem.buyingPrice);
        }
    }

    private void CategorizeItem(Image itemImageRenderer)
    {
        if (itemImageRenderer != null)
        {
            Sprite itemImage = itemImageRenderer.sprite;

            if (TryCategorizeItem(itemImage, category.products, out Product product))
            {
                InventoryManager.Instance.AddItem(product);
                sellItem = product;
            }
            else if (TryCategorizeItem(itemImage, category.plants, out Plant plant))
            {
                InventoryManager.Instance.AddItem(plant);
                sellItem = plant;
            }
            else if (TryCategorizeItem(itemImage, category.tools, out Tool tool))
            {
                InventoryManager.Instance.AddItem(tool);
                sellItem = tool;
            }
            sellPriceText.text = sellItem.buyingPrice.ToString();
        }
    }

    private bool TryCategorizeItem<T>(Sprite itemImage, T[] items, out T foundItem) where T : BaseItem
    {
        foreach (T item in items)
        {
            if (item.image == itemImage)
            {
                foundItem = item;
                return true;
            }
        }
        foundItem = null;
        return false;
    }
}
