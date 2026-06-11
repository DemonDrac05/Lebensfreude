using System.Collections.Generic;
using UnityEngine;

// Fills the hand-crafting panel (toggled with C) with every recipe whose station is Hand.
// Attach this to the hand-craft panel GameObject. It auto-discovers the recipes from the
// ItemDatabase, so no manual list is needed, and reuses the existing CraftingSlot prefab.
public class HandCraftingPanel : MonoBehaviour
{
    [Header("=== Data source ===")]
    [SerializeField] private ItemDatabase itemDatabase;   // drag the project's ItemDatabase asset here

    [Header("=== Slot prefab & container (same prefab the station UI uses) ===")]
    [SerializeField] private Transform   slotsContainer;
    [SerializeField] private GameObject  craftingSlotPrefab;

    private CraftingStation _handStation;
    private readonly List<CraftingSlot> _slots = new();

    // A hidden CraftingStation set to Hand: CanCraft/Craft only use the item's recipe + this type,
    // so its own outputs list can stay empty.
    private CraftingStation HandStation
    {
        get
        {
            if (_handStation == null)
            {
                _handStation = gameObject.AddComponent<CraftingStation>();
                _handStation.stationType = CraftStation.Hand;
            }
            return _handStation;
        }
    }

    private void OnEnable()  => Populate();
    private void OnDisable() => Clear();

    private void Populate()
    {
        Clear();
        if (itemDatabase == null || slotsContainer == null || craftingSlotPrefab == null)
        {
            Debug.LogWarning("[HandCraftingPanel] Assign itemDatabase, slotsContainer and craftingSlotPrefab.", this);
            return;
        }

        foreach (var item in itemDatabase.allItems)
        {
            if (item == null || item.recipe == null) continue;
            if (!item.recipe.IsCraftableAt(CraftStation.Hand)) continue;

            var go = Instantiate(craftingSlotPrefab, slotsContainer);
            var slot = go.GetComponent<CraftingSlot>();
            if (slot == null) { Destroy(go); continue; }
            slot.Setup(item, HandStation);
            _slots.Add(slot);
        }
    }

    // Re-evaluate every slot's craftable state (call after a craft if you want live colours).
    public void RefreshAll()
    {
        foreach (var s in _slots) if (s != null) s.Refresh();
    }

    private void Clear()
    {
        foreach (var s in _slots) if (s != null) Destroy(s.gameObject);
        _slots.Clear();
    }
}
