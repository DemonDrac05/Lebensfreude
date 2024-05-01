using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class WaterCan : MonoBehaviour
{
    [HideInInspector] public PlayerController control;
    [HideInInspector] public Item item;
    [HideInInspector] public PlantingManager plantingManager;
    [HideInInspector] public InventoryItem inventoryItem;

    [HideInInspector] public float usingTime;
    [HideInInspector] public float usingDuration = 0.5f;

    [Header("WaterCanTool")]
    [SerializeField] public Slider waterSlider;
    [SerializeField] public float maxAmountOfWater = 100;
    [HideInInspector] private float currentAmoundOfWater;

    private void Awake()
    {
        control = FindObjectOfType<PlayerController>();
        item = FindObjectOfType<Item>();
        inventoryItem = FindObjectOfType<InventoryItem>();
        plantingManager = FindObjectOfType<PlantingManager>();
    }
    private void Start()
    {
        currentAmoundOfWater = maxAmountOfWater;
    }
    private void FixedUpdate()
    {
        item = InventoryManager.Instance.GetSelectedItem(false);
    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && usingTime == usingDuration && item != null)
        {
            if (item.actionType == ActionType.Water)
            {
                currentAmoundOfWater -= 20f;
                waterSlider.value = currentAmoundOfWater;
            }
        }
    }
}
