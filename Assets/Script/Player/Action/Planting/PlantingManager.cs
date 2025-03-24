using System.Collections.Generic;
using UnityEngine;
using static Plant;

public class PlantingManager : MonoBehaviour
{
    [SerializeField] public GameObject plantDying;

    [HideInInspector] public PlayerController control;
    [HideInInspector] public ToolUsedManager toolUsed;

    public List<Plant> plantTypes = new();
    public List<Vector3> sowedPlots = new();
    public List<PlantInstance> sowedPlants = new();

    private float harvestTime;
    private const float harvestDuration = 1f;

    private void Awake()
    {
        control = FindObjectOfType<PlayerController>();
        toolUsed = FindObjectOfType<ToolUsedManager>();
    }

    private void OnEnable()  => TimeManager.OnNewDay += HandleNewDay;
    private void OnDisable() => TimeManager.OnNewDay -= HandleNewDay;
    private void HandleNewDay()
    {
        foreach (var plant in sowedPlants)
        {
            GrowingPlant(plant);
        }
    }

    private void FixedUpdate()
    {
        UpdateSowedPlotsAndPlants();

        foreach (var plant in sowedPlants)
        {
            if (plant.ShouldAdvanceStage() && !plant.IsReadyForHarvest())
            {
                AdvanceStage(plant);
            }
        }

        harvestTime = Mathf.Max(0f, harvestTime - Time.deltaTime);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(1) && CanHarvestAt(control.mousePosUpdate))
        {
            Harvest(control.mousePosUpdate);
        }
    }

    #region == PLANT STATUS ==========
    private void GrowingPlant(PlantInstance plant)
    {
        if (AbleToGrow(plant))
        {
            plant.UpdateGrowth();
        }
        else if (!PlantSeasonStatus(plant))
        {
            PlantWithered(plant);
        }
    }

    private bool AbleToGrow(PlantInstance plant)
    {
        return !plant.IsWithered && PlantHydratedStatus(plant) && PlantSeasonStatus(plant);
    }

    private bool PlantHydratedStatus(PlantInstance plant)
    {
        return (plant.Type is Seed && toolUsed.IsWatered(plant.Position))
            || plant.Type is Sapling;
    }

    private bool PlantSeasonStatus(PlantInstance plant)
    {
        foreach (var plantSeason in plant.Type.season)
        {
            return plantSeason == TimeManager.currentSeason;
        }
        return false;
    }
    #endregion

    private void PlantWithered(PlantInstance plant)
    {
        Destroy(plant.CurrentGameObject);
        plant.CurrentGameObject = Instantiate(plantDying, plant.Position, Quaternion.identity);
        plant.IsWithered = true;
    }

    private bool CanHarvestAt(Vector3 position) => sowedPlots.Contains(position);

    private void Harvest(Vector3 position)
    {
        int plantIndex = sowedPlots.IndexOf(position);
        var plant = sowedPlants[plantIndex];
        var cropProduct = plant.GetProduct(ProductType.Crop);

        if (cropProduct != null && sowedPlants[plantIndex].IsReadyForHarvest())
        {
            for (int i = 0; i < cropProduct.quantity; i++)
            {
                InventoryManager.Instance.AddItem(cropProduct.product);
            }
            if (cropProduct.harvestType == HarvestType.OnlyOneProduction)
            {
                RemovePlant(plantIndex);
            }
            else if (cropProduct.harvestType == HarvestType.MultipleProduction)
            {
                RegrowPlant(plant);
            }
        }
    }

    public void RegrowPlant(PlantInstance plant)
    {
        int regrowStage = plant.GetRegrowStage();
        plant.CurrentStageIndex = regrowStage;

        if (regrowStage == 0)
        {
            plant.ResetGrowth();
            UpdatePlantStage(plant, regrowStage);
        }
        else
        {
            plant.GrowthTime = plant.Type.GetStageDuration(regrowStage);
        }
    }

    private void UpdatePlantStage(PlantInstance plant, int stageIndex)
    {
        Destroy(plant.CurrentGameObject);
        GameObject nextStagePrefab = plant.Type.GetStagePrefab(stageIndex);
        plant.CurrentGameObject = Instantiate(nextStagePrefab, plant.Position, Quaternion.identity);
    }

    public void RemovePlant(int index)
    {
        toolUsed.sowedPlots.RemoveAt(index);
        Destroy(sowedPlants[index].CurrentGameObject);
        sowedPlants.RemoveAt(index);
    }

    private void AdvanceStage(PlantInstance plant)
    {
        int nextStageIndex = plant.Type.GetNextStageIndex(plant.CurrentStageIndex);

        if (nextStageIndex != -1)
        {
            UpdatePlantStage(plant, nextStageIndex);
            plant.CurrentStageIndex = nextStageIndex;
            plant.GrowthTime = 0;
        }
    }

    private void UpdateSowedPlotsAndPlants()
    {
        sowedPlots = toolUsed.sowedPlots;
        plantTypes = toolUsed.seedCategories;
    }


    public class PlantInstance
    {
        public GameObject CurrentGameObject { get; set; }
        public Vector3 Position { get; set; }
        public Plant Type { get; set; }

        public int CurrentStageIndex { get; set; }
        public int GrowthTime { get; set; }
        public float Durability { get; set; }
        public bool AbleToGrow { get; set; }
        public bool IsWithered { get; set; }

        public PlantInstance(Plant type)
        {
            Type = type;
            GrowthTime = 0;
            Durability = type.durability;

            IsWithered = false;
        }

        public void UpdateGrowth()
        {
            //if (reachCondition)
            //{
            //    GrowthTime++;
            //    AbleToGrow = false;
            //}
            GrowthTime++;
        }

        public bool ShouldAdvanceStage() => GrowthTime >= Type.GetStageDuration(CurrentStageIndex);

        public bool IsReadyForHarvest() => CurrentStageIndex == Type.stages.Count - 1;

        public int GetRegrowStage() => CurrentStageIndex <= 1 ? 0 : CurrentStageIndex - 2;

        public void ResetGrowth() => GrowthTime = 0;

        public ProductModifier GetProduct(ProductType productType) =>
            Type.productList.Find(p => p.product.productType == productType);
    }
}

