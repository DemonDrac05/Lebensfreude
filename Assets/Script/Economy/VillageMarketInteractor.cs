using UnityEngine;

// Chuột phải vào collider làng -> mở VillageMarketUI cho làng đó. Mirror ProcessingStationInteractor.
public class VillageMarketInteractor : MonoBehaviour
{
    [SerializeField] private VillageMarketUI uiPanel; // 1 panel dùng chung; gán cho mọi làng
    private VillageMarket _market; private Collider2D _col; private PlayerController _tile;

    private void Awake()
    {
        _market = GetComponent<VillageMarket>();
        _col    = GetComponent<Collider2D>();
        _tile   = FindObjectOfType<PlayerController>();
        if (uiPanel == null) uiPanel = FindObjectOfType<VillageMarketUI>(true);
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
        if (uiPanel == null || _col == null || _market == null) return;
        Vector3 m = _tile != null ? _tile.mousePosUpdate : Camera.main.ScreenToWorldPoint(Input.mousePosition);
        m.z = 0f;
        if (!_col.OverlapPoint(m)) return;
        if (uiPanel.IsOpen && uiPanel.IsShowing(_market)) uiPanel.Close();
        else { InputManager.Instance?.ForceCloseActivePanel(); uiPanel.Open(_market); }
    }
}
