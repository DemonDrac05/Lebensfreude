using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class CircularLayoutGroup : MonoBehaviour
{
    public float radius;
    public float startAngle;
    public float spacingAngle;

    public Dictionary<RectTransform, Vector3> originalPositions = new Dictionary<RectTransform, Vector3>();

    private void OnEnable()
    {
        StoreOriginalPositions();
        ArrangeElements();
    }

    private void OnDisable()
    {
        foreach (var item in originalPositions)
        {
            if (item.Key != null)
            {
                item.Key.localPosition = Vector3.zero;
            }
        }
    }

    private void StoreOriginalPositions()
    {
        originalPositions.Clear();

        foreach (RectTransform child in transform)
        {
            originalPositions[child] = child.localPosition;
        }
    }

    private void ArrangeElements()
    {
        int childCount = transform.childCount;

        for (int i = 0; i < childCount; i++)
        {
            RectTransform child = transform.GetChild(i) as RectTransform;

            if (child != null)
            {
                if (originalPositions.ContainsKey(child))
                {
                    child.localPosition = originalPositions[child];
                }

                float angle = startAngle + i * spacingAngle;
                float posX = Mathf.Sin(angle * Mathf.Deg2Rad);
                float posY = Mathf.Cos(angle * Mathf.Deg2Rad);
                Vector2 targetPosition = new Vector2(posX, posY) * radius;

                StartCoroutine(SlidingEffect(child, targetPosition, 0.25f));
            }
        }
    }

    public IEnumerator SlidingEffect(RectTransform transform, Vector2 targetPosition, float duration)
    {
        float elapsedTime = 0f;

        Vector3 startingPosition = transform.localPosition;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            transform.localPosition = Vector2.Lerp(startingPosition, targetPosition, elapsedTime / duration);

            yield return null;
        }
    }
}
