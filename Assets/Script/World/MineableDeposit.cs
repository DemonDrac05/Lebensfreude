using System.Collections;
using UnityEngine;

// ─────────────────────────────────────────
// MINEABLE DEPOSIT  (deposit quặng/đá — đào ra piece, không nhặt deposit)
// ─────────────────────────────────────────
// Click trái lên collider khi đang chọn tool MINE (Pickaxe) -> trừ 1 hit; hết hit -> văng piece (dropItem.gameObj)
// qua ResourceDropper -> player đi qua tự lụm (PlayerCollision). Hết thì ẩn + respawn theo thời gian.
//
// Liên kết: InventoryManager.GetSelectedItem<Tool>, ResourceDropper.Drop, OreGemSpawner.Configure, InputBlocker.
public class MineableDeposit : MonoBehaviour
{
    [Header("=== Dữ liệu (OreGemSpawner.Configure ghi đè dropItem/amount) ==========")]
    [SerializeField] private BaseItem   dropItem;       // piece nhặt được
    [SerializeField] private GameObject pieceOverride;  // nếu để trống -> dùng dropItem.gameObj (Product)
    [SerializeField] private int        dropAmount = 1;

    [Header("=== Đào ==========")]
    [SerializeField] private ActionType requiredAction = ActionType.Mine;
    [SerializeField] private int   hitsToBreak   = 3;
    [SerializeField] private float respawnSeconds = 120f;

    private int _hits; private SpriteRenderer _sr; private Collider2D _col;

    private void Awake()
    {
        _hits = hitsToBreak;
        _sr = GetComponent<SpriteRenderer>();
        _col = GetComponent<Collider2D>();
    }

    // OreGemSpawner gọi để gán loại + số lượng. Dùng trong: OreGemSpawner.Spawn().
    public void Configure(BaseItem item, int amount)
    {
        dropItem = item;
        dropAmount = Mathf.Max(1, amount);
    }

    private void OnMouseDown()
    {
        if (InputBlocker.IsBlocked || _hits <= 0) return;
        var tool = InventoryManager.Instance != null ? InventoryManager.Instance.GetSelectedItem<Tool>(false) : null;
        if (tool == null || tool.actionType != requiredAction) return; // phải đang cầm Pickaxe (Mine)

        _hits--;
        if (_hits <= 0) Break();
    }

    private void Break()
    {
        GameObject prefab = pieceOverride != null
            ? pieceOverride
            : (dropItem is Product p ? p.gameObj : null); // piece = gameObj của item (như Axe dùng product.gameObj)

        if (prefab != null) ResourceDropper.Drop(prefab, dropAmount, transform.position, this);

        if (_sr  != null) _sr.enabled  = false;
        if (_col != null) _col.enabled = false;
        StartCoroutine(Respawn());
    }

    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(respawnSeconds);
        _hits = hitsToBreak;
        if (_sr  != null) _sr.enabled  = true;
        if (_col != null) _col.enabled = true;
    }
}
