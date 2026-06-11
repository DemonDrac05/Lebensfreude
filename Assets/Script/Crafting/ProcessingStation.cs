using System;
using System.Collections.Generic;
using UnityEngine;

//
public class ProcessingStation : MonoBehaviour
{
    [Header("=== Station type ==========")]
    public CraftStation stationType = CraftStation.Smelter;

    [Header("=== Items cookable here ==========")]
    [SerializeField] private List<BaseItem> outputs = new();
    public IReadOnlyList<BaseItem> Outputs => outputs;

    // RUNTIME STATE
    private BaseItem _currentRecipe;
    private readonly List<float>    _cookingFinish = new();
    private readonly List<BaseItem> _ready = new();

    public BaseItem CurrentRecipe => _currentRecipe;
    public int CookingCount => _cookingFinish.Count;
    public int ReadyCount   => _ready.Count;
    public event Action OnStateChanged;

    public bool CanAdd(BaseItem output)
    {
        if (output == null || InventoryManager.Instance == null) return false;
        var r = output.recipe;
        if (r == null || !r.IsCraftableAt(stationType)) return false;
        if (_currentRecipe != null && _currentRecipe != output) return false;

        foreach (var inp in r.inputs)
            if (inp == null || inp.material == null
                || InventoryManager.Instance.CountItem(inp.material) < inp.quantity) return false;
        if (r.fuel != null && r.fuelAmount > 0
            && InventoryManager.Instance.CountItem(r.fuel) < r.fuelAmount) return false;

        return true;
    }

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
            else break;
        }

        if (_cookingFinish.Count == 0 && _ready.Count == 0) _currentRecipe = null;
        if (collected > 0) OnStateChanged?.Invoke();
        return collected;
    }

    public Dictionary<BaseItem, int> GetReadySummary()
    {
        var d = new Dictionary<BaseItem, int>();
        foreach (var it in _ready)
        {
            if (it == null) continue;
            d.TryGetValue(it, out int n);
            d[it] = n + 1;
        }
        return d;
    }

    public float NextFinishRemaining
    {
        get
        {
            if (_cookingFinish.Count == 0) return 0f;
            float soonest = float.MaxValue;
            foreach (var t in _cookingFinish) if (t < soonest) soonest = t;
            return Mathf.Max(0f, soonest - Time.time);
        }
    }

    public float NextBatchProgress01
    {
        get
        {
            if (_cookingFinish.Count == 0 || _currentRecipe == null || _currentRecipe.recipe == null) return 0f;
            float ct = Mathf.Max(0.1f, _currentRecipe.recipe.craftTimeSeconds);
            return Mathf.Clamp01(1f - NextFinishRemaining / ct);
        }
    }

    public float TotalRemaining
    {
        get
        {
            if (_cookingFinish.Count == 0) return 0f;
            float latest = 0f;
            foreach (var t in _cookingFinish) if (t > latest) latest = t;
            return Mathf.Max(0f, latest - Time.time);
        }
    }
}
