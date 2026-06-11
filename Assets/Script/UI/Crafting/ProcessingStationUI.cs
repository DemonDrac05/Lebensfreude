using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProcessingStationUI : MonoBehaviour
{
    [Header("=== Station type ==========")]
    [SerializeField] private CraftStation forStationType = CraftStation.Smelter;
    public CraftStation ForStationType => forStationType;

    [Header("=== List recipe ==========")]
    [SerializeField] private Transform  slotsContainer;
    [SerializeField] private GameObject processingSlotPrefab;

    [Header("=== Finished products + Collect ==========")]
    [SerializeField] private Transform  outputContainer;
    [SerializeField] private GameObject outputRowPrefab;
    [SerializeField] private Button     collectButton;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private Slider          progressSlider;
    [SerializeField] private Color colorReady = Color.white;

    private ProcessingStation _station;
    private readonly List<ProcessingSlot> _slots   = new();
    private readonly List<IngredientRow>  _outRows = new();

    public static ProcessingStationUI CurrentOpen { get; private set; }
    public bool IsOpen => gameObject.activeSelf;

    public void Open(ProcessingStation s)
    {
        if (s == null) return;
        if (CurrentOpen != null && CurrentOpen != this) CurrentOpen.Close();
        _station = s; gameObject.SetActive(true);
    }
    public void Close() => gameObject.SetActive(false);
    public static void CloseIfOpen() { if (CurrentOpen != null && CurrentOpen.IsOpen) CurrentOpen.Close(); }

    private void OnEnable()
    {
        CurrentOpen = this;
        if (InputManager.Instance != null) InputManager.Instance.toolBar.SetActive(false);
        if (collectButton != null) { collectButton.onClick.RemoveAllListeners(); collectButton.onClick.AddListener(OnCollect); }
        PopulateSlots();
        if (_station != null) _station.OnStateChanged += OnStateChanged;
        RefreshOutput();
    }
    private void OnDisable()
    {
        if (CurrentOpen == this) CurrentOpen = null;
        if (InputManager.Instance != null) InputManager.Instance.toolBar.SetActive(true);
        if (_station != null) _station.OnStateChanged -= OnStateChanged;
        ClearSlots(); ClearOutput(); _station = null;
    }

    private void Update()
    {
        if (_station == null) return;
        if (timeText != null)
            timeText.text = _station.CookingCount > 0 ? $"Remaining: {_station.NextFinishRemaining:0.0}s" : "—";
        if (progressSlider != null)
        {
            bool cooking = _station.CookingCount > 0;
            if (progressSlider.gameObject.activeSelf != cooking) progressSlider.gameObject.SetActive(cooking);
            if (cooking) progressSlider.value = _station.NextBatchProgress01;
        }
    }

    private void PopulateSlots()
    {
        ClearSlots();
        if (_station == null || slotsContainer == null || processingSlotPrefab == null) return;
        foreach (var o in _station.Outputs)
        {
            if (o == null) continue;
            var go = Instantiate(processingSlotPrefab, slotsContainer);
            var sl = go.GetComponent<ProcessingSlot>();
            if (sl == null) { Destroy(go); continue; }
            sl.Setup(o, _station); _slots.Add(sl);
        }
    }

    private void OnStateChanged()
    {
        foreach (var s in _slots) s.Refresh();
        RefreshOutput();
    }

    private void RefreshOutput()
    {
        ClearOutput();
        if (_station == null) return;
        if (outputContainer != null && outputRowPrefab != null)
        {
            foreach (var kv in _station.GetReadySummary())
            {
                var go = Instantiate(outputRowPrefab, outputContainer);
                var row = go.GetComponent<IngredientRow>();
                if (row == null) { Destroy(go); continue; }
                row.Setup(kv.Key, kv.Value, false, colorReady, colorReady);
                _outRows.Add(row);
            }
        }
        if (statusText != null) statusText.text = $"Refining: {_station.CookingCount}\nReady: {_station.ReadyCount}";
        if (collectButton != null) collectButton.interactable = _station.ReadyCount > 0;
    }

    private void OnCollect() { if (_station != null) { _station.Collect(); RefreshOutput(); } }
    private void ClearSlots()  { foreach (var s in _slots)   if (s != null) Destroy(s.gameObject);   _slots.Clear(); }
    private void ClearOutput() { foreach (var r in _outRows) if (r != null) Destroy(r.gameObject);   _outRows.Clear(); }
}