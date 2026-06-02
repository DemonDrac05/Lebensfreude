using System.Collections;
using UnityEngine;

// ─────────────────────────────────────────
// MINEABLE DEPOSIT  (deposit quặng/đá — đào ra piece, không nhặt deposit)
// ─────────────────────────────────────────
// Click trái lên collider khi đang chọn tool MINE (Pickaxe) → trừ 1 hit; hết hit → văng piece
// qua ResourceDropper → player đi qua tự lụm (PlayerCollision). Hết thì ẩn + respawn.
//
// Tile registration: Sau khi spawn, OreGemSpawner gọi RegisterWithTileSystem() để
// đăng ký ô vào PlayerController.itemsOnGround. Khi deposit vỡ → huỷ đăng ký (tile tự do).
// Khi respawn → đăng ký lại. Nhờ đó placement engine hiện hitbox ĐỎ trên ô có deposit.
//
// Liên kết: InventoryManager.GetSelectedItem<Tool>, ResourceDropper.Drop,
//           OreGemSpawner.Configure + RegisterWithTileSystem,
//           PlayerController.itemsOnGround (tile occupancy tracking), InputBlocker.
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

    // ─────────────────────────────────────────
    // RUNTIME
    // ─────────────────────────────────────────
    private int            _hits;
    private SpriteRenderer _sr;
    private Collider2D     _col;

    // Tile registration — khớp format PlayerController.itemsOnGround (center.y - 0.5f)
    private Vector3          _tilePos;    // vị trí đăng ký trong itemsOnGround
    private PlayerController _tileCtrl;  // ref tới PlayerController
    private bool             _registered; // đã đăng ký chưa (tránh double-add khi prefab có sẵn data)

    private void Awake()
    {
        _hits = hitsToBreak;
        _sr   = GetComponent<SpriteRenderer>();
        _col  = GetComponent<Collider2D>();
    }

    // ─────────────────────────────────────────
    // CONFIGURE  (OreGemSpawner gán loại + số lượng)
    // ─────────────────────────────────────────
    // Dùng trong: OreGemSpawner.Spawn().
    public void Configure(BaseItem item, int amount)
    {
        dropItem   = item;
        dropAmount = Mathf.Max(1, amount);
    }

    // ─────────────────────────────────────────
    // TILE REGISTRATION  (tích hợp với placement engine)
    // ─────────────────────────────────────────
    // Đăng ký ô tile vào PlayerController.itemsOnGround để:
    //   • Placement engine hiện hitbox ĐỎ khi player trỏ vào ô có deposit.
    //   • Tránh đặt vật đè lên deposit.
    // Công thức tọa độ: (center.x, center.y - 0.5f, 0) — khớp format itemsOnGround
    //   (xem PlayerController.GetHitBoxPrefab: itemOffSet.y = position.y - 0.5f).
    // Dùng trong: OreGemSpawner.Spawn().
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

    // ─────────────────────────────────────────
    // INPUT
    // ─────────────────────────────────────────
    private void OnMouseDown()
    {
        if (InputBlocker.IsBlocked || _hits <= 0) return;
        var tool = InventoryManager.Instance != null
            ? InventoryManager.Instance.GetSelectedItem<Tool>(false)
            : null;
        if (tool == null || tool.actionType != requiredAction) return; // phải đang cầm Pickaxe (Mine)

        _hits--;
        if (_hits <= 0) Break();
    }

    // ─────────────────────────────────────────
    // BREAK  (vỡ mỏ → văng piece + ẩn + respawn)
    // ─────────────────────────────────────────
    private void Break()
    {
        GameObject prefab = pieceOverride != null
            ? pieceOverride
            : (dropItem is Product p ? p.gameObj : null);

        if (prefab != null)
            ResourceDropper.Drop(prefab, dropAmount, transform.position, this);

        // Giải phóng ô chiếm đóng
        if (_registered && _tileCtrl != null)
            _tileCtrl.itemsOnGround.Remove(_tilePos);

        if (_sr  != null) _sr.enabled  = false;
        if (_col != null) _col.enabled = false;

        // Nếu đang ở trong Dungeon, báo hiệu để rải thang đi tiếp (không hồi sinh quặng ở tầng hiện tại)
        if (DungeonManager.Instance != null && DungeonManager.Instance.currentDepth > 0)
        {
            DungeonGenerator.Instance?.OnOreMined(transform.position);
            // Hủy hẳn object khi đã thu hoạch xong trong dungeon
            Destroy(gameObject, 1f); 
        }
        else
        {
            StartCoroutine(Respawn()); // Ở Overworld thì tiến hành hồi sinh bình thường
        }
    }

    // ─────────────────────────────────────────
    // RESPAWN  (hồi sinh → khoá tile lại)
    // ─────────────────────────────────────────
    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(respawnSeconds);

        _hits = hitsToBreak;
        if (_sr  != null) _sr.enabled  = true;
        if (_col != null) _col.enabled = true;

        // Khoá tile trở lại — deposit hiện diện, không cho đặt vật đè
        if (_registered && _tileCtrl != null)
            _tileCtrl.itemsOnGround.Add(_tilePos);
    }
}
