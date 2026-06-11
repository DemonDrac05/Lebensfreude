using UnityEngine;

// Picks up dropped "CollectibleItem" objects when the player walks over them.
// Looks the item up from the global ItemDatabase (covers EVERY item type, including
// processed materials such as Plank); falls back to the legacy ItemCategory arrays.
public class PlayerCollision : MonoBehaviour
{
    [SerializeField] private ItemDatabase itemDatabase; // drag the project's ItemDatabase asset here
    private ItemCategory category;

    private void Awake()
    {
        category = FindObjectOfType<ItemCategory>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("CollectibleItem"))
        {
            CollectItem(collision);
        }
    }

    private void CollectItem(Collider2D collision)
    {
        var sr = collision.gameObject.GetComponent<SpriteRenderer>();
        if (sr == null) return;
        var sprite = sr.sprite;

        BaseItem found = null;

        // 1) Primary: global database -> ANY item can be collected (fixes "cannot pick up planks").
        if (itemDatabase != null) found = itemDatabase.FindBySprite(sprite);

        // 2) Fallback: legacy category arrays.
        if (found == null && category != null)
        {
            if (TryCategorizeItem(sprite, category.products, out var product)) found = product;
            else if (TryCategorizeItem(sprite, category.plants, out var plant)) found = plant;
            else if (TryCategorizeItem(sprite, category.tools, out var tool)) found = tool;
        }

        if (found != null && InventoryManager.Instance != null)
        {
            InventoryManager.Instance.AddItem(found);
            Destroy(collision.gameObject); // remove only AFTER it is actually collected
        }
        // else: leave it on the ground so a pickup is never silently lost.
    }

    private static bool TryCategorizeItem<T>(Sprite itemImage, T[] items, out T foundItem) where T : BaseItem
    {
        if (items != null)
        {
            foreach (var item in items)
            {
                if (item == null || item.image != itemImage) continue;
                foundItem = item;
                return true;
            }
        }
        foundItem = null;
        return false;
    }
}
