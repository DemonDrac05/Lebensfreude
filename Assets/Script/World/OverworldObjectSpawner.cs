using System.Collections.Generic;
using UnityEngine;

public class OverworldObjectSpawner : MonoBehaviour
{
    [Header("=== Core References ===")]
    public WorldGenerator worldGenerator;
    public PlayerController playerController;

    [Header("=== Special Dungeon Entrance ===")]
    public GameObject dungeonEntrancePrefab;

    [Header("=== Decorative / Interactive Prefabs ===")]
    public List<GameObject> treePrefabs = new();
    public List<GameObject> bushPrefabs = new();
    public List<GameObject> mushroomPrefabs = new();
    public List<GameObject> wildFlowersAndRocks = new(); // Điểm hoa nhặt dại hoặc đá nhỏ trang trí

    [Header("=== Spawn Densities (0..1) ===")]
    [Range(0f, 1f)] public float treeDensity = 0.12f;
    [Range(0f, 1f)] public float bushDensity = 0.08f;
    [Range(0f, 1f)] public float mushroomDensity = 0.04f;
    [Range(0f, 1f)] public float cosmeticDensity = 0.06f;

    private GameObject _entranceInstance;
    private readonly List<GameObject> _spawnedInstances = new();

    private void Start()
    {
        // Chờ WorldGenerator sinh xong Overworld để lấy dữ liệu ô
        if (worldGenerator != null && worldGenerator.generateOnStart)
        {
            SpawnAllOverworldEntities();
        }
    }

    [ContextMenu("Spawn Objects Now")]
    public void SpawnAllOverworldEntities()
    {
        ClearAllSpawnedEntities();

        if (worldGenerator == null) return;
        List<Vector3Int> cells = new(worldGenerator.SpawnableCells);
        if (cells.Count == 0) return;

        // Tránh đặt vật lên vị trí Player xuất hiện
        Vector3 playerSpawnWorld = worldGenerator.PlayerSpawnWorld;
        Vector3Int playerCell = WorldBlocking.WorldToCell(playerSpawnWorld);
        cells.Remove(playerCell);

        // 1. Tạo lối vào Dungeon (Cổng Hầm)
        int gateIndex = Random.Range(0, cells.Count);
        Vector3Int gateCell = cells[gateIndex];
        cells.RemoveAt(gateIndex);

        Vector3 gateWorldPos = worldGenerator.groundTilemap.GetCellCenterWorld(gateCell);
        if (dungeonEntrancePrefab != null)
        {
            _entranceInstance = Instantiate(dungeonEntrancePrefab, gateWorldPos, Quaternion.identity);
            if (playerController != null)
            {
                playerController.itemsOnGround.Add(gateWorldPos);
            }
        }

        // 2. Rải các vật thể thiên nhiên ngẫu nhiên theo hạt giống bản đồ
        System.Random rng = new System.Random(worldGenerator.seed);
        foreach (Vector3Int cell in cells)
        {
            if (cell == playerCell) continue;

            Vector3 worldPos = worldGenerator.groundTilemap.GetCellCenterWorld(cell);
            double roll = rng.NextDouble();

            GameObject selectedPrefab = null;
            bool blocksMovement = false;

            if (roll < treeDensity)
            {
                if (treePrefabs.Count > 0)
                {
                    selectedPrefab = treePrefabs[rng.Next(treePrefabs.Count)];
                    blocksMovement = true;
                }
            }
            else if (roll < treeDensity + bushDensity)
            {
                if (bushPrefabs.Count > 0)
                {
                    selectedPrefab = bushPrefabs[rng.Next(bushPrefabs.Count)];
                    blocksMovement = true;
                }
            }
            else if (roll < treeDensity + bushDensity + mushroomDensity)
            {
                if (mushroomPrefabs.Count > 0)
                {
                    selectedPrefab = mushroomPrefabs[rng.Next(mushroomPrefabs.Count)];
                }
            }
            else if (roll < treeDensity + bushDensity + mushroomDensity + cosmeticDensity)
            {
                if (wildFlowersAndRocks.Count > 0)
                {
                    selectedPrefab = wildFlowersAndRocks[rng.Next(wildFlowersAndRocks.Count)];
                }
            }

            if (selectedPrefab != null)
            {
                GameObject instance = Instantiate(selectedPrefab, worldPos, Quaternion.identity, transform);
                _spawnedInstances.Add(instance);

                if (blocksMovement)
                {
                    WorldBlocking.Block(cell);
                    if (playerController != null)
                    {
                        playerController.itemsOnGround.Add(worldPos);
                    }
                }
            }
        }
    }

    public void ClearAllSpawnedEntities()
    {
        if (_entranceInstance != null)
        {
            Destroy(_entranceInstance);
            _entranceInstance = null;
        }

        foreach (GameObject inst in _spawnedInstances)
        {
            if (inst != null) Destroy(inst);
        }
        _spawnedInstances.Clear();
    }
}