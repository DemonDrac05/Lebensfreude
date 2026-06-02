using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// ─────────────────────────────────────────
// ORE GEM SPAWNER  (rải deposit quặng/đá quý ở GIỮA Ô theo tầng)
// ─────────────────────────────────────────
// Spawn(floor): lấy ô có tile, chọn random, đặt deposit theo OreGemTable.PickForFloor tại GetCellCenterWorld
// (giữa ô — đúng hitbox center-grid kiểu Stardew). Gán dropItem + số lượng cho từng deposit.
//
// Liên kết: OreGemTable (chọn loại), MineableDeposit (Configure), Tilemap (ô + tâm ô).
public class OreGemSpawner : MonoBehaviour
{
    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private OreGemTable table;
    [SerializeField] private int floor = 1;            // overworld = 1; dungeon đặt theo độ sâu
    [SerializeField] private int depositsToSpawn = 12;
    [SerializeField] private int seed = 0;
    [SerializeField] private bool spawnOnStart = true;

    private void Start() { if (spawnOnStart) Spawn(floor); }

    public void Spawn(int floorLevel)
    {
        if (table == null || groundTilemap == null) return;
        var rng = new System.Random(seed + floorLevel * 1000);

        var cells = new List<Vector3Int>();
        foreach (var pos in groundTilemap.cellBounds.allPositionsWithin)
            if (groundTilemap.HasTile(pos)) cells.Add(pos);

        for (int i = 0; i < depositsToSpawn && cells.Count > 0; i++)
        {
            int idx = rng.Next(cells.Count);
            var cell = cells[idx]; cells.RemoveAt(idx);

            var entry = table.PickForFloor(floorLevel, rng);
            if (entry == null || entry.depositPrefab == null) continue;

            Vector3 center = groundTilemap.GetCellCenterWorld(cell); // GIỮA Ô
            var go = Instantiate(entry.depositPrefab, center, Quaternion.identity, transform);
            var dep = go.GetComponent<MineableDeposit>();
            if (dep != null)
                dep.Configure(entry.dropItem, rng.Next(entry.dropAmountMin, entry.dropAmountMax + 1));
        }
    }
}
