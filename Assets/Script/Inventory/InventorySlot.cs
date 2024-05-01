using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IDropHandler
{
    public GameObject usedBox;
    public Image image;
    public Color selectedColor, originalColor;
    private void Awake()
    {
        DeSelect();
    }
    public void Select()
    {
        image.color = selectedColor;
    }

    public void DeSelect()
    {
        image.color = originalColor;
    }
    public void OnDrop(PointerEventData eventData)
    {
        if(transform.childCount == 0)
        {
            InventoryItem inventoryItem = eventData.pointerDrag.GetComponent<InventoryItem>();
            inventoryItem.parentAfterDrag = transform;
        }
    }
}
