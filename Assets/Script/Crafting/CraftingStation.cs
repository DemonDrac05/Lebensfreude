using System;
using System.Collections.Generic;
using UnityEngine;

//
public class CraftingStation : MonoBehaviour
{
    [Header("=== Station type ==========")]
    public CraftStation stationType = CraftStation.Workbench;

    [Header("=== Items craftable here ==========")]
    [SerializeField] private List<BaseItem> outputs = new();

    public IReadOnlyList<BaseItem> Outputs => outputs;
    public event Action OnCraftListChanged;

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
            outputs.Remove(output);
            OnCraftListChanged?.Invoke();
        }
        return true;
    }
}
