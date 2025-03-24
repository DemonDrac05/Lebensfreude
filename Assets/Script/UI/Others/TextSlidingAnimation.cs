using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextSlidingAnimation : MonoBehaviour
{
    public TMP_Text textMeshPro;
    public float animationDuration = 0.5f;
    public float targetDistance = 100f;

    public AnimationDirection direction; 

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup; 
    private Vector2 initialPosition;
    private Vector2 targetPosition;
    private bool isTextVisible = false;

    public enum AnimationDirection
    {
        BottomToTop,
        TopToBottom,
        LeftToRight,
        RightToLeft
    }

    private void Awake() => textMeshPro.rectTransform.localPosition = Vector3.zero;

    private void OnDisable()
    {
        if (canvasGroup != null) canvasGroup.alpha = 0;
        if (rectTransform != null) textMeshPro.rectTransform.localPosition = Vector3.zero;
    }

    private void Start()
    {
        rectTransform = textMeshPro.GetComponent<RectTransform>();
        canvasGroup = textMeshPro.gameObject.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0;

        initialPosition = rectTransform.anchoredPosition;

        targetPosition = direction switch
        {
            AnimationDirection.BottomToTop => new Vector2(initialPosition.x, initialPosition.y + targetDistance),
            AnimationDirection.TopToBottom => new Vector2(initialPosition.x, initialPosition.y - targetDistance),
            AnimationDirection.LeftToRight => new Vector2(initialPosition.x + targetDistance, initialPosition.y),
            AnimationDirection.RightToLeft => new Vector2(initialPosition.x - targetDistance, initialPosition.y),
            _ => initialPosition
        };

        textMeshPro.gameObject.SetActive(true);
    }

    public void AnimateTextIn()
    {
        rectTransform.DOAnchorPos(targetPosition, animationDuration)
            .SetEase(Ease.OutBack)
            .OnUpdate(() =>
            {
                Vector2 currentPos = rectTransform.anchoredPosition;
                float normalizedPosition = direction switch
                {
                    AnimationDirection.BottomToTop => Mathf.InverseLerp(initialPosition.y - targetDistance, initialPosition.y, currentPos.y),
                    AnimationDirection.TopToBottom => Mathf.InverseLerp(initialPosition.y + targetDistance, initialPosition.y, currentPos.y),
                    AnimationDirection.LeftToRight => Mathf.InverseLerp(initialPosition.x - targetDistance, initialPosition.x, currentPos.x),
                    AnimationDirection.RightToLeft => Mathf.InverseLerp(initialPosition.x + targetDistance, initialPosition.x, currentPos.x),
                    _ => 0
                };
                canvasGroup.alpha = normalizedPosition;
            });

        canvasGroup.DOFade(1, animationDuration)
            .SetEase(Ease.Linear);

        isTextVisible = true;
    }

    public void AnimateTextOut()
    {
        rectTransform.DOAnchorPos(initialPosition, animationDuration)
            .SetEase(Ease.InBack)
            .OnUpdate(() =>
            {
                Vector2 currentPos = rectTransform.anchoredPosition;
                float normalizedPosition = direction switch
                {
                    AnimationDirection.BottomToTop => Mathf.InverseLerp(initialPosition.y - targetDistance, initialPosition.y, currentPos.y),
                    AnimationDirection.TopToBottom => Mathf.InverseLerp(initialPosition.y + targetDistance, initialPosition.y, currentPos.y),
                    AnimationDirection.LeftToRight => Mathf.InverseLerp(initialPosition.x - targetDistance, initialPosition.x, currentPos.x),
                    AnimationDirection.RightToLeft => Mathf.InverseLerp(initialPosition.x + targetDistance, initialPosition.x, currentPos.x),
                    _ => 0
                };
                canvasGroup.alpha = normalizedPosition;
            });

        canvasGroup.DOFade(0, animationDuration)
            .SetEase(Ease.Linear);

        isTextVisible = false;
    }
}
