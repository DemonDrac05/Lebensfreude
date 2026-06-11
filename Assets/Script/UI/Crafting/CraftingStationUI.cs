using System.Collections.Generic;
using UnityEngine;

//
//
//           CraftingSlot (prefab, spawn trong slotsContainer),
public class CraftingStationUI : MonoBehaviour
{
    // INSPECTOR
    [Header("=== Station type this panel serves ==========")]
    [SerializeField] private CraftStation forStationType = CraftStation.Workbench;
    public CraftStation ForStationType => forStationType;

    [Header("=== Slot prefab & container ==========")]
    [SerializeField] private Transform   slotsContainer;
    [SerializeField] private GameObject  craftingSlotPrefab;

    // RUNTIME
    private CraftingStation       _station;
    private readonly List<CraftingSlot> _slots = new();

    public static CraftingStationUI CurrentOpen { get; private set; }

    public bool IsOpen => gameObject.activeSelf;

    public void Open(CraftingStation station)
    {
        if (station == null) return;

        if (CurrentOpen != null && CurrentOpen != this)
            CurrentOpen.Close();

        _station = station;
        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    public static void CloseIfOpen()
    {
        if (CurrentOpen != null && CurrentOpen.IsOpen)
            CurrentOpen.Close();
    }

    // UNITY LIFECYCLE
    private void OnEnable()
    {
        CurrentOpen = this;

        if (InputManager.Instance != null)
            InputManager.Instance.toolBar.SetActive(false);

        PopulateSlots();

        if (_station != null)
            _station.OnCraftListChanged += OnCraftListChanged;
    }

    private void OnDisable()
    {
        if (CurrentOpen == this) CurrentOpen = null;

        if (InputManager.Instance != null)
            InputManager.Instance.toolBar.SetActive(true);

        if (_station != null)
            _station.OnCraftListChanged -= OnCraftListChanged;

        ClearSlots();
        _station = null;
    }

    // SLOTS
    private void PopulateSlots()
    {
        ClearSlots();
        if (_station == null || slotsContainer == null || craftingSlotPrefab == null) return;

        foreach (var output in _station.Outputs)
        {
            if (output == null) continue;
            
            var go = Instantiate(craftingSlotPrefab, slotsContainer);
            var slot = go.GetComponent<CraftingSlot>();
            if (slot == null)
            {
                Debug.LogWarning("[CraftingStationUI] craftingSlotPrefab thiếu component CraftingSlot.", this);
                Destroy(go);
                continue;
            }
            slot.Setup(output, _station);
            _slots.Add(slot);
            
            Debug.Log(slot.name);
        }
    }

    private void OnCraftListChanged()
    {
        if (_station == null) return;

        if (_slots.Count != _station.Outputs.Count)
        {
            PopulateSlots();
            return;
        }

        foreach (var slot in _slots)
            slot.Refresh();
    }

    private void ClearSlots()
    {
        foreach (var slot in _slots)
        {
            if (slot != null)
                Destroy(slot.gameObject);
        }
        _slots.Clear();
    }
}
