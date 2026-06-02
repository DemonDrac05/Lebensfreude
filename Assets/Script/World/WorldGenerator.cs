using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// ╔══════════════════════════════════════════════════════════════════╗
// ║  WorldGenerator — SINH THẾ GIỚI OVERWORLD                         ║
// ╠══════════════════════════════════════════════════════════════════╣
// ║  Biome bằng 2 lớp Perlin noise (độ cao + độ ẩm):                  ║
// ║    water < cát < (đất / cỏ / hoa / rừng) < đá < núi               ║
// ║  - Cỏ: random từ 1 LIST tile cho đa dạng.                         ║
// ║  - Hoa: xác suất nhỏ rải trên cỏ.                                 ║
// ║  - Rừng: vùng ẩm cao (tile cỏ đậm hơn).                           ║
// ║  - Viền bản đồ = đại dương (water) để nhốt player.                ║
// ║  CHẶN player: water + đá + núi -> ghi vào WorldBlocking.          ║
// ║  Cây/bụi/nấm KHÔNG sinh ở đây — để OverworldObjectSpawner lo,     ║
// ║  dùng danh sách ô-có-thể-sinh (cỏ/rừng) mà file này phơi ra.      ║
// ╚══════════════════════════════════════════════════════════════════╝
public class WorldGenerator : MonoBehaviour
{
    [Header("=== Tilemap & Grid ===")]
    public Grid grid;                  // Grid cha (để WorldBlocking quy đổi cell)
    public Tilemap groundTilemap;      // tilemap nền (vẽ tile + đánh dấu chặn)

    [Header("=== Kích thước bản đồ ===")]
    public int width = 80;
    public int height = 60;
    [Tooltip("Số ô viền đại dương bao quanh để nhốt player.")]
    public int oceanBorder = 3;

    [Header("=== Noise ===")]
    public int seed = 0;
    [Tooltip("Càng nhỏ biome càng to/mượt.")]
    public float elevationScale = 0.08f;
    public float moistureScale = 0.12f;

    [Header("=== Ngưỡng độ cao (0..1) ===")]
    [Range(0, 1)] public float waterLevel = 0.30f;   // < -> water
    [Range(0, 1)] public float sandLevel = 0.36f;    // < -> cát (bãi biển)
    [Range(0, 1)] public float landLevel = 0.70f;    // < -> đất liền (cỏ/rừng/hoa/đất)
    [Range(0, 1)] public float stoneLevel = 0.82f;   // < -> đá ; >= -> núi

    [Header("=== Ngưỡng độ ẩm cho đất liền (0..1) ===")]
    [Range(0, 1)] public float forestMoisture = 0.62f; // >= -> rừng
    [Range(0, 1)] public float dirtMoisture = 0.30f;   // <  -> đất ; giữa -> cỏ
    [Range(0, 1)] public float flowerChance = 0.06f;   // xác suất hoa trên cỏ

    [Header("=== Tiles ===")]
    public List<TileBase> grassTiles = new List<TileBase>(); // cỏ (random đa dạng)
    public List<TileBase> flowerTiles = new List<TileBase>(); // hoa trên cỏ
    public TileBase forestTile;       // cỏ rừng (đậm hơn)
    public TileBase sandTile;
    public TileBase dirtTile;
    public TileBase waterTile;
    public TileBase stoneTile;
    public TileBase mountainTile;

    [Header("=== Sinh lúc Start? ===")]
    public bool generateOnStart = true;

    // Các ô có thể đặt object (cỏ/rừng) — OverworldObjectSpawner đọc. ──────
    private readonly List<Vector3Int> _spawnableCells = new List<Vector3Int>();
    public IReadOnlyList<Vector3Int> SpawnableCells => _spawnableCells;

    // Ô spawn cho player (giữa bản đồ, chắc chắn đi được). ─────────────────
    public Vector3 PlayerSpawnWorld { get; private set; }

    private System.Random _rng;

    private void Start()
    {
        if (generateOnStart) Generate();
    }

    // ════════════════ SINH THẾ GIỚI ════════════════
    [ContextMenu("Generate World")]
    public void Generate()
    {
        if (grid == null && groundTilemap != null) grid ??= groundTilemap.GetComponentInParent<Grid>();
        if (groundTilemap == null) { Debug.LogError("[WorldGenerator] Thiếu groundTilemap."); return; }
        if (grassTiles == null || grassTiles.Count == 0) { Debug.LogError("[WorldGenerator] grassTiles trống."); return; }

        // Reset
        groundTilemap.ClearAllTiles();
        WorldBlocking.Clear();
        if (grid != null) WorldBlocking.SetGrid(grid);
        _spawnableCells.Clear();

        int s = seed != 0 ? seed : Random.Range(1, 999999);
        _rng = new System.Random(s);
        // offset ngẫu nhiên để mỗi seed cho map khác nhau
        float ox = (float)_rng.NextDouble() * 1000f;
        float oy = (float)_rng.NextDouble() * 1000f;
        float ox2 = (float)_rng.NextDouble() * 1000f;
        float oy2 = (float)_rng.NextDouble() * 1000f;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3Int cell = new Vector3Int(x, y, 0);

                // Viền đại dương
                bool border = x < oceanBorder || y < oceanBorder ||
                              x >= width - oceanBorder || y >= height - oceanBorder;

                float e = border ? 0f : Mathf.PerlinNoise(x * elevationScale + ox, y * elevationScale + oy);
                float m = Mathf.PerlinNoise(x * moistureScale + ox2, y * moistureScale + oy2);

                TileBase tile;
                bool blocked = false;
                bool spawnable = false;

                if (e < waterLevel)            { tile = waterTile;    blocked = true; }
                else if (e < sandLevel)        { tile = sandTile; }
                else if (e < landLevel)
                {
                    // Đất liền: phân theo độ ẩm
                    if (m >= forestMoisture)   { tile = forestTile != null ? forestTile : PickGrass(); spawnable = true; }
                    else if (m < dirtMoisture) { tile = dirtTile != null ? dirtTile : PickGrass(); }
                    else
                    {
                        // Cỏ — có xác suất ra hoa
                        if (flowerTiles.Count > 0 && _rng.NextDouble() < flowerChance) tile = flowerTiles[_rng.Next(flowerTiles.Count)];
                        else tile = PickGrass();
                        spawnable = true;
                    }
                }
                else if (e < stoneLevel)       { tile = stoneTile != null ? stoneTile : PickGrass(); blocked = true; }
                else                           { tile = mountainTile != null ? mountainTile : stoneTile; blocked = true; }

                groundTilemap.SetTile(cell, tile);
                if (blocked) WorldBlocking.Block(cell);
                else if (spawnable) _spawnableCells.Add(cell);
            }
        }

        ComputePlayerSpawn();
        Debug.Log($"[WorldGenerator] Sinh xong {width}x{height} (seed {s}) — chặn {WorldBlocking.BlockedCount} ô, {_spawnableCells.Count} ô sinh object.");
    }

    private TileBase PickGrass() => grassTiles[_rng.Next(grassTiles.Count)];

    // Tìm ô đi được gần tâm bản đồ làm điểm spawn player. ──────────────────
    private void ComputePlayerSpawn()
    {
        Vector3Int center = new Vector3Int(width / 2, height / 2, 0);
        Vector3Int found = center;
        if (WorldBlocking.IsBlocked(center))
        {
            // xoắn ốc ra ngoài tìm ô không bị chặn
            for (int r = 1; r < Mathf.Max(width, height); r++)
            {
                bool ok = false;
                for (int dx = -r; dx <= r && !ok; dx++)
                    for (int dy = -r; dy <= r && !ok; dy++)
                    {
                        var c = new Vector3Int(center.x + dx, center.y + dy, 0);
                        if (!WorldBlocking.IsBlocked(c)) { found = c; ok = true; }
                    }
                if (ok) break;
            }
        }
        PlayerSpawnWorld = groundTilemap.GetCellCenterWorld(found);
    }
}
