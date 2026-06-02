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
    public GameObject stairsDownPrefab; // Thang chui xuống tầng dưới
    public GameObject exitStairsPrefab; // Thang đi lên mặt đất (Reset về 0)

    [Header("=== Spawning Helpers ===")]
    public OreGemSpawner oreGemSpawner;
    public PlayerController playerController;

    [Header("=== Dimensions ===")]
    public int width = 45;
    public int height = 45;

    [Header("=== Staircase Probability ===")]
    [Range(0f, 1f)] public float stairChance = 0.15f; // Tỷ lệ đập đá ra thang

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

    // Xóa sạch dấu vết tầng cũ
    public void ClearDungeon()
    {
        foreach (Vector3Int cell in _blockedCells)
        {
            WorldBlocking.Unblock(cell);
        }
        _blockedCells.Clear();

        if (groundTilemap != null)
        {
            // Dọn sạch Tilemap trong phạm vi phòng Dungeon
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

    // Sinh tầng mới
    public void GenerateFloor(int depth, Vector3Int offset)
    {
        _offset = offset;
        ClearDungeon();

        // Xoay vòng các thuật toán sinh map ngẫu nhiên theo tầng
        int style = depth % 4;
        _grid = GenerateGridByStyle(style);

        // Vẽ gạch nền và định vị các khối đá chặn đường
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

        // Tạo lớp viền ngoài tuyệt đối an toàn chặn người chơi đi ra ngoài hư vô
        CreateBoundaryOuterWalls(offset);

        // Thu thập các ô đi được
        List<Vector3Int> walkableCells = GetWalkableDungeonCells(offset);

        // 1. Xác định điểm xuất hiện an toàn của người chơi
        Vector3Int spawnCell = walkableCells[Random.Range(0, walkableCells.Count)];
        walkableCells.Remove(spawnCell);
        PlayerSpawnWorldPos = groundTilemap.GetCellCenterWorld(spawnCell);

        // 2. Sinh thang đi lên mặt đất (đặt ở điểm xa spawn)
        Vector3Int exitCell = walkableCells[Random.Range(0, walkableCells.Count)];
        walkableCells.Remove(exitCell);
        Vector3 exitPos = groundTilemap.GetCellCenterWorld(exitCell);
        if (exitStairsPrefab != null)
        {
            GameObject exitGo = Instantiate(exitStairsPrefab, exitPos, Quaternion.identity);
            _spawnedEntities.Add(exitGo);
        }

        // 3. Tiến hành gọi rải quặng
        if (oreGemSpawner != null)
        {
            oreGemSpawner.Spawn(depth);
        }

        // 4. Nhận dạng danh sách các quặng vừa sinh dưới lòng đất
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
            // Lọc ra các quặng thuộc phạm vi khu vực Dungeon
            if (pos.x >= _offset.x && pos.x <= _offset.x + width &&
                pos.y >= _offset.y && pos.y <= _offset.y + height)
            {
                _activeOresInLevel.Add(pos);
                _spawnedEntities.Add(ore.gameObject);
            }
        }
    }

    // Được gọi từ MineableDeposit khi người chơi đập quặng vỡ
    public void OnOreMined(Vector3 worldPosition)
    {
        _activeOresInLevel.Remove(worldPosition);

        if (_staircaseSpawnedOnCurrentFloor) return;

        bool hasChance = Random.value <= stairChance;
        bool isLastOne = _activeOresInLevel.Count == 0;

        // Nếu quay trúng thưởng hoặc đập tới khối quặng cuối cùng -> Mở thang đi tiếp 100%
        if (hasChance || isLastOne)
        {
            SpawnStairsDown(worldPosition);
        }
    }

    private void SpawnStairsDown(Vector3 worldPosition)
    {
        if (stairsDownPrefab == null) return;

        Vector3Int cell = groundTilemap.WorldToCell(worldPosition);
        Vector3 snapped = groundTilemap.GetCellCenterWorld(cell);

        GameObject stairs = Instantiate(stairsDownPrefab, snapped, Quaternion.identity);
        _spawnedEntities.Add(stairs);
        _staircaseSpawnedOnCurrentFloor = true;
    }

    // ─────────────────────────────────────────
    // CÁC THUẬT TOÁN HÌNH DẠNG KHÔNG GIAN
    // ─────────────────────────────────────────
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

    // Thuật toán 1: Sinh hang động hữu cơ (Cellular Automata)
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

        // Thực hiện làm mượt địa hình 3 lần
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

    // Thuật toán 2: Sinh phòng trung tâm lớn tỏa nhánh 4 hướng
    private int[,] StyleCentralHubSpokes()
    {
        int[,] grid = new int[width, height];
        int cx = width / 2;
        int cy = height / 2;

        // Sinh sảnh chính tròn lớn ở tâm
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float d = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                if (d <= 5.5f) grid[x, y] = 1;
            }
        }

        // Kéo đường đi ra 4 hướng
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
                grid[tx + dy, ty + dx] = 1; // Tạo hành lang dày 2 ô
            }
        }

        // Phòng cuối mỗi nhánh
        for (int rx = tx - 3; rx <= tx + 3; rx++)
        {
            for (int ry = ty - 3; ry <= ty + 3; ry++)
            {
                if (rx > 1 && rx < width - 2 && ry > 1 && ry < height - 2)
                    grid[rx, ry] = 1;
            }
        }
    }

    // Thuật toán 3: Sinh đường hầm xoắn ốc sâu dần vào tâm
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

    // Thuật toán 4: Hai phòng lớn đối diện nối với nhau qua nhịp cầu zig-zag
    private int[,] StyleChamberBridge()
    {
        int[,] grid = new int[width, height];

        // Buồng bên trái
        for (int x = 4; x < 14; x++)
            for (int y = 10; y < 32; y++)
                grid[x, y] = 1;

        // Buồng bên phải
        for (int x = 30; x < 40; x++)
            for (int y = 10; y < 32; y++)
                grid[x, y] = 1;

        // Cầu nối
        int currentY = 21;
        for (int x = 13; x <= 31; x++)
        {
            if (x == 22)
            {
                // Đoạn bẻ cua dọc dốc đứng
                for (int y = 14; y <= 28; y++) grid[x, y] = 1;
                currentY = 14;
            }
            grid[x, currentY] = 1;
            grid[x, currentY + 1] = 1;
        }

        return grid;
    }
}