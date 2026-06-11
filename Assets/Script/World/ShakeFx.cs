using System.Collections;
using UnityEngine;

// Tiny reusable hit feedback: briefly jitter a transform, then restore it.
public static class ShakeFx
{
    public static void Shake(MonoBehaviour host, Transform target, float duration = 0.14f, float magnitude = 0.07f)
    {
        if (host == null || target == null || !host.isActiveAndEnabled) return;
        host.StartCoroutine(Run(target, duration, magnitude));
    }

    private static IEnumerator Run(Transform t, float dur, float mag)
    {
        if (t == null) yield break;
        Vector3 orig = t.localPosition;
        float e = 0f;
        while (e < dur && t != null)
        {
            e += Time.deltaTime;
            Vector2 off = Random.insideUnitCircle * mag;
            t.localPosition = orig + new Vector3(off.x, off.y, 0f);
            yield return null;
        }
        if (t != null) t.localPosition = orig;
    }
}
