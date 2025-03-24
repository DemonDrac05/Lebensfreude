using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemOutOfInventory : MonoBehaviour, IPointerClickHandler
{
    protected Player player;

    private void Awake()
    {
        player = FindObjectOfType<Player>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (InventoryItem.CurrentMovingItem != null)
        {
            InventoryItem inventoryItem = InventoryItem.CurrentMovingItem;
            ScriptableObject checkItem = inventoryItem.item;

            GameObject newItem = checkItem switch
            {
                Product product => product.gameObj,
                Plant plant => plant.gameObj,
                Tool tool => tool.gameObj,
                _ => null
            };

            int numOfItem = inventoryItem.count;
            for (int i = 0; i < numOfItem && newItem != null; i++)
            {
                Vector2 initialPosition = player.transform.position;
                Vector2 targetPosition = ItemDropDistance();

                GameObject dropItem = Instantiate(newItem, initialPosition, Quaternion.identity);
                StartCoroutine(SlidingEffect(dropItem.transform, targetPosition, 1f));

                inventoryItem.count--;
                inventoryItem.RefreshCount();

                if (inventoryItem.count == 0)
                {
                    InventoryItem.CurrentMovingItem = null;
                    Destroy(inventoryItem.gameObject);
                    break;
                }
            }
        }
    }

    Vector2 ItemDropDistance()
    {
        float distanceDropX = 0f;
        float distanceDropY = 0f;
        float randomRange = Random.Range(-0.5f, 0.5f);

        switch (player.FacingDirection)
        {
            case Player.FacialDirection.Right:
                distanceDropX = 2f; break;
            case Player.FacialDirection.Left:
                distanceDropX = -2f; break;
            case Player.FacialDirection.Up:
                distanceDropY = 2f; break;
            case Player.FacialDirection.Down:
                distanceDropY = -2f; break;
        }

        distanceDropX = distanceDropX == 0 ? randomRange : distanceDropX;
        distanceDropY = distanceDropY == 0 ? randomRange : distanceDropY;

        Vector2 playerPosition = player.transform.position;
        return new Vector2(playerPosition.x + distanceDropX, playerPosition.y + distanceDropY);
    }

    IEnumerator SlidingEffect(Transform itemTransform, Vector2 targetPosition, float duration)
    {
        Vector2 startPosition = itemTransform.position;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            itemTransform.position = Vector2.Lerp(startPosition, targetPosition, elapsedTime / duration);
            itemTransform.GetComponent<CircleCollider2D>().enabled = elapsedTime >= duration;

            yield return null;
        }
    }
}