using System.Collections.Generic;
using UnityEngine;

// ─────────────────────────────────────────
// CRAFTING STATION UI  (panel trong Canvas — instant station: Workbench, Alchemy, Hand)
// ─────────────────────────────────────────
// Mỗi loại station có 1 panel riêng trong Canvas với ForStationType tương ứng.
// CraftingStationInteractor tự tìm panel này theo stationType và gọi Open(station).
//
// OnEnable: ẩn toolbar (nhất quán Chest/ShopItemManager) + spawn CraftingSlot cho mỗi output.
// OnDisable: hiện toolbar + unsubscribe event + destroy slots.
// Nghe CraftingStation.OnCraftListChanged để rebuild khi craftOnce xóa món.
//
// Liên kết: CraftingStation (Outputs, OnCraftListChanged),
//           CraftingSlot (prefab, spawn trong slotsContainer),
//           InputManager (toolbar toggle, tương tự ChestUI),
//           CraftingStationInteractor (gọi Open/Close).
public class CraftingStationUI : MonoBehaviour
{
    // ─────────────────────────────────────────
    // INSPECTOR
    // ─────────────────────────────────────────
    [Header("=== Loại station panel này phục vụ ==========")]
    [SerializeField] private CraftStation forStationType = CraftStation.Workbench;
    public CraftStation ForStationType => forStationType;

    [Header("=== Slot prefab & container ==========")]
    [SerializeField] private Transform   slotsContainer;       // parent spawn các slot
    [SerializeField] private GameObject  craftingSlotPrefab;   // prefab có component CraftingSlot

    // ─────────────────────────────────────────
    // RUNTIME
    // ─────────────────────────────────────────
    private CraftingStation       _station;
    private readonly List<CraftingSlot> _slots = new();

    // Static ref: InputManager gọi CloseIfOpen() khi mở panel khác. Pattern InventoryItem.CurrentMovingItem.
    public static CraftingStationUI CurrentOpen { get; private set; }

    // Trạng thái mở/đóng. Dùng trong: CraftingStationInteractor.Update() + TryToggle().
    public bool IsOpen => gameObject.activeSelf;

    // ─────────────────────────────────────────
    // OPEN / CLOSE  (gọi từ CraftingStationInteractor)
    // ─────────────────────────────────────────
    // Mở panel với station chỉ định. Dùng trong: CraftingStationInteractor.TryToggle().
    public void Open(CraftingStation station)
    {
        if (station == null) return;

        // Nếu panel khác đang mở → đóng trước (vd mở Workbench khi Alchemy đang mở)
        if (CurrentOpen != null && CurrentOpen != this)
            CurrentOpen.Close();

        _station = station;
        gameObject.SetActive(true);  // → trigger OnEnable
    }

    // Đóng panel. Dùng trong: CraftingStationInteractor.Update(), InputManager.KeyPressMethod().
    public void Close()
    {
        gameObject.SetActive(false); // → trigger OnDisable
    }

    // Đóng nếu bất kỳ panel nào đang mở. Gọi từ InputManager.KeyPressMethod() (1 dòng thêm).
    // Dùng trong: InputManager.
    public static void CloseIfOpen()
    {
        if (CurrentOpen != null && CurrentOpen.IsOpen)
            CurrentOpen.Close();
    }

    // ─────────────────────────────────────────
    // UNITY LIFECYCLE
    // ─────────────────────────────────────────
    private void OnEnable()
    {
        CurrentOpen = this;

        // Ẩn toolbar (nhất quán với ChestUI.OnEnable)
        if (InputManager.Instance != null)
            InputManager.Instance.toolBar.SetActive(false);

        PopulateSlots();

        // Nghe event craftOnce xóa món khỏi danh sách
        if (_station != null)
            _station.OnCraftListChanged += OnCraftListChanged;
    }

    private void OnDisable()
    {
        if (CurrentOpen == this) CurrentOpen = null;

        // Hiện toolbar (nhất quán với ChestUI.OnDisable)
        if (InputManager.Instance != null)
            InputManager.Instance.toolBar.SetActive(true);

        if (_station != null)
            _station.OnCraftListChanged -= OnCraftListChanged;

        ClearSlots();
        _station = null;
    }

    // ─────────────────────────────────────────
    // SLOTS
    // ─────────────────────────────────────────
    // Spawn 1 CraftingSlot cho mỗi output trong station.Outputs. Dùng trong: OnEnable.
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

    // Xử lý khi CraftingStation.OnCraftListChanged fire (vd craftOnce xóa 1 món).
    // Nếu số slot thay đổi → rebuild; nếu không → chỉ Refresh màu.
    // Dùng trong: OnEnable (subscribe), OnDisable (unsubscribe).
    private void OnCraftListChanged()
    {
        if (_station == null) return;

        // Số output thay đổi → spawn lại toàn bộ (craftOnce đã xóa 1 món)
        if (_slots.Count != _station.Outputs.Count)
        {
            PopulateSlots();
            return;
        }

        // Số output giữ nguyên → chỉ refresh màu/text
        foreach (var slot in _slots)
            slot.Refresh();
    }

    // Destroy tất cả slot đang có. Dùng trong: PopulateSlots() (trước khi spawn lại), OnDisable.
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
