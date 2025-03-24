using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UpgradeButton : MonoBehaviour, IPointerClickHandler
{
    [Header("=== Slots Required ==========")]
    public GameObject materialSlot;
    public GameObject refineSlot;
    public GameObject outputSlot;
    public GameObject inputSlot;

    [Header("=== GUI Components ==========")]
    public Slider timeSlider;

    [Header("=== Instance ==========")]
    public static UpgradeButton Instance;

    [Header("=== Properties ==========")]
    [HideInInspector] public bool enableToInteract = true;
    private float upgradeTimeRemaining = 0f;
    private int upgradeCost;

    private void Awake()
    {
        Instance = this;
        InitialiseSliderValue();
    }

    private void InitialiseSliderValue()
    {
        timeSlider.minValue = 0f;
        timeSlider.value = timeSlider.maxValue;
    }

    private void Update()
    {
        UpgradeProgress();
        enableToInteract = upgradeTimeRemaining <= 0f;
    }

    private void UpgradeProgress()
    {
        if (upgradeTimeRemaining > 0f)
        {
            upgradeTimeRemaining -= Time.deltaTime;
            timeSlider.value = upgradeTimeRemaining;

            if (upgradeTimeRemaining <= 0f)
            {
                CompleteUpgrade();
            }
        }
    }

    private void CompleteUpgrade()
    {
        upgradeTimeRemaining = 0f;

        InventoryItem materialItem = GetItemInSlot(materialSlot);
        InventoryItem outputItem = GetItemInSlot(outputSlot);
        InventoryItem inputItem = GetItemInSlot(inputSlot);

        materialItem.count -= MaterialUpgradeSlot.Instance.materialQuantity;
        MaterialUpgradeSlot.Instance.RefreshMaterialsNotUsed(materialSlot);

        SetItemVisibility(outputItem, true);

        Destroy(materialItem.gameObject);
        Destroy(inputItem.gameObject);
    }

    public void OnPointerClick(PointerEventData eventData) => IsUpgrading();

    public bool IsUpgrading()
    {
        if (IsReadyToUpgrade())
        {
            InventoryItem outputItem = GetItemInSlot(outputSlot);
            Tool upgradeTool = outputItem.GetItem<Tool>();

            timeSlider.maxValue = upgradeTool.timeToUpgrade;
            upgradeTimeRemaining = timeSlider.maxValue;

            StartCoroutine(AlphaTransitionCoroutine(outputItem, 0.5f, 1f, upgradeTool.timeToUpgrade));
            return true;
        }
        return false;
    }

    public bool IsReadyToUpgrade()
    {
        InventoryItem materialItem = GetItemInSlot(materialSlot);
        InventoryItem outputItem = GetItemInSlot(outputSlot);
        InventoryItem inputItem = GetItemInSlot(inputSlot);

        upgradeCost = outputItem.GetItem<Tool>().feeToUpgrade;

        bool hasSufficientMaterials = MaterialUpgradeSlot.Instance.reachQuantiy;
        bool hasSufficientTokens = InventoryManager.token >= upgradeCost;
        bool outputSlotNotEmpty = outputItem != null;
        bool inputSlotNotEmpty = inputItem != null;

        return hasSufficientMaterials && hasSufficientTokens && outputSlotNotEmpty && inputSlotNotEmpty;
    }

    private InventoryItem GetItemInSlot(GameObject slot)
    {
        return slot.GetComponentInChildren<InventoryItem>();
    }

    private void SetItemVisibility(InventoryItem item, bool isVisible)
    {
        Color color = item.image.color;
        color.a = isVisible ? 1f : 0.5f;
        item.image.color = color;
    }

    private IEnumerator AlphaTransitionCoroutine(InventoryItem item, float startAlpha, float endAlpha, float duration)
    {
        float elapsedTime = 0f;
        Color originalColor = item.image.color;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float currentAlpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);

            Color newColor = new Color(originalColor.r, originalColor.g, originalColor.b, currentAlpha);
            item.image.color = newColor;

            yield return null; 
        }
        Color finalColor = new Color(originalColor.r, originalColor.g, originalColor.b, endAlpha);
        item.image.color = finalColor;
    }
}
