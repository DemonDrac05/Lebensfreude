using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using static PlantingManager;

public class ToolUsedManager : MonoBehaviour
{
    [HideInInspector] public PlayerController control;
    [HideInInspector] public PlantingManager plantingManager;
    [HideInInspector] public InventoryItem inventoryItem;

    [HideInInspector] public List<Vector3> cultivatedPlots = new();
    [HideInInspector] public List<Vector3> wateredPlots = new();
    [HideInInspector] public List<Vector3> sowedPlots = new();
    [HideInInspector] public List<Plant> seedCategories = new();

    [HideInInspector] public float usingTime;
    [HideInInspector] public float usingDuration = 0.5f;
    [HideInInspector] public int numOfSowedPlants = 0;

    [Header("== Layer Mask Properties ==========")]
    [SerializeField] public Tilemap tilemap;
    [SerializeField] protected LayerMask unsoilableGround;
    [SerializeField] protected LayerMask soilableGround;
    [SerializeField] protected LayerMask waterTerrain;

    [Header("== WaterPot Tool Settings ==========")]
    [HideInInspector] public Slider waterSlider;
    [HideInInspector] public float maxAmountOfWater = 100;
    [HideInInspector] public float currentAmountOfWater;

    [Header("== Hoe Tool Settings ==========")]
    [SerializeField] protected Color soiledGroundColor;
    [SerializeField] protected Color wateredGroundColor;

    [Header("== Action With Tools ==========")]
    private Axe axe;
    private Hoe hoe;
    private WateringPot wateringPot;

    public static ToolUsedManager Instance { get; set; }
    public bool isReadyToUse = true;

    private void Awake()
    {
        control = FindObjectOfType<PlayerController>();
        inventoryItem = FindObjectOfType<InventoryItem>();
        plantingManager = FindObjectOfType<PlantingManager>();

        Instance = this;
    }

    private void OnEnable()
    {
        axe = new(control, plantingManager, sowedPlots);
        hoe = new(this, control, tilemap, cultivatedPlots, soilableGround, soiledGroundColor);
        wateringPot = new(this, control, tilemap, wateredPlots, maxAmountOfWater, waterTerrain, wateredGroundColor);

        TimeManager.OnNewDay += OnNewDay;
    }

    private void OnDisable()
    {
        TimeManager.OnNewDay -= OnNewDay;
    }

    private void Start()
    {
        currentAmountOfWater = maxAmountOfWater;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && isReadyToUse)
        {
            var toolItem = InventoryManager.Instance.GetSelectedItem<Tool>(false);
            if (toolItem != null && usingTime == usingDuration)
            {
                HandleToolAction(toolItem);

                int currenIndex = InventoryManager.Instance.selectedSlot;
                var currentSlot = InventoryManager.Instance.ToolbarSlots[currenIndex];
                var itemInSlot = currentSlot.GetComponentInChildren<InventoryItem>();
                if (itemInSlot != null && toolItem.actionType == ActionType.Water)
                {
                    waterSlider = itemInSlot.slider;
                    waterSlider.value = currentAmountOfWater;
                }
            }

            var seedItem = InventoryManager.Instance.GetSelectedItem<Plant>(false);
            if (seedItem != null)
            {
                HandleSeedAction(seedItem);
            }
        }
    }

    private void FixedUpdate() 
    { 
        isReadyToUse = InventoryManager.Instance.toolbar.activeSelf;
    }

    private List<Vector3Int> plotsToRemove = new List<Vector3Int>();
    private bool tileReset = true;
    private void ResetTiles()
    {
        foreach (var plot in cultivatedPlots)
        {
            Vector3Int mouseGridPos = tilemap.WorldToCell(plot);
            if (wateredPlots.Contains(plot))
            {
                tilemap.SetColor(mouseGridPos, soiledGroundColor);
            }
            else if (!sowedPlots.Contains(plot))
            {
                if (Random.Range(0f, 1f) <= 0.5f)
                {
                    tilemap.SetColor(mouseGridPos, Color.white);
                    plotsToRemove.Add(mouseGridPos);
                }
            }
        }
        foreach (var plot in plotsToRemove)
        {
            cultivatedPlots.Remove(plot);
        }
        wateredPlots.Clear();
        plotsToRemove.Clear();
    }

    private void OnNewDay()
    {
        ResetTiles();
    }

    private void HandleToolAction(Tool tool)
    {
        Vector3Int mouseGridPos = tilemap.WorldToCell(control.mousePosUpdate);

        switch (tool.actionType)
        {
            case ActionType.Chop:
                axe.Execute(tool, control.mousePosUpdate);
                break;
            case ActionType.Cultivate:
                hoe.Execute(mouseGridPos);
                break;
            case ActionType.Water:
                wateringPot.Execute(mouseGridPos);
                break;
        }
    }

    private void HandleSeedAction(Plant seed)
    {
        if (!sowedPlots.Contains(control.mousePosUpdate))
        {
            if(CanSowSeed(seed))
            {
                seed.plantPosition = control.mousePosUpdate;
                seedCategories.Add(seed);
                InventoryManager.Instance.GetSelectedItem<Plant>(true);

                var plant = new PlantingManager.PlantInstance(seedCategories[numOfSowedPlants]);
                plantingManager.sowedPlants.Add(plant);

                GameObject newPlant = Instantiate(seedCategories[numOfSowedPlants].stages[0], seedCategories[numOfSowedPlants].plantPosition, Quaternion.identity);
                plant.CurrentGameObject = newPlant;
                plant.Position = control.mousePosUpdate;

                numOfSowedPlants++;
                sowedPlots.Add(control.mousePosUpdate);
            }
        }
    }

    private bool CanSowSeed(Plant seed)
    {
        return IsSeedEligibleForSowing(seed) && IsInSeason(seed);
    }

    private bool IsSeedEligibleForSowing(Plant seed)
    {
        var selectedItem = InventoryManager.Instance.GetSelectedItem<Plant>(false);
        return (seed == selectedItem && IsReadyToWater() && selectedItem is Seed)
            || (seed == selectedItem && !IsReadyToWater() && (selectedItem is Sapling || selectedItem is IndustrialTree));
    }

    private bool IsInSeason(Plant seed)
    {
        return seed.season.Contains(TimeManager.currentSeason);
    }

    public bool IsReadyToCultivate(Vector3 hitBoxPosition, LayerMask terrain)
    {
        if (!sowedPlots.Contains(hitBoxPosition))
        {
            Vector2 boxSize = new Vector2(0.1f, 0.1f);
            RaycastHit2D hit = Physics2D.BoxCast(hitBoxPosition, boxSize, 0f, Vector2.zero, 0f, terrain);
            return hit.collider != null;
        }
        return false;
    }

    public bool IsReadyToWater() => cultivatedPlots.Contains(control.mousePosUpdate);

    public bool IsWatered(Vector3 wateredPosition) => wateredPlots.Contains(wateredPosition);
}
