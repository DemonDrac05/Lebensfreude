using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class BackgroundColorManager : MonoBehaviour
{
    [SerializeField] Light2D globalLight2D;
    [SerializeField] Color originalColor;
    [SerializeField] Color targetColor;

    private float elapsedTime = 0f;

    private void Update()
    {
        if (TimeManager.hours - 12 >= 5)
        {
            NightTimeTransfering(300f, originalColor, targetColor);
        }
    }

    //IEnumerator ColorTransferingByTime(float duration, Color orginalColor, Color targetColor)
    //{
    //    float elapsedTime = 0f;
    //    while (elapsedTime < duration)
    //    {
    //        float timer = elapsedTime / duration;
    //        globalLight2D.color = Color.Lerp(orginalColor, targetColor, timer);
    //        globalLight2D.intensity = Mathf.Lerp(1f, 0.5f, timer);

    //        elapsedTime += Time.deltaTime;
    //        yield return null;
    //    }
    //    globalLight2D.color = targetColor;
    //}

    private void NightTimeTransfering(float duration, Color orginalColor, Color targetColor)
    {
        if (elapsedTime < duration)
        {
            float timer = elapsedTime / duration;
            globalLight2D.color = Color.Lerp(orginalColor, targetColor, timer);
            globalLight2D.intensity = Mathf.Lerp(1f, 0.5f, timer);

            elapsedTime += Time.deltaTime;
        }
    }
}
