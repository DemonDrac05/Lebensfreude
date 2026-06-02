using System.Collections;
using UnityEngine;

// ─────────────────────────────────────────
// RESOURCE DROPPER  (văng piece ra + slide animation — dùng chung)
// ─────────────────────────────────────────
// Tách cơ chế rớt-đồ kiểu Stardew từ Axe để mining/chặt cây/vứt tay DÙNG CHUNG.
// piecePrefab nên có Collider2D + tag "CollectibleItem" để PlayerCollision tự lụm.
// Dùng trong: MineableDeposit.Break() (và có thể refactor Axe/ItemOutOfInventory dùng sau).
public static class ResourceDropper
{
    // Spawn 'count' piece tại center, văng ra vòng tròn bán kính radius bằng slide.
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
        if (col != null) col.enabled = false;   // tắt lúc đang bay -> tránh lụm sớm
        float e = 0f;
        while (e < dur && t != null)
        {
            e += Time.deltaTime;
            t.position = Vector2.Lerp(start, target, e / dur);
            yield return null;
        }
        if (col != null) col.enabled = true;     // hạ cánh -> bật collider để lụm
    }
}
