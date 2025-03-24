using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChestUI : MonoBehaviour
{
    private void OnEnable()
    {
        InputManager.Instance.toolBar.SetActive(false);
        //foreach(var key in InputManager.Instance.keyMappings)
        //{
        //    if (key.UIElement.name == "MainInventory")
        //    {
        //        key.UIElement.SetActive(true);
        //        break;
        //    }
        //}
    }

    private void OnDisable()
    {
        InputManager.Instance.toolBar.SetActive(true);
        //Set main inventory false when chest closes
        foreach (var key in InputManager.Instance.keyMappings)
        {
            if (key.UIElement.name == "MainInventory")
            {
                key.UIElement.SetActive(false);
                break;
            }
        }
    }
}
