using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DungeonGenerator : MonoBehaviour
{
    public static DungeonGenerator Instance { get; private set; }

    [Header("=== Tilemaps & Ground Tiles ===")]
    public Tilemap groundTilemap;
    public TileBase floorTile;
    public TileBase wallTile;

    [Header("=== Interactive Prefabs ===")]
    public GameObject stairsDownPrefab;
    public GameObject exitStairsPrefab;

    [Header("=== Spawning Helpers ===")]
    public OreGemSpawner oreGemSpawner;
    public PlayerController playerController;

    [Header("=== Dimensions ===")]
    public int width = 45;
    public int height = 45;

    [Header("=== Staircase Probability ===")]
    [Range(0f, 1f)] public float stairChance = 0.15f;

    public Vector3 PlayerSpawnWorldPos { get; private set; }

    private Vector3Int _offset;
    private int[,] _grid; // 0 = Wall, 1 = Floor
    private bool _staircaseSpawnedOnCurrentFloor;

    private readonly List<Vector3Int> _blockedCells = new();
    private readonly List<GameObject> _spawnedEntities = new();
    private readonly List<Vector3> _activeOresInLevel = new();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void ClearDungeon()
    {
        foreach (Vector3Int cell in _blockedCells)
        {
            WorldBlocking.Unblock(cell);
        }
        _blockedCells.Clear();

        if (groundTilemap != null)
        {
            for (int x = -5; x < width + 5; x++)
            {
                for (int y = -5; y < height + 5; y++)
                {
                    Vector3Int target = new Vector3Int(x + _offset.x, y + _offset.y, 0);
                    groundTilemap.SetTile(target, null);
                }
            }
        }

        foreach (GameObject entity in _spawnedEntities)
        {
            if (entity != null) Destroy(entity);
        }
        _spawnedEntities.Clear();
        _activeOresInLevel.Clear();
        _staircaseSpawnedOnCurrentFloor = false;
    }

    public void GenerateFloor(int depth, Vector3Int offset)
    {
        _offset = offset;
        ClearDungeon();

        int style = depth % 4;
        _grid = GenerateGridByStyle(style);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3Int cell = new Vector3Int(x + offset.x, y + offset.y, 0);
                if (_grid[x, y] == 1)
                {
                    groundTilemap.SetTile(cell, floorTile);
                }
                else
                {
                    groundTilemap.SetTile(cell, wallTile);
                    WorldBlocking.Block(cell);
                    _blockedCells.Add(cell);
                }
            }
        }

        CreateBoundaryOuterWalls(offset);

        List<Vector3Int> walkableCells = GetWalkableDungeonCells(offset);

        Vector3Int spawnCell = walkableCells[Random.Range(0, walkableCells.Count)];
        walkableCells.Remove(spawnCell);
        PlayerSpawnWorldPos = groundTilemap.GetCellCenterWorld(spawnCell);

        Vector3Int exitCell = walkableCells[Random.Range(0, walkableCells.Count)];
        walkableCells.Remove(exitCell);
        Vector3 exitPos = groundTilemap.GetCellCenterWorld(exitCell);
        if (exitStairsPrefab != null)
        {
            GameObject exitGo = Instantiate(exitStairsPrefab, exitPos, Quaternion.identity);
            _spawnedEntities.Add(exitGo);
        }

        if (oreGemSpawner != null)
        {
            oreGemSpawner.Spawn(depth);
        }

        ScanActiveDungeonOres();
    }

    private void CreateBoundaryOuterWalls(Vector3Int offset)
    {
        for (int x = -3; x < width + 3; x++)
        {
            for (int y = -3; y < height + 3; y++)
            {
                if (x < 0 || y < 0 || x >= width || y >= height)
                {
                    Vector3Int cell = new Vector3Int(x + offset.x, y + offset.y, 0);
                    groundTilemap.SetTile(cell, wallTile);
                    WorldBlocking.Block(cell);
                    _blockedCells.Add(cell);
                }
            }
        }
    }

    private List<Vector3Int> GetWalkableDungeonCells(Vector3Int offset)
    {
        List<Vector3Int> cells = new();
        for (int x = 2; x < width - 2; x++)
        {
            for (int y = 2; y < height - 2; y++)
            {
                if (_grid[x, y] == 1)
                {
                    cells.Add(new Vector3Int(x + offset.x, y + offset.y, 0));
                }
            }
        }
        return cells;
    }

    private void ScanActiveDungeonOres()
    {
        _activeOresInLevel.Clear();
        var ores = FindObjectsOfType<MineableDeposit>();
        foreach (var ore in ores)
        {
            Vector3 pos = ore.transform.position;
            if (pos.x >= _offset.x && pos.x <= _offset.x + width &&
                pos.y >= _offset.y && pos.y <= _offset.y + height)
            {
                _activeOresInLevel.Add(pos);
                _spawnedEntities.Add(ore.gameObject);
            }
        }
    }

    public void OnOreMined(Vector3 worldPosition)
    {
        _activeOresInLevel.Remove(worldPosition);

        if (_staircaseSpawnedOnCurrentFloor) return;

        bool hasChance = Random.value <= stairChance;
        bool isLastOne = _activeOresInLevel.Count == 0;

        if (hasChance || isLastOne)
        {
            SpawnStairsDown(worldPosition);
        }
    }

    private void SpawnStairsDown(Vector3 worldPosition)
    {
        if (stairsDownPrefab == null) return;

        // Spawn the staircase right next to the PLAYER, on the nearest walkable floor cell
        // (never inside a wall), instead of at the mined ore (which can be far away).
        Player player = FindObjectOfType<Player>();
        Vector3 anchor = player != null ? player.transform.position
                       : (playerController != null ? playerController.transform.position : worldPosition);
        Vector3Int origin = groundTilemap.WorldToCell(anchor);
        Vector3Int cell = FindNearestFloorCell(origin);

        Vector3 snapped = groundTilemap.GetCellCenterWorld(cell);
        GameObject stairs = Instantiate(stairsDownPrefab, snapped, Quaternion.identity);
        _spawnedEntities.Add(stairs);
        _staircaseSpawnedOnCurrentFloor = true;
    }

    // Nearest walkable floor cell to 'origin', searched ring by ring so the staircase lands
    // right next to the player. Guaranteed to be real floor and not a wall.
    private Vector3Int FindNearestFloorCell(Vector3Int origin)
    {
        for (int r = 1; r <= 8; r++)
        {
            for (int dx = -r; dx <= r; dx++)
            {
                for (int dy = -r; dy <= r; dy++)
                {
                    if (Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy)) != r) continue; // only this ring's edge
                    Vector3Int c = new Vector3Int(origin.x + dx, origin.y + dy, 0);
                    if (IsFloorCell(c)) return c;
                }
            }
        }
        return origin;
    }

    private bool IsFloorCell(Vector3Int cell)
    {
        int gx = cell.x - _offset.x;
        int gy = cell.y - _offset.y;
        if (gx < 0 || gy < 0 || gx >= width || gy >= height) return false;
        return _grid[gx, gy] == 1 && !WorldBlocking.IsBlocked(cell);
    }

    private int[,] GenerateGridByStyle(int style)
    {
        switch (style)
        {
            case 0: return StyleOrganicCaves();
            case 1: return StyleCentralHubSpokes();
            case 2: return StyleSpiralTunnel();
            case 3: return StyleChamberBridge();
            default: return StyleOrganicCaves();
        }
    }

    private int[,] StyleOrganicCaves()
    {
        int[,] grid = new int[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (x == 0 || y == 0 || x == width - 1 || y == height - 1)
                    grid[x, y] = 0;
                else
                    grid[x, y] = (Random.value < 0.44f) ? 0 : 1;
            }
        }

        for (int i = 0; i < 3; i++)
        {
            int[,] temp = new int[width, height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (x == 0 || y == 0 || x == width - 1 || y == height - 1)
                    {
                        temp[x, y] = 0;
                        continue;
                    }
                    int count = 0;
                    for (int nx = x - 1; nx <= x + 1; nx++)
                        for (int ny = y - 1; ny <= y + 1; ny++)
                            if (grid[nx, ny] == 0) count++;

                    temp[x, y] = (count > 4) ? 0 : 1;
                }
            }
            grid = temp;
        }
        return grid;
    }

    private int[,] StyleCentralHubSpokes()
    {
        int[,] grid = new int[width, height];
        int cx = width / 2;
        int cy = height / 2;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float d = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                if (d <= 5.5f) grid[x, y] = 1;
            }
        }

        DrawDungeonCorridor(grid, cx, cy, 1, 0, 13);
        DrawDungeonCorridor(grid, cx, cy, -1, 0, 13);
        DrawDungeonCorridor(grid, cx, cy, 0, 1, 13);
        DrawDungeonCorridor(grid, cx, cy, 0, -1, 13);

        return grid;
    }

    private void DrawDungeonCorridor(int[,] grid, int sx, int sy, int dx, int dy, int len)
    {
        int tx = sx;
        int ty = sy;
        for (int i = 0; i < len; i++)
        {
            tx += dx;
            ty += dy;
            if (tx > 2 && tx < width - 3 && ty > 2 && ty < height - 3)
            {
                grid[tx, ty] = 1;
                grid[tx + dy, ty + dx] = 1;
            }
        }

        for (int rx = tx - 3; rx <= tx + 3; rx++)
        {
            for (int ry = ty - 3; ry <= ty + 3; ry++)
            {
                if (rx > 1 && rx < width - 2 && ry > 1 && ry < height - 2)
                    grid[rx, ry] = 1;
            }
        }
    }

    private int[,] StyleSpiralTunnel()
    {
        int[,] grid = new int[width, height];
        int xMin = 4, xMax = width - 5;
        int yMin = 4, yMax = height - 5;

        while (xMin <= xMax && yMin <= yMax)
        {
            for (int tx = xMin; tx <= xMax; tx++) ApplyWormBrush(grid, tx, yMin);
            yMin += 4;
            for (int ty = yMin - 4; ty <= yMax; ty++) ApplyWormBrush(grid, xMax, ty);
            xMax -= 4;
            for (int tx = xMax + 4; tx >= xMin; tx--) ApplyWormBrush(grid, tx, yMax);
            yMax -= 4;
            for (int ty = yMax + 4; ty >= yMin; ty--) ApplyWormBrush(grid, xMin, ty);
            xMin += 4;
        }
        return grid;
    }

    private void ApplyWormBrush(int[,] grid, int cx, int cy)
    {
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                int rx = cx + i;
                int ry = cy + j;
                if (rx > 1 && rx < width - 2 && ry > 1 && ry < height - 2)
                    grid[rx, ry] = 1;
            }
        }
    }

    private int[,] StyleChamberBridge()
    {
        int[,] grid = new int[width, height];

        for (int x = 4; x < 14; x++)
            for (int y = 10; y < 32; y++)
                grid[x, y] = 1;

        for (int x = 30; x < 40; x++)
            for (int y = 10; y < 32; y++)
                grid[x, y] = 1;

        int currentY = 21;
        for (int x = 13; x <= 31; x++)
        {
            if (x == 22)
            {
                for (int y = 14; y <= 28; y++) grid[x, y] = 1;
                currentY = 14;
            }
            grid[x, currentY] = 1;
            grid[x, currentY + 1] = 1;
        }

        return grid;
    }
}