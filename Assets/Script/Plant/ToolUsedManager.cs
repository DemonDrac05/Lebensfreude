using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class ToolUsedManager : MonoBehaviour
{
    [HideInInspector] public PlayerController control;
    [HideInInspector] public Item item;
    [HideInInspector] public PlantingManager plantingManager;
    [HideInInspector] public InventoryItem inventoryItem;

    [HideInInspector] public List<Vector3> cultivatedPlot = new List<Vector3>();
    [HideInInspector] public List<Vector3> watteredPlot = new List<Vector3>();
    [HideInInspector] public List<Vector3> sowedPlot = new List<Vector3>();

    [HideInInspector] public List<Item> seedCategory = new List<Item>();

    [HideInInspector] public float usingTime;
    [HideInInspector] public float usingDuration = 0.5f;
    [HideInInspector] public int numOfSowedPlant = 0;

    [SerializeField] public Tilemap tilemap;
    [SerializeField] private LayerMask unsoiledGround;
    [SerializeField] private Color soiledGroundColor;
    [SerializeField] private Color waterredGroundColor;

    [Header("WaterCanTool")]
    [SerializeField] private LayerMask waterTerrain;

    [HideInInspector] public Slider waterSlider;
    [HideInInspector] public float maxAmountOfWater = 100;
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
            Vector3Int mouseGridPos = tilemap.WorldToCell(control.mousePosUpdate);

            #region ###TOOL: HOE
            if (item.actionType == ActionType.Cultivate)
            {
                if (IsReadyToCultivated(control.mousePosUpdate,unsoiledGround) 
                && !Waterred(control.mousePosUpdate))
                {
                    tilemap.SetTileFlags(mouseGridPos, TileFlags.None);
                    tilemap.SetColor(mouseGridPos, soiledGroundColor);

                    if (!cultivatedPlot.Contains(control.mousePosUpdate))
                        cultivatedPlot.Add(control.mousePosUpdate);
                }        
            }
            #endregion
            #region ###TOOL: WATER CAN
            if (item.actionType == ActionType.Water)
            {
                if (currentAmoundOfWater > 0f && IsReadyToWaterred())
                {
                    tilemap.SetTileFlags(mouseGridPos, TileFlags.None);
                    tilemap.SetColor(mouseGridPos, waterredGroundColor);

                    if (!watteredPlot.Contains(control.mousePosUpdate))
                        watteredPlot.Add(control.mousePosUpdate);
                }

                if (IsReadyToCultivated(control.mousePosUpdate, waterTerrain))
                    currentAmoundOfWater = maxAmountOfWater;
                else
                    currentAmoundOfWater -= 20f;
                waterSlider.value = currentAmoundOfWater;
            }
            #endregion
            #region ###TOOL: SEED
            if (item.actionType == ActionType.Sow)
            {
                if (!sowedPlot.Contains(control.mousePosUpdate) && IsReadyToWaterred())
                {
                    item.plantPosition = control.mousePosUpdate;
                    seedCategory.Add(item);
                    item = InventoryManager.Instance.GetSelectedItem(true);

                    PlantingManager.Plant plant = new PlantingManager.Plant(seedCategory[numOfSowedPlant]);
                    plantingManager.sowedPlant.Add(plant);

                    GameObject newPlant = Instantiate(seedCategory[numOfSowedPlant].stages[0], seedCategory[numOfSowedPlant].plantPosition, Quaternion.identity);
                    plant.currentPlantObj = newPlant;
                    plant.plantPosition = control.mousePosUpdate;

                    numOfSowedPlant++;

                    sowedPlot.Add(control.mousePosUpdate);
                }      
            }
            #endregion
        }
    }
    public bool IsReadyToCultivated(Vector3 hitBoxPosition,LayerMask terrain)
    {
        Vector2 boxSize = new Vector2(0.1f, 0.1f);
        float angle = 0f;
        Vector2 direction = Vector2.zero;
        RaycastHit2D hit = Physics2D.BoxCast(hitBoxPosition, boxSize, angle, direction, 0f, terrain);
        return hit.collider != null;
    }
    public bool IsReadyToWaterred()
    {
        return cultivatedPlot.Contains(control.mousePosUpdate);
    }

    public bool Waterred(Vector3 waterredPosition)
    {
        return watteredPlot.Contains(waterredPosition);
    }
}
