using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventoryModifier : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler 
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        ToolUsedManager.Instance.isReadyToUse = false;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ToolUsedManager.Instance.isReadyToUse = true;
    }
}
