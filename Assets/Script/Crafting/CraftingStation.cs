using System;
using System.Collections.Generic;
using UnityEngine;

// ─────────────────────────────────────────
// CRAFTING STATION  (chế tạo TỨC THỜI — Workbench, Alchemy Table)
// ─────────────────────────────────────────
// Liệt kê outputs trong Inspector; đọc recipe TỪ item output. Bấm craft -> trừ nguyên liệu -> ra ngay.
// Workbench craft tất cả station (gồm Bonfire), gộp recipe Drying Rack + Mortar & Pestle, và Journal (1 lần).
//
// Liên kết: BaseItem.recipe, InventoryManager (CountItem/RemoveItem/AddItem), MerchantJournal (Unlock).
public class CraftingStation : MonoBehaviour
{
    [Header("=== Loại station ==========")]
    public CraftStation stationType = CraftStation.Workbench;

    [Header("=== Danh sách món craft được ở đây ==========")]
    [SerializeField] private List<BaseItem> outputs = new();

    public IReadOnlyList<BaseItem> Outputs => outputs;
    public event Action OnCraftListChanged;   // UI nghe để refresh (vd sau craftOnce xóa món)

    // Đủ nguyên liệu + fuel để craft món này chưa? Dùng trong: UI (bật/khóa nút), Craft().
    public bool CanCraft(BaseItem output)
    {
        if (output == null || InventoryManager.Instance == null) return false;
        var r = output.recipe;
        if (r == null || !r.IsCraftableAt(stationType)) return false;

        foreach (var inp in r.inputs)
            if (inp == null || inp.material == null
                || InventoryManager.Instance.CountItem(inp.material) < inp.quantity) return false;

        if (r.fuel != null && r.fuelAmount > 0
            && InventoryManager.Instance.CountItem(r.fuel) < r.fuelAmount) return false;

        return true;
    }

    // Craft 1 món: trừ nguyên liệu/fuel -> ra output ngay (instant luôn thành công). Dùng trong: UI (nút Craft).
    public bool Craft(BaseItem output)
    {
        if (!CanCraft(output)) return false;
        var r = output.recipe;

        foreach (var inp in r.inputs)
            InventoryManager.Instance.RemoveItem(inp.material, inp.quantity);
        if (r.fuel != null && r.fuelAmount > 0)
            InventoryManager.Instance.RemoveItem(r.fuel, r.fuelAmount);

        for (int i = 0; i < Mathf.Max(1, r.outputAmount); i++)
            InventoryManager.Instance.AddItem(output);

        if (r.unlocksMerchantJournal) MerchantJournal.Instance?.Unlock();
        if (r.craftOnce)
        {
            outputs.Remove(output);          // craft 1 lần rồi xóa recipe
            OnCraftListChanged?.Invoke();
        }
        return true;
    }
}
