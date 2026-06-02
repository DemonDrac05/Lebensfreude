using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// ─────────────────────────────────────────
// ORE GEM SPAWNER  (rải deposit quặng/đá quý ở GIỮA Ô theo tầng)
// ─────────────────────────────────────────
// Spawn(floor): lấy ô có tile, chọn random, đặt deposit theo OreGemTable.PickForFloor tại GetCellCenterWorld.
// Sau khi đặt: đăng ký vị trí tile vào PlayerController.itemsOnGround để placement engine
// hiển thị hitbox ĐỎ khi player cố đặt vật lên ô đã có deposit (tích hợp đúng tile system).
//
// Liên kết: OreGemTable (chọn loại), MineableDeposit (Configure + RegisterWithTileSystem),
//           Tilemap (ô + tâm ô), PlayerController (itemsOnGround tile tracking).
public class OreGemSpawner : MonoBehaviour
{
    [Header("=== Scene references ==========")]
    [SerializeField] private Tilemap         groundTilemap;
    [SerializeField] private OreGemTable     table;
    [SerializeField] private PlayerController playerController; // để đăng ký tile (tự tìm nếu để trống)

    [Header("=== Spawn config ==========")]
    [SerializeField] private int  floor          = 1;   // overworld = 1; dungeon đặt theo độ sâu
    [SerializeField] private int  depositsToSpawn = 12;
    [SerializeField] private int  seed           = 0;
    [SerializeField] private bool spawnOnStart   = true;

    private void Awake()
    {
        // Tự tìm PlayerController nếu không kéo tay trong Inspector
        if (playerController == null)
            playerController = FindObjectOfType<PlayerController>();
    }

    private void Start() { if (spawnOnStart) Spawn(floor); }

    // Rải deposit cho floor chỉ định. Gọi từ Start hoặc từ DungeonGenerator.
    public void Spawn(int floorLevel)
    {
        if (table == null || groundTilemap == null) return;
        var rng = new System.Random(seed + floorLevel * 1000);

        // Thu thập tất cả ô thực sự có tile (tránh spawn vào vùng trống trong bounds)
        var cells = new List<Vector3Int>();
        foreach (var pos in groundTilemap.cellBounds.allPositionsWithin)
            if (groundTilemap.HasTile(pos)) cells.Add(pos);

        for (int i = 0; i < depositsToSpawn && cells.Count > 0; i++)
        {
            // Lấy ô ngẫu nhiên (RemoveAt tránh trùng vị trí)
            int idx  = rng.Next(cells.Count);
            var cell = cells[idx]; cells.RemoveAt(idx);

            var entry = table.PickForFloor(floorLevel, rng);
            if (entry == null || entry.depositPrefab == null) continue;

            // Đặt ĐÚNG tâm ô — nhất quán với GetSnappedMousePosition / GetCellCenterWorld
            Vector3 center = groundTilemap.GetCellCenterWorld(cell);
            var go  = Instantiate(entry.depositPrefab, new Vector2(center.x, center.y - 0.5f), Quaternion.identity, transform);
            var dep = go.GetComponent<MineableDeposit>();
            if (dep == null) continue;

            dep.Configure(entry.dropItem, rng.Next(entry.dropAmountMin, entry.dropAmountMax + 1));

            // Đăng ký tile vào PlayerController.itemsOnGround:
            // → placement engine sẽ hiện hitbox ĐỎ khi player trỏ vào ô này.
            // Format khớp với cách PlayerController lưu itemsOnGround (center.y - 0.5f).
            dep.RegisterWithTileSystem(center, playerController);
        }
    }
}
