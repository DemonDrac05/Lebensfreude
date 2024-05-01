using System.Collections.Generic;
using UnityEngine.Tilemaps;
using UnityEngine;
using System.Collections;

public class PlantingMechanics : MonoBehaviour
{
    public Player player;

    [SerializeField] private Tilemap tilemap;
    [SerializeField] private int numOfSeed = 0;
    [SerializeField] private List<PlantType> plantTypes = new List<PlantType>();

    [SerializeField] private float popUpTime;
    [SerializeField] private float popUpDuration = 1f;
    [SerializeField] private Transform popUpPoint;

    private List<Vector3> usedPositions = new List<Vector3>();
    private List<Plant> cultivatedPlants = new List<Plant>();
    private List<GameObject> popUpAnimation = new List<GameObject>();

    private void Start()
    {
        player = FindObjectOfType<Player>();
    }

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Vector3Int playerGridPosition = tilemap.WorldToCell(player.transform.position);
            TileBase playerTile = tilemap.GetTile(playerGridPosition);

            Vector3 plantTile = new Vector3(playerGridPosition.x + 0.5f, playerGridPosition.y + 0.5f, 0f);

            if (playerTile != null && numOfSeed > 0 && !usedPositions.Contains(plantTile))
            {
                PlantType selectedPlant = plantTypes[0]; // Assuming only one type for now

                Plant plant = new Plant(selectedPlant);
                cultivatedPlants.Add(plant);

                GameObject newPlant = Instantiate(selectedPlant.stages[0], plantTile, Quaternion.identity);
                plant.cultivatedPlant = newPlant;

                usedPositions.Add(plantTile);
                numOfSeed--;
            }

            if (IsPlantTile(plantTile)) HarvestPlant(plantTile);
        }
    }

    bool IsPlantTile(Vector3 plantTile)
    {
        return usedPositions.Contains(plantTile);
    }

    IEnumerator PopUpAnimationCoroutine(int index)
    {
        GameObject popUpObject = Instantiate(cultivatedPlants[index].plantProduct, popUpPoint.position, Quaternion.identity);

        yield return new WaitForSeconds(popUpDuration);

        Destroy(popUpObject);
    }

    void HarvestPlant(Vector3 plantTile)
    {
        int plantIndex = usedPositions.IndexOf(plantTile);

        if (cultivatedPlants[plantIndex].IsReadyToHarvest())
        {
            StartCoroutine(PopUpAnimationCoroutine(plantIndex));

            usedPositions.RemoveAt(plantIndex);
            Destroy(cultivatedPlants[plantIndex].cultivatedPlant);
            cultivatedPlants.RemoveAt(plantIndex);
        }
    }

    private void FixedUpdate()
    {
        for (int i = 0; i < cultivatedPlants.Count; i++)
        {
            cultivatedPlants[i].UpdateTime(Time.deltaTime);

            if (cultivatedPlants[i].IsReadyToNextStage())
            {
                NextStage(i);
            }
        }
    }
    private void NextStage(int index)
    {
        Plant currentPlant = cultivatedPlants[index];
        PlantType plantType = currentPlant.plantType;
        int nextStageIndex = plantType.GetNextStageIndex(currentPlant.currentStageIndex);

        if (nextStageIndex != -1)
        {
            Destroy(currentPlant.cultivatedPlant);

            currentPlant.currentStageIndex = nextStageIndex;
            GameObject nextStagePrefab = plantType.GetStagePrefab(nextStageIndex);
            GameObject newPlant = Instantiate(nextStagePrefab, usedPositions[index], Quaternion.identity);
            cultivatedPlants[index].cultivatedPlant = newPlant;
        }
        else
        {
        }
    }

    [System.Serializable]
    public class PlantType
    {
        public string nameOfPlant;

        public List<GameObject> stages = new List<GameObject>();

        public List<float> stageDurations = new List<float>();

        public GameObject Harvest;

        public GameObject GetStagePrefab(int index)
        {
            if (index >= 0 && index < stages.Count)
                return stages[index];
            return null;
        }

        public float GetStageDuration(int index)
        {
            if (index >= 0 && index < stageDurations.Count)
                return stageDurations[index];
            return 0f;
        }

        public int GetNextStageIndex(int currentIndex)
        {
            if (currentIndex >= 0 && currentIndex < stages.Count - 1)
                return currentIndex + 1;
            return -1;
        }
    }

    private class Plant
    {
        public GameObject cultivatedPlant;

        public GameObject plantProduct;

        public PlantType plantType;

        public int currentStageIndex = 0;

        public float plantedTime;

        public Plant(PlantType plantType)
        {
            this.plantType = plantType;

            this.plantedTime = 0f;

            this.plantProduct = plantType.Harvest;
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
    }
}
