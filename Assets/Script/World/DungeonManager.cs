using UnityEngine;

public class DungeonManager : MonoBehaviour
{
    public static DungeonManager Instance { get; private set; }

    [Header("=== Dungeon Coordinate Offset ===")]
    [Tooltip("Dungeon sẽ được dựng tại tọa độ lệch xa so với Overworld.")]
    public Vector3Int dungeonOffset = new Vector3Int(500, 500, 0);

    [Header("=== Core References ===")]
    public DungeonGenerator dungeonGenerator;
    public GameObject playerInstance;

    [Header("=== Active State ===")]
    public int currentDepth = 0; // 0: Overworld, 1+: Dungeon Floors

    private Vector3 _overworldReturnPosition;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
    }

    // Tiến hành chui xuống hầm tối tầng 1
    public void EnterDungeon()
    {
        if (playerInstance == null) playerInstance = FindObjectOfType<Player>()?.gameObject;
        if (playerInstance == null) return;

        // Lưu lại vị trí Overworld trước khi bước vào cổng dịch chuyển
        _overworldReturnPosition = playerInstance.transform.position;

        currentDepth = 1;
        LoadDungeonFloor(currentDepth);
    }

    // Xuống tầng hầm tiếp theo
    public void DescendFloor()
    {
        currentDepth++;
        LoadDungeonFloor(currentDepth);
    }

    // Thoát khỏi Dungeon quay về tầng 0
    public void ExitDungeon()
    {
        currentDepth = 0;

        // Dọn dẹp dữ liệu chặn ô và dọn sạch các cấu trúc tile của Dungeon
        if (dungeonGenerator != null) dungeonGenerator.ClearDungeon();

        if (playerInstance == null) playerInstance = FindObjectOfType<Player>()?.gameObject;
        if (playerInstance != null)
        {
            // Đưa người chơi về vị trí lối vào cổng Dungeon ở thế giới chính
            playerInstance.transform.position = _overworldReturnPosition;
            Camera.main.transform.position = new Vector3(_overworldReturnPosition.x, _overworldReturnPosition.y, Camera.main.transform.position.z);
        }
    }

    private void LoadDungeonFloor(int floor)
    {
        if (dungeonGenerator == null) dungeonGenerator = FindObjectOfType<DungeonGenerator>();
        if (dungeonGenerator == null)
        {
            Debug.LogError("[DungeonManager] Thiếu tham chiếu tới DungeonGenerator trong Scene.");
            return;
        }

        // Tạo cấu trúc tầng hầm ngục mới
        dungeonGenerator.GenerateFloor(floor, dungeonOffset);

        // Dịch chuyển người chơi và máy quay đến tọa độ an toàn
        if (playerInstance == null) playerInstance = FindObjectOfType<Player>()?.gameObject;
        if (playerInstance != null)
        {
            Vector3 spawnPos = dungeonGenerator.PlayerSpawnWorldPos;
            playerInstance.transform.position = spawnPos;
            Camera.main.transform.position = new Vector3(spawnPos.x, spawnPos.y, Camera.main.transform.position.z);
        }
    }
}