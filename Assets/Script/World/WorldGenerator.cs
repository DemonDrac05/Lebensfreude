using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WorldGenerator : MonoBehaviour
{
    [Header("=== Tilemap & Grid ===")]
    public Grid grid;                  
    public Tilemap groundTilemap;      

    [Header("=== Kích thước bản đồ ===")]
    public int width = 120; 
    public int height = 120;
    [Tooltip("Số ô viền đại dương bao quanh để nhốt player.")]
    public int oceanBorder = 4;

    [Header("=== Định vị 3 Làng chính ===")]
    public Transform sylvanVillage;   // Góc Đông Bắc
    public Transform ironholdVillage; // Góc Tây Nam
    public Transform aurumVillage;    // Góc Đông Nam

    [Header("=== Định vị Legendary Merchant Hall (Quan trọng) ===")]
    public Transform legendaryHall;   // Đền thờ cổ kính (Chính Bắc bản đồ)

    [Header("=== Noise ===")]
    public int seed = 0;
    public float elevationScale = 0.06f;
    public float moistureScale = 0.10f;

    [Header("=== Ngưỡng độ cao ===")]
    [Range(0, 1)] public float waterLevel = 0.30f;   
    [Range(0, 1)] public float sandLevel = 0.35f;    
    [Range(0, 1)] public float landLevel = 0.72f;    
    [Range(0, 1)] public float stoneLevel = 0.84f;   

    [Header("=== Ngưỡng độ ẩm ===")]
    [Range(0, 1)] public float forestMoisture = 0.60f; 
    [Range(0, 1)] public float dirtMoisture = 0.28f;   
    [Range(0, 1)] public float flowerChance = 0.05f;   

    [Header("=== Tiles ===")]
    public List<TileBase> grassTiles = new List<TileBase>(); 
    public List<TileBase> flowerTiles = new List<TileBase>(); 
    public TileBase forestTile;       
    public TileBase sandTile;
    public TileBase dirtTile;
    public TileBase waterTile;
    public TileBase stoneTile;
    public TileBase mountainTile;

    public bool generateOnStart = true;

    private readonly List<Vector3Int> _spawnableCells = new List<Vector3Int>();
    public IReadOnlyList<Vector3Int> SpawnableCells => _spawnableCells;

    public Vector3 PlayerSpawnWorld { get; private set; }

    private System.Random _rng;

    private void Start()
    {
        if (generateOnStart) Generate();
    }

    [ContextMenu("Generate World")]
    public void Generate()
    {
        if (grid == null && groundTilemap != null) grid ??= groundTilemap.GetComponentInParent<Grid>();
        if (groundTilemap == null) { Debug.LogError("[WorldGenerator] Thiếu groundTilemap."); return; }
        if (grassTiles == null || grassTiles.Count == 0) { Debug.LogError("[WorldGenerator] grassTiles trống."); return; }

        groundTilemap.ClearAllTiles();
        WorldBlocking.Clear();
        if (grid != null) WorldBlocking.SetGrid(grid);
        _spawnableCells.Clear();

        int s = seed != 0 ? seed : Random.Range(1, 999999);
        _rng = new System.Random(s);
        
        float ox = (float)_rng.NextDouble() * 1000f;
        float oy = (float)_rng.NextDouble() * 1000f;
        float ox2 = (float)_rng.NextDouble() * 1000f;
        float oy2 = (float)_rng.NextDouble() * 1000f;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3Int cell = new Vector3Int(x, y, 0);

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
                    if (m >= forestMoisture)   { tile = forestTile != null ? forestTile : PickGrass(); spawnable = true; }
                    else if (m < dirtMoisture) { tile = dirtTile != null ? dirtTile : PickGrass(); }
                    else
                    {
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

        // Tự động sắp đặt vị trí 3 làng chính xa nhau
        LocateThreeVillages();

        // Tự động định vị Legendary Merchant Hall ở vùng Chính Bắc xa xôi
        LocateLegendaryMerchantHall();

        // Tính toán điểm xuất hiện của Player ở vùng trung tâm
        ComputePlayerSpawn();

        // FIX LỖI KẸT: Thực thi dịch chuyển người chơi tới điểm an toàn vừa sinh
        TeleportPlayerToSpawn();

        // Kích hoạt Spawner rải cây dã ngoại và cổng hầm ngục
        OverworldObjectSpawner spawner = FindObjectOfType<OverworldObjectSpawner>();
        if (spawner != null)
        {
            spawner.SpawnAllOverworldEntities();
        }

        Debug.Log($"[WorldGenerator] Sinh xong {width}x{height} (seed {s}) — chặn {WorldBlocking.BlockedCount} ô.");
    }

    private TileBase PickGrass() => grassTiles[_rng.Next(grassTiles.Count)];

    private void TeleportPlayerToSpawn()
    {
        Player player = FindObjectOfType<Player>();
        if (player != null)
        {
            player.transform.position = PlayerSpawnWorld;
            Camera.main.transform.position = new Vector3(PlayerSpawnWorld.x, PlayerSpawnWorld.y, Camera.main.transform.position.z);
        }
    }

    private void ComputePlayerSpawn()
    {
        Vector3Int center = new Vector3Int(width / 2, height / 2, 0);
        Vector3Int found = center;
        if (WorldBlocking.IsBlocked(center))
        {
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

    private void LocateThreeVillages()
    {
        PlayerController pc = FindObjectOfType<PlayerController>();

        // 1. Làng Sylvan (Đông Bắc)
        Vector3 sylvanPos = FindWalkableCellInArea(width * 3 / 4, width - 6, height * 3 / 4, height - 6);
        if (sylvanVillage != null)
        {
            sylvanVillage.position = sylvanPos;
            if (pc != null) pc.itemsOnGround.Add(sylvanPos);
        }

        // 2. Làng Ironhold (Tây Nam)
        Vector3 ironholdPos = FindWalkableCellInArea(6, width / 4, 6, height / 4);
        if (ironholdVillage != null)
        {
            ironholdVillage.position = ironholdPos;
            if (pc != null) pc.itemsOnGround.Add(ironholdPos);
        }

        // 3. Làng Aurum (Đông Nam)
        Vector3 aurumPos = FindWalkableCellInArea(width * 3 / 4, width - 6, 6, height / 4);
        if (aurumVillage != null)
        {
            aurumVillage.position = aurumPos;
            if (pc != null) pc.itemsOnGround.Add(aurumPos);
        }
    }

    // Tự động tìm khu vực đất liền an toàn ở phía Bắc bản đồ để đặt Legendary Merchant Hall
    private void LocateLegendaryMerchantHall()
    {
        PlayerController pc = FindObjectOfType<PlayerController>();
        
        // Vùng tìm kiếm: Nằm ở trục dọc trung tâm (giữa X), sát mép trên biên giới bản đồ (Y cao)
        int xStart = (width / 2) - 10;
        int xEnd = (width / 2) + 10;
        int yStart = height - 15;
        int yEnd = height - 6;

        Vector3 hallPos = FindWalkableCellInArea(xStart, xEnd, yStart, yEnd);
        if (legendaryHall != null)
        {
            legendaryHall.position = hallPos;
            if (pc != null)
            {
                // Thêm vị trí Legendary Hall vào danh sách cấm xây dựng đè đồ vật lên
                pc.itemsOnGround.Add(hallPos);
                
                // Đồng thời khóa cứng vị trí ô này lại trên ma trận để player không đi xuyên qua sảnh
                Vector3Int cell = groundTilemap.WorldToCell(hallPos);
                WorldBlocking.Block(cell);
            }
        }
    }

    private Vector3 FindWalkableCellInArea(int xStart, int xEnd, int yStart, int yEnd)
    {
        for (int x = xStart; x <= xEnd; x++)
        {
            for (int y = yStart; y <= yEnd; y++)
            {
                Vector3Int cell = new Vector3Int(x, y, 0);
                if (!WorldBlocking.IsBlocked(cell))
                {
                    return groundTilemap.GetCellCenterWorld(cell);
                }
            }
        }
        return groundTilemap.GetCellCenterWorld(new Vector3Int(width / 2, height / 2, 0));
    }
}