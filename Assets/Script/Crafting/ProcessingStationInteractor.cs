using UnityEngine;

public class ProcessingStationInteractor : MonoBehaviour
{
    [SerializeField] private ProcessingStationUI uiPanel;
    private ProcessingStation _station; private Collider2D _col; private PlayerController _tile;

    private void Awake()
    {
        _station = GetComponent<ProcessingStation>();
        _col     = GetComponent<Collider2D>();
        _tile    = FindObjectOfType<PlayerController>();
    }
    private void Start() { if (uiPanel == null) uiPanel = FindPanel(); }

    private ProcessingStationUI FindPanel()
    {
        if (_station == null) return null;
        foreach (var p in FindObjectsOfType<ProcessingStationUI>(true))
            if (p.ForStationType == _station.stationType) return p;
        Debug.LogWarning($"[ProcessingStationInteractor] Không tìm thấy ProcessingStationUI cho {_station.stationType}", this);
        return null;
    }

    private void Update()
    {
        if (InputBlocker.IsBlocked) return;
        if (uiPanel != null && uiPanel.IsOpen && (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.E)))
        { uiPanel.Close(); return; }
        if (Input.GetMouseButtonDown(1)) TryToggle();
    }

    private void TryToggle()
    {
        if (uiPanel == null || _col == null) return;
        Vector3 m = _tile != null ? _tile.mousePosUpdate : Camera.main.ScreenToWorldPoint(Input.mousePosition);
        m.z = 0f;
        if (!_col.OverlapPoint(m)) return;
        if (uiPanel.IsOpen) uiPanel.Close();
        else { InputManager.Instance?.ForceCloseActivePanel(); uiPanel.Open(_station); }
    }
}
