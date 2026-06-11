using UnityEngine;

public class DungeonManager : MonoBehaviour
{
    public static DungeonManager Instance { get; private set; }

    [Header("=== Dungeon Coordinate Offset ===")]
    [Tooltip("The dungeon is built at coordinates far from the Overworld.")]
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

    public void EnterDungeon()
    {
        if (playerInstance == null) playerInstance = FindObjectOfType<Player>()?.gameObject;
        if (playerInstance == null) return;

        _overworldReturnPosition = playerInstance.transform.position;

        currentDepth = 1;
        LoadDungeonFloor(currentDepth);
    }

    public void DescendFloor()
    {
        currentDepth++;
        LoadDungeonFloor(currentDepth);
    }

    public void ExitDungeon()
    {
        currentDepth = 0;

        if (dungeonGenerator != null) dungeonGenerator.ClearDungeon();

        if (playerInstance == null) playerInstance = FindObjectOfType<Player>()?.gameObject;
        if (playerInstance != null)
        {
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

        dungeonGenerator.GenerateFloor(floor, dungeonOffset);

        if (playerInstance == null) playerInstance = FindObjectOfType<Player>()?.gameObject;
        if (playerInstance != null)
        {
            Vector3 spawnPos = dungeonGenerator.PlayerSpawnWorldPos;
            playerInstance.transform.position = spawnPos;
            Camera.main.transform.position = new Vector3(spawnPos.x, spawnPos.y, Camera.main.transform.position.z);
        }
    }
}