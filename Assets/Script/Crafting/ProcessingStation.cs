using System;
using System.Collections.Generic;
using UnityEngine;

// ─────────────────────────────────────────
// PROCESSING STATION  (chế tạo THEO THỜI GIAN — Smelter, Forge)  [Option 2]
// ─────────────────────────────────────────
// Cho vào LIÊN TỤC cùng 1 loại; mỗi lần Add nấu thêm 1 mẻ (trừ nguyên liệu + fuel ngay).
// Các mẻ xong ở thời điểm KHÁC nhau (theo Time.time thực trong game). Bấm Collect nhặt các mẻ đã xong;
// KHO ĐẦY thì giữ lại trong lò. Mỗi mẻ roll thành công/thất bại -> ra output hoặc byproduct.
// Khóa: chỉ nấu 1 loại cho tới khi nấu hết + nhặt hết thì nhả khóa.
//
// Liên kết: BaseItem.recipe, InventoryManager (CountItem/RemoveItem/AddItem). Fuel cố định (coal/charcoal).
public class ProcessingStation : MonoBehaviour
{
    [Header("=== Loại station ==========")]
    public CraftStation stationType = CraftStation.Smelter;

    [Header("=== Danh sách món nấu được ở đây ==========")]
    [SerializeField] private List<BaseItem> outputs = new();
    public IReadOnlyList<BaseItem> Outputs => outputs;

    // ─────────────────────────────────────────
    // RUNTIME STATE
    // ─────────────────────────────────────────
    private BaseItem _currentRecipe;                  // loại đang nấu (khóa cùng loại)
    private readonly List<float>    _cookingFinish = new(); // thời điểm hoàn thành từng mẻ
    private readonly List<BaseItem> _ready = new();         // kết quả đã xong, chờ nhặt (đã roll)

    public BaseItem CurrentRecipe => _currentRecipe;
    public int CookingCount => _cookingFinish.Count;
    public int ReadyCount   => _ready.Count;
    public event Action OnStateChanged;               // UI nghe để cập nhật (đang nấu / sẵn sàng)

    // ─────────────────────────────────────────
    // ADD  — cho thêm 1 mẻ
    // ─────────────────────────────────────────
    // Đủ điều kiện thêm 1 mẻ món này không? (đúng station, cùng loại đang nấu, đủ nguyên liệu+fuel).
    // Dùng trong: UI (bật/khóa nút Add), Add().
    public bool CanAdd(BaseItem output)
    {
        if (output == null || InventoryManager.Instance == null) return false;
        var r = output.recipe;
        if (r == null || !r.IsCraftableAt(stationType)) return false;
        if (_currentRecipe != null && _currentRecipe != output) return false; // chỉ 1 loại/lần

        foreach (var inp in r.inputs)
            if (inp == null || inp.material == null
                || InventoryManager.Instance.CountItem(inp.material) < inp.quantity) return false;
        if (r.fuel != null && r.fuelAmount > 0
            && InventoryManager.Instance.CountItem(r.fuel) < r.fuelAmount) return false;

        return true;
    }

    // Trừ nguyên liệu+fuel ngay rồi xếp 1 mẻ vào lò. Dùng trong: UI (nút Add).
    public bool Add(BaseItem output)
    {
        if (!CanAdd(output)) return false;
        var r = output.recipe;

        foreach (var inp in r.inputs)
            InventoryManager.Instance.RemoveItem(inp.material, inp.quantity);
        if (r.fuel != null && r.fuelAmount > 0)
            InventoryManager.Instance.RemoveItem(r.fuel, r.fuelAmount);

        _currentRecipe = output;
        _cookingFinish.Add(Time.time + Mathf.Max(0.1f, r.craftTimeSeconds));
        OnStateChanged?.Invoke();
        return true;
    }

    // ─────────────────────────────────────────
    // COOK  — đến hạn thì chuyển mẻ sang "sẵn sàng" (roll success/fail)
    // ─────────────────────────────────────────
    private void Update()
    {
        if (_cookingFinish.Count == 0 || _currentRecipe == null) return;

        bool changed = false;
        var r = _currentRecipe.recipe;
        for (int i = _cookingFinish.Count - 1; i >= 0; i--)
        {
            if (Time.time < _cookingFinish[i]) continue;

            bool success = UnityEngine.Random.value <= r.successRate;
            BaseItem result = success ? _currentRecipe : r.failByproduct;
            int amount = success ? Mathf.Max(1, r.outputAmount) : Mathf.Max(1, r.failByproductAmount);

            if (result != null)
                for (int k = 0; k < amount; k++) _ready.Add(result);

            _cookingFinish.RemoveAt(i);
            changed = true;
        }
        if (changed) OnStateChanged?.Invoke();
    }

    // ─────────────────────────────────────────
    // COLLECT  — nhặt các mẻ đã xong (kho đầy thì giữ lại)
    // ─────────────────────────────────────────
    // Trả về số món đã nhặt được vào kho. Dùng trong: UI (bấm vào ô output).
    public int Collect()
    {
        int collected = 0;
        for (int i = _ready.Count - 1; i >= 0; i--)
        {
            var item = _ready[i];
            if (item == null) { _ready.RemoveAt(i); continue; }

            if (InventoryManager.Instance != null && InventoryManager.Instance.AddItem(item))
            {
                _ready.RemoveAt(i);
                collected++;
            }
            else break; // kho đầy -> giữ phần còn lại trong lò
        }

        // Nấu hết + nhặt hết -> nhả khóa để đổi loại khác.
        if (_cookingFinish.Count == 0 && _ready.Count == 0) _currentRecipe = null;
        if (collected > 0) OnStateChanged?.Invoke();
        return collected;
    }
}
