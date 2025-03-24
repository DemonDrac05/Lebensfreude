using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlacksmithStation : MonoBehaviour
{
    private CanvasGroup canvasGroup;

    private void Start() => canvasGroup = GetComponent<CanvasGroup>();

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            if (canvasGroup.alpha == 0)
            {
                canvasGroup.alpha = 1;
                canvasGroup.interactable = true;
            }
            else
            {
                canvasGroup.alpha = 0;
            }
        }
        if (canvasGroup.alpha == 0 && UpgradeButton.Instance.enableToInteract)
        {
            canvasGroup.interactable = false;
        }
    }
}
