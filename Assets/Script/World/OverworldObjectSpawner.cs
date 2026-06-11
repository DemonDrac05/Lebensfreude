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
    public List<GameObject> wildFlowersAndRocks = new(); // Wild pickable flowers or small decorative rocks

    [Header("=== Spawn Densities (0..1) ===")]
    [Range(0f, 1f)] public float treeDensity = 0.12f;
    [Range(0f, 1f)] public float bushDensity = 0.08f;
    [Range(0f, 1f)] public float mushroomDensity = 0.04f;
    [Range(0f, 1f)] public float cosmeticDensity = 0.06f;

    [Header("=== Harvest drops (assign the item each object yields) ===")]
    public BaseItem treeDropItem;      public int treeDrops = 3; public int treeHits = 3; public float treeRespawn = 120f;
    public BaseItem bushDropItem;      public int bushDrops = 1;
    public BaseItem mushroomDropItem;  public int mushroomDrops = 1;
    public float forageRespawn = 60f;

    private GameObject _entranceInstance;
    private readonly List<GameObject> _spawnedInstances = new();

    private void Start()
    {
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

        // Avoid placing objects on the player's spawn cell
        Vector3 playerSpawnWorld = worldGenerator.PlayerSpawnWorld;
        Vector3Int playerCell = WorldBlocking.WorldToCell(playerSpawnWorld);
        cells.Remove(playerCell);

        // 1. Create the dungeon entrance gate
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

        // 2. Scatter natural objects randomly using the map seed
        System.Random rng = new System.Random(worldGenerator.seed);
        foreach (Vector3Int cell in cells)
        {
            if (cell == playerCell) continue;

            Vector3 worldPos = worldGenerator.groundTilemap.GetCellCenterWorld(cell);
            double roll = rng.NextDouble();

            GameObject selectedPrefab = null;
            bool blocksMovement = false;
            int kind = 0; // 1=tree, 2=bush, 3=mushroom, 0=cosmetic/none

            if (roll < treeDensity)
            {
                if (treePrefabs.Count > 0)
                {
                    selectedPrefab = treePrefabs[rng.Next(treePrefabs.Count)];
                    blocksMovement = true;
                    kind = 1;
                }
            }
            else if (roll < treeDensity + bushDensity)
            {
                if (bushPrefabs.Count > 0)
                {
                    selectedPrefab = bushPrefabs[rng.Next(bushPrefabs.Count)];
                    blocksMovement = false; // bushes are forageable and walk-through (must NOT block the player)
                    kind = 2;
                }
            }
            else if (roll < treeDensity + bushDensity + mushroomDensity)
            {
                if (mushroomPrefabs.Count > 0)
                {
                    selectedPrefab = mushroomPrefabs[rng.Next(mushroomPrefabs.Count)];
                    kind = 3;
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
                AttachHarvest(instance, kind);

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

    // Attach a Harvestable so a spawned natural object can actually be harvested by click.
    private void AttachHarvest(GameObject go, int kind)
    {
        if (go == null || kind == 0) return;
        var h = go.GetComponent<Harvestable>();
        if (h == null) h = go.AddComponent<Harvestable>();
        if (kind == 1)      h.Configure(false, ActionType.Chop, treeDropItem,     null, treeDrops,     treeHits, true, treeRespawn);
        else if (kind == 2) h.Configure(true,  ActionType.Chop, bushDropItem,     null, bushDrops,     1,        true, forageRespawn);
        else if (kind == 3) h.Configure(true,  ActionType.Chop, mushroomDropItem, null, mushroomDrops, 1,        true, forageRespawn);
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
