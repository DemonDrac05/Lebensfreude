using UnityEngine;

// ─────────────────────────────────────────
// FOOD SYSTEM  (ăn vật phẩm để hồi stamina)
// ─────────────────────────────────────────
// CHỌN 1 Consumable trên toolbar + NHẤN CHUỘT PHẢI để ăn (đã chốt). Stamina ĐẦY thì KHÔNG ăn (đỡ phí).
// Chặn khi đang có overlay (InputBlocker) hoặc đang mở panel (toolbar ẩn).
//
// Liên kết: InventoryManager (GetSelectedItem<Consumable>), StaminaManager (Restore), InputBlocker.
public class FoodSystem : MonoBehaviour
{
    private void Update()
    {
        if (InputBlocker.IsBlocked) return;                  // đang ngủ/overlay
        if (!Input.GetMouseButtonDown(1)) return;            // chỉ chuột phải
        if (InventoryManager.Instance == null) return;
        if (InventoryManager.Instance.toolbar == null
            || !InventoryManager.Instance.toolbar.activeSelf) return; // đang mở panel -> không ăn

        var food = InventoryManager.Instance.GetSelectedItem<Consumable>(false);
        if (food == null) return;                            // món đang chọn không phải đồ ăn

        var sm = StaminaManager.Instance;
        if (sm != null && sm.Current >= sm.Max) return;      // stamina đầy -> không cần ăn

        sm?.Restore(food.staminaRestore);
        InventoryManager.Instance.GetSelectedItem<Consumable>(true); // tiêu 1 món đang chọn
    }
}
