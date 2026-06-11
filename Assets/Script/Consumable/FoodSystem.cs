using UnityEngine;

//
public class FoodSystem : MonoBehaviour
{
    private void Update()
    {
        if (InputBlocker.IsBlocked) return;
        if (!Input.GetMouseButtonDown(1)) return;
        if (InventoryManager.Instance == null) return;
        if (InventoryManager.Instance.toolbar == null
            || !InventoryManager.Instance.toolbar.activeSelf) return;

        var food = InventoryManager.Instance.GetSelectedItem<Consumable>(false);
        if (food == null) return;

        var sm = StaminaManager.Instance;
        if (sm != null && sm.Current >= sm.Max) return;

        sm?.Restore(food.staminaRestore);
        InventoryManager.Instance.GetSelectedItem<Consumable>(true);
    }
}
