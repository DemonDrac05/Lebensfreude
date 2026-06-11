using System.Collections;
using UnityEngine;

public static class ResourceDropper
{
    public static void Drop(GameObject piecePrefab, int count, Vector3 center,
                            MonoBehaviour host, float radius = 0.6f, float duration = 0.6f)
    {
        if (piecePrefab == null || host == null || count <= 0) return;
        for (int i = 0; i < count; i++)
        {
            var go = Object.Instantiate(piecePrefab, center, Quaternion.identity);
            Vector2 target = (Vector2)center + Random.insideUnitCircle * radius;
            host.StartCoroutine(Slide(go.transform, target, duration));
        }
    }

    private static IEnumerator Slide(Transform t, Vector2 target, float dur)
    {
        if (t == null) yield break;
        Vector2 start = t.position;
        var col = t.GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
        float e = 0f;
        while (e < dur && t != null)
        {
            e += Time.deltaTime;
            t.position = Vector2.Lerp(start, target, e / dur);
            yield return null;
        }
        if (col != null) col.enabled = true;
    }
}
