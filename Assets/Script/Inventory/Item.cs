using JetBrains.Annotations;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using static PlantingMechanics;

[CreateAssetMenu(menuName = "ScriptableObjects/Item")]
public class Item : ScriptableObject
{
    [Header("Gameplay")]
    public TileBase tile;
    public Slider slider;

    public ItemType itemType;
    public ActionType actionType;

    [Header("UI")]
    public bool stackable = true;

    [Header("Both")]
    public Sprite image;

    #region <PLANTSEED_TYPE_COMPONENT>
    [HideInInspector] public Vector3 plantPosition;
    [HideInInspector] public PlantType plantType;
    [HideInInspector] public int harvestCount;
    [HideInInspector] public List<GameObject> stages = new List<GameObject>();
    [HideInInspector] public List<float> stageDuration = new List<float>();
    [HideInInspector] public Item productItem;
    

    public SerializedObject serializedObject;

    public SerializedProperty plantTypeProperty;
    public SerializedProperty harvestCountProperty;
    public SerializedProperty stagesProperty;
    public SerializedProperty stageDurationProperty;
    public SerializedProperty productItemProperty;

    public GameObject GetStagePrefab(int index)
    {
        if (index >= 0 && index < stages.Count)
            return stages[index];
        return null;
    }

    public float GetStageDuration(int index)
    {
        if (index >= 0 && index < stageDuration.Count)
            return stageDuration[index];
        return 0f;
    }

    public int GetNextStageIndex(int currentIndex)
    {
        if (currentIndex >= 0 && currentIndex < stages.Count - 1)
            return currentIndex + 1;
        return -1;
    }
    #endregion

    #region <PLANT_PRODUCT_COMPONENT>
    [HideInInspector] public GameObject product;

    public SerializedProperty productProperty;
    #endregion

    private void OnEnable()
    {
        serializedObject = new SerializedObject(this);

        #region <PlantSeed> Property
        plantTypeProperty = serializedObject.FindProperty ("plantType");
        harvestCountProperty = serializedObject.FindProperty("harvestCount");
        stagesProperty = serializedObject.FindProperty("stages");
        stageDurationProperty = serializedObject.FindProperty("stageDuration");
        productItemProperty = serializedObject.FindProperty("productItem");
        #endregion

        #region <PlantProduct> Property
        productProperty = serializedObject.FindProperty("product");
        #endregion
    }

    public void OnValidate()
    {
        if (serializedObject == null)
            return;

        #region <PlantSeed> Illustration
        if (itemType == ItemType.PlantSeed || itemType == ItemType.PlantSapling)
        {
            plantTypeProperty.isExpanded = true;
            harvestCountProperty.isExpanded= true;
            stagesProperty.isExpanded = true;
            stageDurationProperty.isExpanded = true;
            productItemProperty.isExpanded = true;
        }
        else
        {
            plantTypeProperty.isExpanded = false;
            harvestCountProperty.isExpanded = false;
            stagesProperty.isExpanded = false;
            stageDurationProperty.isExpanded = false;
            productItemProperty.isExpanded = false;
        }
        #endregion

        if(itemType == ItemType.PlantProduct)
        {
            productProperty.isExpanded = true;
        }
        else productProperty.isExpanded = false;
    }
}

[CustomEditor(typeof(Item))]
public class ItemEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Item item = (Item)target;

        item.serializedObject.Update();

        DrawDefaultInspector();

        if (item.itemType == ItemType.PlantSeed || item.itemType == ItemType.PlantSapling)
        {
            #region <Plant_Seed> Inspector Label
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("PLANT_SEED_SETTING", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            #endregion

            #region <Plant_Seed> Inspector Properties
            EditorGUILayout.PropertyField(item.plantTypeProperty, true);
            EditorGUILayout.PropertyField(item.harvestCountProperty, true);
            EditorGUILayout.PropertyField(item.stagesProperty, true);
            EditorGUILayout.PropertyField(item.stageDurationProperty, true);
            EditorGUILayout.PropertyField(item.productItemProperty, true);
            #endregion
        }

        if (item.itemType == ItemType.PlantProduct)
        {
            EditorGUILayout.PropertyField(item.productProperty, true);
        }

        item.serializedObject.ApplyModifiedProperties();
    }
}

public enum ItemType
{
    BuildingBlock,
    PlantProduct,
    PlantSeed,
    PlantSapling,
    Tool
}

public enum ActionType
{
    Water,
    Chop,
    Cultivate,
    Sow,
    Harvest,
    Mine,
    Attack
}

public enum PlantType
{
    OnlyOneProduction,
    MultipleProduction,
}