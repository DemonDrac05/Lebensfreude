using System.Collections;
using UnityEngine;

// Unified harvestable surface resource: tree, bush, mushroom, foraged plant.
// Hit by HarvestController (central click raycast); independent of the planting system and OnMouseDown.
// On break it FREES its movement tile and destroys itself, so the dropped item is never blocked.
public class Harvestable : MonoBehaviour
{
    [Header("=== Harvest rules ==========")]
    public bool forageable = false;                       // true = no tool needed (bush, mushroom, flower)
    public ActionType requiredAction = ActionType.Chop;   // used when not forageable (Chop for trees)
    public int hitsToBreak = 1;
    public bool treeFall = false;                         // trees fall over before being removed

    [Header("=== Drop ==========")]
    public BaseItem dropItem;                             // awarded item; its piece prefab is dropItem.gameObj (Product)
    public GameObject pieceOverride;                     // optional explicit collectible piece prefab
    public int dropAmount = 1;

    private int _hits;
    private bool _broken;
    private SpriteRenderer _sr;
    private Collider2D _col;

    private void Awake()
    {
        _hits = Mathf.Max(1, hitsToBreak);
        _sr = GetComponentInChildren<SpriteRenderer>();
        _col = GetComponent<Collider2D>();
        if (_col == null)
        {
            // Add a small trigger collider so the click raycast can find this object.
            var cc = gameObject.AddComponent<CircleCollider2D>();
            cc.isTrigger = true;
            cc.radius = 0.45f;
            _col = cc;
        }
    }

    // canRespawn / respawn are accepted for compatibility but ignored: objects are destroyed on break
    // (per design), which guarantees the dropped item is reachable.
    public void Configure(bool forage, ActionType action, BaseItem item, GameObject piece,
                          int amount, int hits, bool canRespawn, float respawn)
    {
        forageable = forage;
        requiredAction = action;
        dropItem = item;
        pieceOverride = piece;
        dropAmount = Mathf.Max(1, amount);
        hitsToBreak = Mathf.Max(1, hits);
        _hits = hitsToBreak;
        treeFall = !forage; // only trees (non-forageable) fall over; bushes / mushrooms just pop
    }

    // Called by HarvestController. Returns true when a hit lands (so the caller stops).
    public bool TryHit(Tool tool)
    {
        if (_broken || _hits <= 0) return false;
        if (!forageable && (tool == null || tool.actionType != requiredAction)) return false;
        _hits--;
        if (_hits <= 0) Break();
        else ShakeFx.Shake(this, transform);   // hit feedback while still standing
        return true;
    }

    private void Break()
    {
        if (_broken) return;
        _broken = true;

        // Free the movement tile so the dropped item is reachable, and stop any further blocking.
        WorldBlocking.Unblock(WorldBlocking.WorldToCell(transform.position));
        foreach (var c in GetComponentsInChildren<Collider2D>()) c.enabled = false;

        GameObject piece = pieceOverride != null ? pieceOverride : (dropItem is Product p ? p.gameObj : null);
        if (piece != null) ResourceDropper.Drop(piece, dropAmount, transform.position, this);

        if (treeFall) StartCoroutine(FallOver());
        else ShakeFx.Shake(this, transform);

        // Destroy AFTER the drop's slide finishes (~0.6s) so the item's collider re-enables and is collectible.
        Destroy(gameObject, 0.8f);
    }

    private IEnumerator FallOver()
    {
        Transform vis = _sr != null ? _sr.transform : transform;
        Quaternion from = vis.localRotation;
        Quaternion to = from * Quaternion.Euler(0f, 0f, Random.value < 0.5f ? 78f : -78f);
        float dur = 0.4f, e = 0f;
        while (e < dur && vis != null)
        {
            e += Time.deltaTime;
            vis.localRotation = Quaternion.Slerp(from, to, e / dur);
            yield return null;
        }
    }
}
