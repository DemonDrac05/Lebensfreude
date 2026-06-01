using UnityEngine;

// ─────────────────────────────────────────
// CRAFTING STATION INTERACTOR  (world-space — chuột phải để mở/đóng UI)
// ─────────────────────────────────────────
// Gắn cùng prefab với CraftingStation. Phát hiện chuột phải khi mouse nằm trong Collider2D
// của station → mở CraftingStationUI tương ứng.
// Tự tìm panel theo stationType (không cần kéo tay) nhưng vẫn cho SerializeField override.
// Guard: InputBlocker.IsBlocked (ngủ/dream/hall/ending) chặn mở UI.
//
// Liên kết: CraftingStation (cùng GO, lấy stationType + gọi Craft),
//           CraftingStationUI (panel trên Canvas, tìm theo ForStationType),
//           PlayerController (mousePosUpdate, theo pattern Chest),
//           InputManager (ForceCloseActivePanel khi mở, toolbar hide/show qua CraftingStationUI).
public class CraftingStationInteractor : MonoBehaviour
{
    // ─────────────────────────────────────────
    // INSPECTOR
    // ─────────────────────────────────────────
    [Header("=== Panel UI (để trống = tự tìm theo stationType) ==========")]
    [SerializeField] private CraftingStationUI uiPanel;

    // ─────────────────────────────────────────
    // RUNTIME
    // ─────────────────────────────────────────
    private CraftingStation  _station;
    private Collider2D       _col;
    private PlayerController _tileControl;

    // ─────────────────────────────────────────
    // INIT
    // ─────────────────────────────────────────
    private void Awake()
    {
        _station     = GetComponent<CraftingStation>();
        _col         = GetComponent<Collider2D>();
        _tileControl = FindObjectOfType<PlayerController>();
    }

    // Start chạy sau Awake mọi object → panel đã tồn tại trong scene để FindObjectsOfType tìm thấy.
    // Dùng trong: Awake().
    private void Start()
    {
        if (uiPanel == null)
            uiPanel = FindPanelForStation();
    }

    // Tìm CraftingStationUI khớp stationType của station này. Dùng trong: Start().
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

    // ─────────────────────────────────────────
    // INPUT
    // ─────────────────────────────────────────
    private void Update()
    {
        if (InputBlocker.IsBlocked) return;

        // Đóng bằng ESC hoặc E khi panel đang mở
        if (uiPanel != null && uiPanel.IsOpen &&
            (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.E)))
        {
            uiPanel.Close();
            return;
        }

        // Chuột phải → check xem mouse có nằm trong collider station không
        if (Input.GetMouseButtonDown(1))
            TryToggle();
    }

    // Lấy vị trí mouse (ưu tiên mousePosUpdate để nhất quán với grid, fallback ScreenToWorld).
    // Kiểm tra Collider2D.OverlapPoint → toggle panel. Dùng trong: Update().
    private void TryToggle()
    {
        if (uiPanel == null || _col == null) return;

        // Lấy world-pos của mouse (nhất quán với grid như Chest)
        Vector3 mouseWorld = _tileControl != null
            ? _tileControl.mousePosUpdate
            : Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        if (!_col.OverlapPoint(mouseWorld)) return; // mouse không nằm trong station

        if (uiPanel.IsOpen)
        {
            uiPanel.Close();
        }
        else
        {
            // Đóng panel đang mở (inventory, shop...) trước khi mở station UI
            InputManager.Instance?.ForceCloseActivePanel();
            uiPanel.Open(_station);
        }
    }
}
