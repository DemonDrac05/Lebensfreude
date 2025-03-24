using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    private ItemCategory category;

    private void Awake()
    {
        category = FindObjectOfType<ItemCategory>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("CollectibleItem"))
        {
            CategorizeItem(collision);
        }
    }

    private void CategorizeItem(Collider2D collision)
    {
        SpriteRenderer itemImageRenderer = collision.gameObject.GetComponent<SpriteRenderer>();
        if (itemImageRenderer != null)
        {
            Sprite itemImage = itemImageRenderer.sprite;

            if (TryCategorizeItem(itemImage, category.products, out Product product))
            {
                InventoryManager.Instance.AddItem(product);
            }
            else if (TryCategorizeItem(itemImage, category.plants, out Plant plant))
            {
                InventoryManager.Instance.AddItem(plant);
            }
            else if (TryCategorizeItem(itemImage, category.tools, out Tool tool))
            {
                InventoryManager.Instance.AddItem(tool);
            }
            else if (TryCategorizeItem(itemImage, category.others, out CraftingItem craftingItem))
            {
                InventoryManager.Instance.AddItem(craftingItem);
            }

            Destroy(collision.gameObject);
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
