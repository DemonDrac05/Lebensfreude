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
        var itemImageRenderer = collision.gameObject.GetComponent<SpriteRenderer>();
        if (itemImageRenderer == null) return;
        var itemImage = itemImageRenderer.sprite;

        if (TryCategorizeItem(itemImage, category.products, out var product))
        {
            InventoryManager.Instance.AddItem(product);
        }
        else if (TryCategorizeItem(itemImage, category.plants, out var plant))
        {
            InventoryManager.Instance.AddItem(plant);
        }
        else if (TryCategorizeItem(itemImage, category.tools, out var tool))
        {
            InventoryManager.Instance.AddItem(tool);
        }

        Destroy(collision.gameObject);
    }

    private static bool TryCategorizeItem<T>(Sprite itemImage, T[] items, out T foundItem) where T : BaseItem
    {
        foreach (var item in items)
        {
            if (item.image != itemImage) continue;
            foundItem = item;
            return true;
        }
        foundItem = null;
        return false;
    }
}
