using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GradientRevealController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Material revealMaterial;
    public float revealDuration = 0.25f;
    public float hideDuration = 0.25f;
    public Vector2 revealDirection;

    private Material materialInstance;

    private void OnDisable() => materialInstance?.SetFloat("_RevealAmount", 0f);

    private void Start()
    {
        InitializeValue();
        DisableChildRayCast();
    }

    private void InitializeValue()
    {
        materialInstance = new Material(revealMaterial);
        materialInstance.SetVector("_Direction", new Vector4(revealDirection.x, revealDirection.y, 0, 0));
        GetComponent<Image>().material = materialInstance;
    }

    private void DisableChildRayCast()
    {
        var textComponents = GetComponentsInChildren<TMPro.TextMeshProUGUI>();
        foreach (var text in textComponents)
        {
            text.raycastTarget = false;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.pointerEnter == gameObject)
        {
            StartCoroutine(Reveal(materialInstance));
            var text = GetComponentInChildren<TextSlidingAnimation>();
            if (text != null)
            {
                text.AnimateTextIn();
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (eventData.pointerEnter == gameObject) 
        {
            StartCoroutine(Hide(materialInstance));
            var text = GetComponentInChildren<TextSlidingAnimation>();
            if (text != null)
            {
                text.AnimateTextOut();
            }
        }
    }

    private IEnumerator Reveal(Material materialInstance)
    {
        float elapsedTime = 0f;

        while (elapsedTime < revealDuration)
        {
            elapsedTime += Time.deltaTime;
            float revealAmount = Mathf.Clamp01(elapsedTime / revealDuration);
            materialInstance.SetFloat("_RevealAmount", revealAmount);
            yield return null;
        }

        materialInstance.SetFloat("_RevealAmount", 1f);
    }

    private IEnumerator Hide(Material materialInstance)
    {
        float elapsedTime = 0f;

        while (elapsedTime < hideDuration)
        {
            elapsedTime += Time.deltaTime;
            float hideAmount = Mathf.Clamp01(1f - (elapsedTime / hideDuration));
            materialInstance.SetFloat("_RevealAmount", hideAmount);
            yield return null;
        }

        materialInstance.SetFloat("_RevealAmount", 0f);
    }
}
