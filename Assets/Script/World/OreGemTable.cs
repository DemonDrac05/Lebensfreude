using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/World/OreGemTable")]
public class OreGemTable : ScriptableObject
{
    [Serializable]
    public class Entry
    {
        public string label;
        public GameObject depositPrefab;
        public BaseItem dropItem;
        [Tooltip("x = floor, y = spawn weight. Shape the curve per type.")]
        public AnimationCurve weightByFloor = AnimationCurve.Constant(0, 80, 1f);
        public int dropAmountMin = 1;
        public int dropAmountMax = 2;
    }

    public List<Entry> entries = new();

    public Entry PickForFloor(int floor, System.Random rng)
    {
        float total = 0f;
        var w = new List<float>(entries.Count);
        foreach (var e in entries)
        {
            float val = e != null ? Mathf.Max(0f, e.weightByFloor.Evaluate(floor)) : 0f;
            w.Add(val); total += val;
        }
        if (total <= 0f) return null;

        double roll = rng.NextDouble() * total, cum = 0;
        for (int i = 0; i < entries.Count; i++)
        {
            cum += w[i];
            if (roll < cum) return entries[i];
        }
        return entries[entries.Count - 1];
    }
}
