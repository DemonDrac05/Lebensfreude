using System.Collections.Generic;
using UnityEngine;

// ╔══════════════════════════════════════════════════════════════════╗
// ║  WorldBlocking — DỊCH VỤ CHẶN Ô (tile-based collision)            ║
// ╠══════════════════════════════════════════════════════════════════╣
// ║  Vì sao KHÔNG dùng physics collider?                              ║
// ║   - Player di chuyển bằng rb2d.MovePosition. Nếu Rigidbody2D là   ║
// ║     Kinematic thì MovePosition XUYÊN collider (đây là lý do ref   ║
// ║     fail dù có TilemapCollider2D).                                ║
// ║   - Map generate runtime -> set up collider dễ sai nhiều setting. ║
// ║                                                                    ║
// ║  Cách này: generator GHI các ô bị chặn (water/đá/núi/tường) vào   ║
// ║  1 HashSet. Movement chỉ HỎI HashSet trước khi đi.                ║
// ║   -> O(1) lookup, KHÔNG physics, gần như 0 tải CPU/FPS.           ║
// ║   -> Tường KHỎI cần collider.                                     ║
// ╚══════════════════════════════════════════════════════════════════╝
public static class WorldBlocking
{
    // Tập các ô bị chặn. Dùng Vector3Int (toạ độ cell của Grid).
    private static readonly HashSet<Vector3Int> _blocked = new HashSet<Vector3Int>();

    // Grid để quy đổi world <-> cell. Generator set 1 lần lúc khởi tạo.
    private static Grid _grid;

    // Gán Grid (WorldGenerator/DungeonGenerator gọi 1 lần). ───────────────
    public static void SetGrid(Grid grid) => _grid = grid;
    public static bool HasGrid => _grid != null;

    // Ghi / xoá / hỏi 1 ô bị chặn. ────────────────────────────────────────
    public static void Block(Vector3Int cell) => _blocked.Add(cell);
    public static void Unblock(Vector3Int cell) => _blocked.Remove(cell);
    public static bool IsBlocked(Vector3Int cell) => _blocked.Contains(cell);

    // Xoá toàn bộ (reset khi regenerate cả thế giới). ─────────────────────
    public static void Clear() => _blocked.Clear();

    // Quy đổi world position -> cell. ─────────────────────────────────────
    public static Vector3Int WorldToCell(Vector3 worldPos)
        => _grid != null ? _grid.WorldToCell(worldPos) : Vector3Int.zero;

    // Ô tại world position có bị chặn không? (MovementState gọi.) ──────────
    //  - Chưa có Grid (chưa generate) -> trả false để không kẹt player.
    public static bool IsBlockedWorld(Vector3 worldPos)
    {
        if (_grid == null) return false;
        return _blocked.Contains(_grid.WorldToCell(worldPos));
    }

    public static int BlockedCount => _blocked.Count;
}
