using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using UnityEngine.TextCore.Text;

public class ItemToCraft : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("=== Item To Craft ==========")]
    public CraftingItem itemToCraft;

    [Header("=== Information Card Properties ==========")]
    public Sprite frameImage;
    public Sprite backgroundImage;
    public TMP_FontAsset fontAsset;

    private GameObject infoCard;

    private List<IngredientQuantity> quantityList = new List<IngredientQuantity>();

    [System.Serializable]
    public class IngredientQuantity
    {
        public BaseItem ingredient;
        public int currentQuantity;
        public int maxQuantity;
    }

    private void Awake()
    {
        foreach (var material in itemToCraft.materialBox)
        {
            IngredientQuantity newQuantity = new IngredientQuantity
            {
                ingredient = material.material,
                currentQuantity = 0,
                maxQuantity = material.quantity
            };

            quantityList.Add(newQuantity);
        }
    }


    private void Update()
    {
        if (infoCard != null)
        {
            RectTransform rectTransform = infoCard.GetComponent<RectTransform>();
            if (infoCard.activeSelf && rectTransform != null)
            {
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    rectTransform.parent as RectTransform,
                    Input.mousePosition,
                    null,
                    out Vector2 localPoint))
                {
                    rectTransform.localPosition = new(localPoint.x + 20f, localPoint.y - 20f);
                }
            }
        }
    }

    private void RefreshQuantityText(IngredientQuantity currentIndex, TextMeshProUGUI text)
    {
        currentIndex.currentQuantity = 0;
        for (int i = 0; i < InventoryManager.Instance.MainInventorySlots.Length; i++)
        {
            var itemInSlot = InventoryManager.Instance.MainInventorySlots[i].GetComponentInChildren<InventoryItem>();
            if (itemInSlot != null && itemInSlot.GetItem<BaseItem>() == currentIndex.ingredient)
            {
                currentIndex.currentQuantity += itemInSlot.count;
            }
        }
        text.color = currentIndex.currentQuantity >= currentIndex.maxQuantity ? Color.black : Color.red;
        text.text = $"{currentIndex.currentQuantity} / {currentIndex.maxQuantity}";
        text.alignment = TextAlignmentOptions.MidlineLeft;
        text.font = fontAsset;
        text.fontSize = 15f;
    }

    private bool CanCraftItem()
    {
        foreach (var ingredient in quantityList)
        {
            if (ingredient.currentQuantity < ingredient.maxQuantity)
            {
                return false;
            }
        }
        return true;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (CanCraftItem())
        {
            GameObject purchaseItem = Instantiate(InventoryManager.Instance.inventoryPrefab, this.transform);
            var inventoryItem = purchaseItem.GetComponent<InventoryItem>();

            inventoryItem.InitialiseItem(itemToCraft);
            inventoryItem.PickUpItem(inventoryItem);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        CreateInfoCard();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        DestroyImmediate(infoCard);
        infoCard = null;
    }

    private void CreateInfoCard()
    {
        infoCard = new GameObject("InformationCard");
        RectTransform infoCardRect = infoCard.AddComponent<RectTransform>();
        infoCardRect.SetParent(this.transform.parent);

        infoCardRect.sizeDelta = new(100f, 100f);
        infoCardRect.localPosition = new(-50f, 50f);
        infoCardRect.pivot = new(0f, 1f);

        #region [LABEL] NAME ========== - - - ==========
        #region *** MAIN ==============
        GameObject labelNameObj = Instantiate(new GameObject("[Label] Name"), infoCard.transform);
        labelNameObj.AddComponent<Image>().type = Image.Type.Sliced;
        labelNameObj.GetComponent<Image>().sprite = frameImage;

        RectTransform labelNameRect = labelNameObj.GetComponent<RectTransform>();
        labelNameRect.anchoredPosition = new(100f, 0f);
        labelNameRect.sizeDelta = new(400f, 50f);
        labelNameRect.pivot = new(0.5f, 0f);
        #endregion

        #region *** BACKGROUND ========
        GameObject labelNameBackground = Instantiate(new GameObject("Background"), labelNameObj.transform);
        labelNameBackground.AddComponent<Image>().type = Image.Type.Sliced;
        labelNameBackground.GetComponent<Image>().sprite = backgroundImage;

        RectTransform labelNameBackgroundRect = labelNameBackground.GetComponent<RectTransform>();
        labelNameBackgroundRect.anchoredPosition = Vector3.zero;
        labelNameBackgroundRect.sizeDelta = new(380f, 30f);
        #endregion

        #region *** TMP ===============
        GameObject labelNameText = Instantiate(new GameObject("[Item] Name"), labelNameBackground.transform);

        TextMeshProUGUI labelNameTMP = labelNameText.AddComponent<TextMeshProUGUI>();
        labelNameTMP.alignment = TextAlignmentOptions.MidlineLeft;
        labelNameTMP.text = itemToCraft.name;
        labelNameTMP.color = Color.black;
        labelNameTMP.font = fontAsset;
        labelNameTMP.fontSize = 20f;

        RectTransform labelNameTextRect = labelNameText.GetComponent<RectTransform>();
        labelNameTextRect.sizeDelta = new(340f, 20f);
        labelNameTextRect.anchoredPosition = Vector2.zero;
        #endregion
        #endregion

        #region [LABEL] DETAIL ========== - - - ==========
        #region *** MAIN ================
        GameObject labelDetailObj = Instantiate(new GameObject("[Label] Detail"), infoCard.transform);
        labelDetailObj.AddComponent<Image>().type = Image.Type.Sliced;
        labelDetailObj.GetComponent<Image>().sprite = frameImage;

        RectTransform labelDetailObjRect = labelDetailObj.GetComponent<RectTransform>();
        labelDetailObjRect.anchoredPosition = new(100f, 0f);

        float heightOfBoard = 50f; //default 50
        heightOfBoard += itemToCraft.materialBox.Count * 30;

        int spacingLine = itemToCraft.instructionText.Length / 30;
        heightOfBoard += (spacingLine + 1) * 15;

        labelDetailObjRect.sizeDelta = new(400f, heightOfBoard);
        labelDetailObjRect.pivot = new(0.5f, 1f);
        #endregion

        #region *** BACKGROUND =========
        GameObject labelDetailBackground = Instantiate(new GameObject("Background"), labelDetailObj.transform);
        labelDetailBackground.AddComponent<Image>().type = Image.Type.Sliced;
        labelDetailBackground.GetComponent<Image>().sprite = backgroundImage;

        RectTransform labelDetailBackgroundRect = labelDetailBackground.GetComponent<RectTransform>();
        labelDetailBackgroundRect.anchoredPosition = Vector3.zero;
        labelDetailBackgroundRect.sizeDelta = new(380f, heightOfBoard - 20f);
        #endregion

        #region *** TMP ===============
        GameObject introductionField = Instantiate(new GameObject("[Item] Introduction"), labelDetailBackground.transform);

        TextMeshProUGUI introductionTMP = introductionField.AddComponent<TextMeshProUGUI>();
        introductionTMP.alignment = TextAlignmentOptions.MidlineLeft;
        introductionTMP.text = itemToCraft.instructionText;
        introductionTMP.color = Color.black;
        introductionTMP.font = fontAsset;
        introductionTMP.fontSize = 15f;

        RectTransform introductionFieldRect = introductionField.GetComponent<RectTransform>();
        introductionFieldRect.pivot = new(0.5f, 0f);
        introductionFieldRect.sizeDelta = new(340f, (spacingLine + 1) * 15);
        introductionFieldRect.anchoredPosition = new(0f, 15f - (labelDetailBackgroundRect.sizeDelta.y / 2));
        #endregion

        #region *** INGREDIENT ==========
        GameObject ingredientField = Instantiate(new GameObject("[Item] Ingredient"), labelDetailBackground.transform);
        ingredientField.AddComponent<RectTransform>();

        RectTransform ingredientFieldRect = ingredientField.GetComponent<RectTransform>();
        ingredientFieldRect.anchorMin = new(0f, 0f);
        ingredientFieldRect.anchorMax = new(1f, 1f);

        for (int i = 0; i < quantityList.Count; i++)
        {
            var index = quantityList[i];
            GameObject newObjectImage = Instantiate(new GameObject(index.ingredient.name), ingredientField.transform);
            newObjectImage.AddComponent<Image>().sprite = index.ingredient.image;

            RectTransform newObjectImageRect = newObjectImage.GetComponent<RectTransform>();
            newObjectImageRect.pivot = new(0f, 1f);
            newObjectImageRect.sizeDelta = new(15f, 15f);
            newObjectImageRect.anchoredPosition = new(-170f, (labelDetailBackgroundRect.sizeDelta.y / 2) - (45 + (i - 1) * 30));

            GameObject newObjectText = Instantiate(new GameObject($"{index.ingredient.name}Quantity"), ingredientField.transform);
            TextMeshProUGUI newObjectTMP = newObjectText.AddComponent<TextMeshProUGUI>();
            RefreshQuantityText(index, newObjectTMP);

            RectTransform newObjectTextRect = newObjectText.GetComponent<RectTransform>();
            newObjectTextRect.pivot = new(0f, 1f);
            newObjectTextRect.sizeDelta = new(100f, 15f);
            newObjectTextRect.anchoredPosition = new(-140f, (labelDetailBackgroundRect.sizeDelta.y / 2) - (45 + (i - 1) * 30));
        }
        #endregion
        #endregion
    }
}