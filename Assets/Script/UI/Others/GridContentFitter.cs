using UnityEngine;
using UnityEngine.UI;

// Tự canh chiều cao (hoặc rộng) của Content theo GridLayoutGroup: cellSize + spacing + padding + số con đang active.
// Gắn lên Content (cùng GO với GridLayoutGroup). Bật autoUpdate hoặc gọi Fit() sau khi thêm/xóa slot.
[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(GridLayoutGroup))]
public class GridContentFitter : MonoBehaviour
{
    [SerializeField] private bool autoUpdate = true;   // tự canh mỗi frame (tiện, hơi tốn) — tắt thì gọi Fit() thủ công
    [SerializeField] private bool fitWidthInstead = false; // true: canh chiều RỘNG (grid cuộn ngang)

    private GridLayoutGroup _grid; private RectTransform _rt;
    private void Awake() { _grid = GetComponent<GridLayoutGroup>(); _rt = GetComponent<RectTransform>(); }
    private void OnEnable() 
    {
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(_rt);
        Fit();
    }
    private void LateUpdate() { if (autoUpdate) Fit(); }

    public void Fit()
    {
        if (_grid == null || _rt == null) return;
        int count = 0;
        foreach (Transform c in transform) if (c.gameObject.activeSelf) count++;

        var p = _grid.padding;
        if (!fitWidthInstead)
        {
            int cols = ColsFor(count);
            int rows = Mathf.CeilToInt((float)count / Mathf.Max(1, cols));
            float h = p.top + p.bottom + rows * _grid.cellSize.y + Mathf.Max(0, rows - 1) * _grid.spacing.y;
            _rt.sizeDelta = new Vector2(_rt.sizeDelta.x, h);
        }
        else
        {
            int rows = RowsFor(count);
            int cols = Mathf.CeilToInt((float)count / Mathf.Max(1, rows));
            float w = p.left + p.right + cols * _grid.cellSize.x + Mathf.Max(0, cols - 1) * _grid.spacing.x;
            _rt.sizeDelta = new Vector2(w, _rt.sizeDelta.y);
        }
    }

    private int ColsFor(int count)
    {
        if (_grid.constraint == GridLayoutGroup.Constraint.FixedColumnCount) return Mathf.Max(1, _grid.constraintCount);
        if (_grid.constraint == GridLayoutGroup.Constraint.FixedRowCount)
            return Mathf.CeilToInt((float)count / Mathf.Max(1, _grid.constraintCount));
        float w = _rt.rect.width - _grid.padding.left - _grid.padding.right;            // Flexible -> ước theo bề rộng
        return Mathf.Max(1, Mathf.FloorToInt((w + _grid.spacing.x) / (_grid.cellSize.x + _grid.spacing.x)));
    }
    private int RowsFor(int count)
    {
        if (_grid.constraint == GridLayoutGroup.Constraint.FixedRowCount) return Mathf.Max(1, _grid.constraintCount);
        return 1;
    }
}
