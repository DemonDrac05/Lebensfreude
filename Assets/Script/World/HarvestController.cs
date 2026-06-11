using UnityEngine;
using UnityEngine.EventSystems;

// Central, robust harvest input. On left-click it raycasts the mouse position and hits the
// Harvestable or MineableDeposit under the cursor with the selected tool. Replaces fragile
// per-object OnMouseDown (which a UI raycast or missing event setup can silently block).
// Self-creates at runtime, so no scene setup is required.
public class HarvestController : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        var go = new GameObject("HarvestController");
        go.AddComponent<HarvestController>();
        DontDestroyOnLoad(go);
    }

    private void Update()
    {
        if (InputBlocker.IsBlocked) return;
        if (!Input.GetMouseButtonDown(0)) return;
        if (InventoryManager.Instance == null || InventoryManager.Instance.toolbar == null
            || !InventoryManager.Instance.toolbar.activeSelf) return;                 // only while playing
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return; // not through UI
        if (Camera.main == null) return;

        Vector3 mw = Camera.main.ScreenToWorldPoint(Input.mousePosition); mw.z = 0f;
        Tool tool = InventoryManager.Instance.GetSelectedItem<Tool>(false);

        foreach (var col in Physics2D.OverlapPointAll(mw))
        {
            var harv = col.GetComponentInParent<Harvestable>();
            if (harv != null && harv.TryHit(tool)) return;
            var dep = col.GetComponentInParent<MineableDeposit>();
            if (dep != null && dep.TryMine(tool)) return;
        }
    }
}
