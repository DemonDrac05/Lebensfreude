using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plant : Farming
{
    [Header("=== General Settings ==========")]
    public GameObject gameObj;
    public Season[] season;

    [Header("=== Properties ==========")]
    public float durability;

    [Header("=== Planting Mechanics ==========")]
    public List<GameObject> stages = new List<GameObject>();
    public List<int> stageDuration = new List<int>();
    [HideInInspector] public Vector3 plantPosition;

    [Header("=== Product Settings ==========")]
    public List<ProductModifier> productList;

    [System.Serializable]
    public class ProductModifier
    {
        public HarvestType harvestType;
        public CropType cropType;
        public Product product;
        public int quantity;
    }

    public GameObject GetStagePrefab(int index)
    {
        if (index >= 0 && index < stages.Count)
            return stages[index];
        return null;
    }

    public int GetStageDuration(int index)
    {
        if (index >= 0 && index < stageDuration.Count)
            return stageDuration[index];
        return 0;
    }

    public int GetNextStageIndex(int currentIndex)
    {
        if (currentIndex >= 0 && currentIndex < stages.Count - 1)
            return currentIndex + 1;
        return -1;
    }
}
public enum HarvestType
{
    OnlyOneProduction,
    MultipleProduction,
}
public enum CropType
{
    None,
    Fruit,
    Vegetable
}
