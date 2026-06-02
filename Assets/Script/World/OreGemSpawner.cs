using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class OreGemSpawner : MonoBehaviour
{
    [Header("=== Scene references ==========")]
    [SerializeField] private Tilemap         groundTilemap;
    [SerializeField] private OreGemTable     table;
    [SerializeField] private PlayerController playerController; 

    [Header("=== Spawn config ==========")]
    [SerializeField] private int  floor          = 1;   
    [SerializeField] private int  depositsToSpawn = 12;
    [SerializeField] private int  seed           = 0;
    [SerializeField] private bool spawnOnStart   = true;

    private void Awake()
    {
        if (playerController == null)
            playerController = FindObjectOfType<PlayerController>();
    }

    private void Start() { if (spawnOnStart) Spawn(floor); }

    public void Spawn(int floorLevel)
    {
        if (table == null || groundTilemap == null) return;
        var rng = new System.Random(seed + floorLevel * 1000);

        // Thu thập các ô đất KHÔNG BỊ CHẶN (Không nằm đè lên Wall Tile)
        var cells = new List<Vector3Int>();
        foreach (var pos in groundTilemap.cellBounds.allPositionsWithin)
        {
            if (groundTilemap.HasTile(pos) && !WorldBlocking.IsBlocked(pos)) 
            {
                cells.Add(pos);
            }
        }

        for (int i = 0; i < depositsToSpawn && cells.Count > 0; i++)
        {
            int idx  = rng.Next(cells.Count);
            var cell = cells[idx]; 
            cells.RemoveAt(idx);

            var entry = table.PickForFloor(floorLevel, rng);
            if (entry == null || entry.depositPrefab == null) continue;

            Vector3 center = groundTilemap.GetCellCenterWorld(cell);
            var go  = Instantiate(entry.depositPrefab, new Vector2(center.x, center.y - 0.5f), Quaternion.identity, transform);
            var dep = go.GetComponent<MineableDeposit>();
            if (dep == null) continue;

            dep.Configure(entry.dropItem, rng.Next(entry.dropAmountMin, entry.dropAmountMax + 1));

            dep.RegisterWithTileSystem(center, playerController);
        }
    }
}