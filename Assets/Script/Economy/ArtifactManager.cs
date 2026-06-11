using System;
using System.Collections.Generic;
using UnityEngine;

//
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

    public void Grant(ArtifactType type)
    {
        if (type == ArtifactType.None || _collected.Contains(type)) return;
        _collected.Add(type);
        OnArtifactCollected?.Invoke(type);
        if (HasAllArtifacts) OnAllArtifactsCollected?.Invoke();
    }

    public bool HasArtifact(ArtifactType type) => _collected.Contains(type);
    public int  Count => _collected.Count;

    public bool HasAllArtifacts =>
        _collected.Contains(ArtifactType.Forest) &&
        _collected.Contains(ArtifactType.Mountain) &&
        _collected.Contains(ArtifactType.Golden);

    private readonly HashSet<ArtifactType> _inserted = new();

    public event Action<ArtifactType> OnArtifactInserted;
    public event Action OnAllInserted;

    public int EarnedCount => _collected.Count;

    public void Insert(ArtifactType type)
    {
        if (type == ArtifactType.None || _inserted.Contains(type)) return;
        _inserted.Add(type);
        OnArtifactInserted?.Invoke(type);
        if (AllInserted) OnAllInserted?.Invoke();
    }

    public bool IsInserted(ArtifactType type) => _inserted.Contains(type);

    public bool AllInserted =>
        _inserted.Contains(ArtifactType.Forest) &&
        _inserted.Contains(ArtifactType.Mountain) &&
        _inserted.Contains(ArtifactType.Golden);
}
