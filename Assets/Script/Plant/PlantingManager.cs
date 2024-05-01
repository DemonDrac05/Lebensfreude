using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Purchasing;

public class PlantingManager : MonoBehaviour
{
    [HideInInspector] public PlayerController control;
    [HideInInspector] public ToolUsedManager toolUsed;
    [HideInInspector] public Item item;

    [HideInInspector] public List<Item> plantType = new List<Item>();
    [HideInInspector] public List<Vector3> sowedPlot = new List<Vector3>();
    [HideInInspector] public List<Plant> sowedPlant = new List<Plant>();

    [HideInInspector] public float harvestTime;
    [HideInInspector] public float harvestDuration = 1f;
    private void Awake()
    {
        item = FindObjectOfType<Item>();
        control = FindObjectOfType<PlayerController>();
        toolUsed = FindObjectOfType<ToolUsedManager>();
    }
    private void FixedUpdate()
    {
        item = InventoryManager.Instance.GetSelectedItem(false);

        sowedPlot = toolUsed.sowedPlot;
        plantType = toolUsed.seedCategory;

        for (int i = 0; i < sowedPlant.Count; i++)
        {
            if (toolUsed.Waterred(sowedPlant[i].plantPosition))
            {
                sowedPlant[i].UpdateTime(Time.deltaTime);
            }

            if (sowedPlant[i].IsReadyToNextStage() && !sowedPlant[i].IsReadyToHarvest())
            {
                NextStage(i);
            }
        }

        if (harvestTime > 0f) harvestTime -= Time.deltaTime;
        if (harvestTime <= 0f) harvestTime = 0f;
    }

    private void Update()
    {
        if(Input.GetMouseButtonDown(1))
        {
            if(HarvestCondition(control.mousePosUpdate))
                Harvest(control.mousePosUpdate);
        }
    }

    public bool HarvestCondition(Vector3 plantTile)
    {
        return sowedPlot.Contains(plantTile);
    }

    public void Harvest(Vector3 plantTile)
    {
        int plantIndex = sowedPlot.IndexOf(plantTile);

        if (plantIndex != -1 && sowedPlant[plantIndex].IsReadyToHarvest())
        {
            if(sowedPlant[plantIndex].plantType.harvestCount == 0)
            {
                sowedPlant[plantIndex].plantType.harvestCount = 1;
            }

            for(int i = 0; i < sowedPlant[plantIndex].plantType.harvestCount; i++)
            {
                InventoryManager.Instance.AddItem(sowedPlant[plantIndex].plantType.productItem);
            }

            if (sowedPlant[plantIndex].plantType.plantType == PlantType.OnlyOneProduction)
            {
                toolUsed.sowedPlot.RemoveAt(plantIndex);
                Destroy(sowedPlant[plantIndex].currentPlantObj);
                sowedPlant.RemoveAt(plantIndex);
            }

            else if (sowedPlant[plantIndex].plantType.plantType == PlantType.MultipleProduction)
            {
                int reStageIndex  = sowedPlant[plantIndex].ReGrowIndex();
                sowedPlant[plantIndex].currentStageIndex = reStageIndex;
                sowedPlant[plantIndex].plantedTime = sowedPlant[plantIndex].plantType.stageDuration[reStageIndex];
            }
        }
    }

    public void NextStage(int index)
    {
        Plant currentPlant = sowedPlant[index];
        Item plantType = currentPlant.plantType;
        int nextStageIndex = plantType.GetNextStageIndex(currentPlant.currentStageIndex);

        if(nextStageIndex != -1)
        {
            Destroy(currentPlant.currentPlantObj);

            currentPlant.currentStageIndex = nextStageIndex;
            GameObject nextStagePrefab = plantType.GetStagePrefab(nextStageIndex);
            GameObject newPlant = Instantiate(nextStagePrefab, sowedPlot[index], Quaternion.identity);
            sowedPlant[index].currentPlantObj = newPlant;
        }
    }

    public class Plant
    {
        public GameObject currentPlantObj;

        public Vector3 plantPosition;

        public Item plantType;

        public int currentStageIndex = 0;

        public float plantedTime;

        public Plant(Item item)
        {
            this.plantType = item;

            this.plantedTime = 0f;
        }

        public void UpdateTime(float deltaTime)
        {
            plantedTime += deltaTime;
        }

        public bool IsReadyToNextStage()
        {
            float currentStageDuration = plantType.GetStageDuration(currentStageIndex);
            return plantedTime >= currentStageDuration;
        }

        public bool IsReadyToHarvest()
        {
            if (currentStageIndex == plantType.stages.Count - 1)
                return true;
            return false;
        }

        public int ReGrowIndex()
        {
            if(currentStageIndex <= 1) return 0;
            else return currentStageIndex - 2;
        }
    }
}
