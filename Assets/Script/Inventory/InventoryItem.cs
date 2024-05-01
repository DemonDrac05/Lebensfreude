using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("UI")]

    public Image image;
    public Slider slider;
    public Text countText;

    [HideInInspector] public Item item;
    [HideInInspector] public int count = 1;
    [HideInInspector] public Transform parentAfterDrag;

    [HideInInspector] public ToolUsedManager tool;
    private void Awake()
    {
        tool = FindObjectOfType<ToolUsedManager>();
    }
    public void InitialiseItem(Item newItem)
    {
        item = newItem;
        image.sprite = newItem.image;
        RefreshCount();

        if (item.name == "WaterCan")
        {
            tool.waterSlider = slider;
            slider.gameObject.SetActive(true);
        }
        else slider.gameObject.SetActive(false);
    }
    public void RefreshCount()
    {
        countText.text = count.ToString();
        bool textActive = count > 1;
        countText.gameObject.SetActive(textActive);
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        image.raycastTarget = false;
        parentAfterDrag = transform.parent;
        transform.SetParent(transform.root);
    }
    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        image.raycastTarget = true;
        transform.SetParent(parentAfterDrag);
    }
}
