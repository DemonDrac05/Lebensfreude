using System.Collections;
using UnityEngine;

//
//
//           OreGemSpawner.Configure + RegisterWithTileSystem,
//           PlayerController.itemsOnGround (tile occupancy tracking), InputBlocker.
public class MineableDeposit : MonoBehaviour
{
    [Header("=== Data (OreGemSpawner.Configure overrides dropItem/amount) ==========")]
    [SerializeField] private BaseItem   dropItem;
    [SerializeField] private GameObject pieceOverride;
    [SerializeField] private int        dropAmount = 1;

    [Header("=== Mining ==========")]
    [SerializeField] private ActionType requiredAction = ActionType.Mine;
    [SerializeField] private int   hitsToBreak   = 3;
    [SerializeField] private float respawnSeconds = 120f;

    // RUNTIME
    private int            _hits;
    private SpriteRenderer _sr;
    private Collider2D     _col;

    private Vector3          _tilePos;
    private PlayerController _tileCtrl;
    private bool             _registered;

    private void Awake()
    {
        _hits = hitsToBreak;
        _sr   = GetComponent<SpriteRenderer>();
        _col  = GetComponent<Collider2D>();
    }

    public void Configure(BaseItem item, int amount)
    {
        dropItem   = item;
        dropAmount = Mathf.Max(1, amount);
    }

    //   (xem PlayerController.GetHitBoxPrefab: itemOffSet.y = position.y - 0.5f).
    public void RegisterWithTileSystem(Vector3 center, PlayerController pc)
    {
        if (pc == null) return;
        _tileCtrl  = pc;
        _tilePos   = new Vector3(center.x, center.y - 0.5f, 0f);

        if (!_registered)
        {
            pc.itemsOnGround.Add(_tilePos);
            _registered = true;
        }
    }

    // INPUT
    // Called by HarvestController (central click raycast) instead of OnMouseDown, so a UI
    // raycast or missing event setup can never silently block mining. Returns true on a hit.
    public bool TryMine(Tool tool)
    {
        if (InputBlocker.IsBlocked || _hits <= 0) return false;
        if (tool == null || tool.actionType != requiredAction) return false;
        _hits--;
        ShakeFx.Shake(this, transform);   // hit feedback (also on the breaking hit)
        if (_hits <= 0) Break();
        return true;
    }

    private void Break()
    {
        GameObject prefab = pieceOverride != null
            ? pieceOverride
            : (dropItem is Product p ? p.gameObj : null);

        if (prefab != null)
            ResourceDropper.Drop(prefab, dropAmount, transform.position, this);

        if (_registered && _tileCtrl != null)
            _tileCtrl.itemsOnGround.Remove(_tilePos);

        if (_sr  != null) _sr.enabled  = false;
        if (_col != null) _col.enabled = false;

        if (DungeonManager.Instance != null && DungeonManager.Instance.currentDepth > 0)
        {
            DungeonGenerator.Instance?.OnOreMined(transform.position);
            Destroy(gameObject, 1f); 
        }
        else
        {
            StartCoroutine(Respawn());
        }
    }

    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(respawnSeconds);

        _hits = hitsToBreak;
        if (_sr  != null) _sr.enabled  = true;
        if (_col != null) _col.enabled = true;

        if (_registered && _tileCtrl != null)
            _tileCtrl.itemsOnGround.Add(_tilePos);
    }
}
