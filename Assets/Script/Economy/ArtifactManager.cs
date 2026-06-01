using System;
using System.Collections.Generic;
using UnityEngine;

// ─────────────────────────────────────────
// ARTIFACT MANAGER  (theo dõi 3 mảnh Artifact mở Legendary Hall)
// ─────────────────────────────────────────
// Singleton SỐNG QUA SCENE. Nhận Grant() từ VillageProgressionManager khi 1 làng hoàn thành revival.
// Cung cấp HasArtifact / HasAllArtifacts cho DreamHintSystem (hint vị trí Hall) và LegendaryHall (mở cửa).
//
// Liên kết: VillageProgressionManager.TryAdvance (gọi Grant), DreamHintSystem.RollSleepHint (HasAllArtifacts),
//           LegendaryHall (kiểm tra để mở ending), EndingManager.
public class ArtifactManager : MonoBehaviour
{
    public static ArtifactManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
    }

    private readonly HashSet<ArtifactType> _collected = new();

    public event Action<ArtifactType> OnArtifactCollected;
    public event Action OnAllArtifactsCollected;

    // Trao 1 mảnh artifact. Bỏ qua None/trùng. Dùng trong: VillageProgressionManager.TryAdvance().
    public void Grant(ArtifactType type)
    {
        if (type == ArtifactType.None || _collected.Contains(type)) return;
        _collected.Add(type);
        OnArtifactCollected?.Invoke(type);
        if (HasAllArtifacts) OnAllArtifactsCollected?.Invoke();
    }

    public bool HasArtifact(ArtifactType type) => _collected.Contains(type);
    public int  Count => _collected.Count;

    // Đã đủ cả 3 mảnh chưa? Dùng trong: DreamHintSystem (hint Hall), LegendaryHall (mở cửa).
    public bool HasAllArtifacts =>
        _collected.Contains(ArtifactType.Forest) &&
        _collected.Contains(ArtifactType.Mountain) &&
        _collected.Contains(ArtifactType.Golden);

    // ─────────────────────────────────────────
    // INSERTED SEALS  (đã CẮM vào Hall — cho ending; tách khỏi _collected = đã kiếm)
    // ─────────────────────────────────────────
    private readonly HashSet<ArtifactType> _inserted = new();

    public event Action<ArtifactType> OnArtifactInserted;
    public event Action OnAllInserted;

    public int EarnedCount => _collected.Count;   // = số làng đã hồi sinh (dùng cho màn Ending)

    // Ghi nhận 1 seal đã cắm vào Hall. Dùng trong: LegendaryHall.OnMouseDown().
    public void Insert(ArtifactType type)
    {
        if (type == ArtifactType.None || _inserted.Contains(type)) return;
        _inserted.Add(type);
        OnArtifactInserted?.Invoke(type);
        if (AllInserted) OnAllInserted?.Invoke();
    }

    public bool IsInserted(ArtifactType type) => _inserted.Contains(type);

    // Đã cắm đủ 3 seal chưa? Dùng trong: LegendaryHall.AfterLore() -> EndingManager.
    public bool AllInserted =>
        _inserted.Contains(ArtifactType.Forest) &&
        _inserted.Contains(ArtifactType.Mountain) &&
        _inserted.Contains(ArtifactType.Golden);
}
