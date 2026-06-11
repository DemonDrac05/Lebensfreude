using UnityEngine;

//
//           PlayerController (mousePosUpdate, theo pattern Chest),
public class CraftingStationInteractor : MonoBehaviour
{
    // INSPECTOR
    [Header("=== UI panel (leave empty = auto-find by stationType) ==========")]
    [SerializeField] private CraftingStationUI uiPanel;

    // RUNTIME
    private CraftingStation  _station;
    private Collider2D       _col;
    private PlayerController _tileControl;

    // INIT
    private void Awake()
    {
        _station     = GetComponent<CraftingStation>();
        _col         = GetComponent<Collider2D>();
        _tileControl = FindObjectOfType<PlayerController>();
    }

    private void Start()
    {
        if (uiPanel == null)
            uiPanel = FindPanelForStation();
    }

    private CraftingStationUI FindPanelForStation()
    {
        if (_station == null) return null;
        foreach (var panel in FindObjectsOfType<CraftingStationUI>(includeInactive: true))
        {
            if (panel.ForStationType == _station.stationType)
                return panel;
        }
        Debug.LogWarning($"[CraftingStationInteractor] Không tìm thấy CraftingStationUI " +
                         $"cho stationType={_station.stationType}", this);
        return null;
    }

    // INPUT
    private void Update()
    {
        if (InputBlocker.IsBlocked) return;

        if (uiPanel != null && uiPanel.IsOpen &&
            (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.E)))
        {
            uiPanel.Close();
            return;
        }

        if (Input.GetMouseButtonDown(1))
            TryToggle();
    }

    private void TryToggle()
    {
        if (uiPanel == null || _col == null) return;

        Vector3 mouseWorld = _tileControl != null
            ? _tileControl.mousePosUpdate
            : Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        if (!_col.OverlapPoint(mouseWorld)) return;

        if (uiPanel.IsOpen)
        {
            uiPanel.Close();
        }
        else
        {
            InputManager.Instance?.ForceCloseActivePanel();
            uiPanel.Open(_station);
        }
    }
}
