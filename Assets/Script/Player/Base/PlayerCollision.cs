using UnityEngine;
using UnityEngine.UI;

public class PlayerCollision : MonoBehaviour
{
    public SpriteRenderer playerSprite;
    public Category category;

    private void Awake()
    {
        category = FindObjectOfType<Category>();
    }
    private void Start()
    {
        playerSprite = GetComponent<SpriteRenderer>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Seed"))
        {
            SpriteRenderer image = collision.gameObject.GetComponent<SpriteRenderer>();
            category.PlantSeedCollected(image.sprite);
            Destroy(collision.gameObject);
        }
        if (collision.gameObject.CompareTag("Resource"))
        {
            playerSprite.sortingLayerName = "Player";
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Resource"))
        {
            playerSprite.sortingLayerName = "Resource";
        }
    }

}
