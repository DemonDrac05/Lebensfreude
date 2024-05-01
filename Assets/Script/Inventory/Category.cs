using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Category : MonoBehaviour
{
    [field: HideInInspector]
    public InventoryManager inventoryManager;
    private void Start()
    {
        this.inventoryManager = InventoryManager.Instance;
    }

    [Header("Plant Seed")]
    [SerializeField]
    public Item[] plantSeed;
    public void PlantSeedCollected(Sprite seedImage)
    {
        foreach (var item in plantSeed)
        {
            if (seedImage == item.image)
            {
                inventoryManager.AddItem(item);
                return;
            }
        }
    }

    [Header("Plant Product")]
    [SerializeField]
    public Item[] plantProduct;
    public void PlantProductCollected(Sprite productImage)
    {
        foreach (var item in plantSeed)
        {
            if (productImage == item.image)
            {
                inventoryManager.AddItem(item);
                return;
            }
        }
    }

    [Header("Materials")]
    [SerializeField]
    public Item[] material;
    public void MaterialCollected(Sprite materialImage)
    {
        foreach (var item in plantSeed)
        {
            if (materialImage == item.image)
            {
                inventoryManager.AddItem(item);
                return;
            }
        }
    }




}
