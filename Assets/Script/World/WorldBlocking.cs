using System.Collections.Generic;
using UnityEngine;

public static class WorldBlocking
{
    private static readonly HashSet<Vector3Int> _blocked = new HashSet<Vector3Int>();

    private static Grid _grid;

    public static void SetGrid(Grid grid) => _grid = grid;
    public static bool HasGrid => _grid != null;

    public static void Block(Vector3Int cell) => _blocked.Add(cell);
    public static void Unblock(Vector3Int cell) => _blocked.Remove(cell);
    public static bool IsBlocked(Vector3Int cell) => _blocked.Contains(cell);

    public static void Clear() => _blocked.Clear();

    public static Vector3Int WorldToCell(Vector3 worldPos)
        => _grid != null ? _grid.WorldToCell(worldPos) : Vector3Int.zero;

    public static bool IsBlockedWorld(Vector3 worldPos)
    {
        if (_grid == null) return false;
        return _blocked.Contains(_grid.WorldToCell(worldPos));
    }

    public static int BlockedCount => _blocked.Count;
}
