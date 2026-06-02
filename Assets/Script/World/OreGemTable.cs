using System;
using System.Collections.Generic;
using UnityEngine;

// ─────────────────────────────────────────
// ORE GEM TABLE  (bảng spawn quặng/đá quý theo TẦNG)
// ─────────────────────────────────────────
// Mỗi entry: item rớt ra + deposit prefab + ĐƯỜNG CONG trọng số theo tầng (AnimationCurve, chỉnh trực quan).
// Càng xuống sâu: chỉnh curve để quặng thường giảm, gem hiếm tăng dần. Dùng trong: OreGemSpawner.
[CreateAssetMenu(menuName = "ScriptableObjects/World/OreGemTable")]
public class OreGemTable : ScriptableObject
{
    [Serializable]
    public class Entry
    {
        public string label;                         // ghi chú (vd "Copper", "Painite")
        public GameObject depositPrefab;             // object đặt xuống (có MineableDeposit)
        public BaseItem dropItem;                    // piece nhặt được khi đào (Copper, Emerald...)
        [Tooltip("x = số tầng, y = trọng số spawn. Chỉnh hình curve cho từng loại.")]
        public AnimationCurve weightByFloor = AnimationCurve.Constant(0, 80, 1f);
        public int dropAmountMin = 1;
        public int dropAmountMax = 2;
    }

    public List<Entry> entries = new();

    // Chọn 1 entry theo trọng số tại 'floor'. Dùng trong: OreGemSpawner.Spawn().
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
