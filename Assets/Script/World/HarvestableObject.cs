using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class HarvestableObject : MonoBehaviour
{
    public enum ObjectType { Tree, Bush, Flower, Mushroom, Rock }

    [Header("=== Phân loại Object ===")]
    public ObjectType objectType = ObjectType.Tree;

    [Header("=== Yêu cầu công cụ ===")]
    [SerializeField] private ActionType requiredAction = ActionType.Chop; // Rìu (Chop), Cúp (Mine) hoặc Tay không/Khác
    [SerializeField] private int hitsToBreak = 3;

    [Header("=== Vật phẩm rơi ra ===")]
    [SerializeField] private BaseItem dropItem;
    [SerializeField] private int dropCount = 1;

    [Header("=== Hiệu ứng rung lắc (Độ nảy hạt pixel) ===")]
    [SerializeField] private float shakeAmount = 0.1f;
    [SerializeField] private float shakeDuration = 0.15f;

    private int _currentHits;
    private bool _isDestroyed = false;
    private Vector3 _originalSpritePos;
    private SpriteRenderer _sr;
    private Collider2D _col;

    // Quản lý vị trí khóa ô ảo
    private Vector3 _registeredTilePos;
    private PlayerController _pc;
    private bool _hasRegisteredTile = false;

    private void Awake()
    {
        _currentHits = hitsToBreak;
        _sr = GetComponentInChildren<SpriteRenderer>();
        _col = GetComponent<Collider2D>();
        if (_sr != null) _originalSpritePos = _sr.transform.localPosition;
    }

    private void Start()
    {
        _pc = FindObjectOfType<PlayerController>();
        RegisterTileObstacle();
    }

    // Tự động khóa ô đất ngăn không cho đặt nội thất chồng lên tài nguyên tự nhiên
    private void RegisterTileObstacle()
    {
        if (_pc == null) return;
        _registeredTilePos = new Vector3(transform.position.x, transform.position.y - 0.5f, 0f);
        _pc.itemsOnGround.Add(_registeredTilePos);
        _hasRegisteredTile = true;
    }

    private void UnregisterTileObstacle()
    {
        if (_hasRegisteredTile && _pc != null)
        {
            _pc.itemsOnGround.Remove(_registeredTilePos);
            _hasRegisteredTile = false;
        }

        // Giải phóng ô chặn đường đi của nhân vật
        Vector3Int cell = WorldBlocking.WorldToCell(transform.position);
        WorldBlocking.Unblock(cell);
    }

    private void OnMouseDown()
    {
        if (InputBlocker.IsBlocked || _isDestroyed) return;

        // Kiểm tra loại dụng cụ đang cầm trên thanh Toolbar
        Tool tool = InventoryManager.Instance != null 
            ? InventoryManager.Instance.GetSelectedItem<Tool>(false) 
            : null;

        // Nếu công việc cần công cụ (Chop/Mine), kiểm tra điều kiện cầm đúng đồ
        if (requiredAction == ActionType.Chop || requiredAction == ActionType.Mine)
        {
            if (tool == null || tool.actionType != requiredAction) return;
        }

        // Tiêu hao Stamina của người chơi
        if (StaminaManager.Instance != null)
        {
            if (!StaminaManager.Instance.CanUseTool) return; // Kiệt sức không thể chặt đào
            StaminaManager.Instance.Drain(5f); // Tiêu hao 5 thể lực cho mỗi lần chặt/đào
        }

        _currentHits--;
        
        if (_currentHits > 0)
        {
            StartCoroutine(WobbleEffect());
        }
        else
        {
            BreakObject();
        }
    }

    private IEnumerator WobbleEffect()
    {
        if (_sr == null) yield break;
        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;
            float offsetX = Random.Range(-shakeAmount, shakeAmount);
            _sr.transform.localPosition = new Vector3(_originalSpritePos.x + offsetX, _originalSpritePos.y, _originalSpritePos.z);
            yield return null;
        }
        _sr.transform.localPosition = _originalSpritePos;
    }

    private void BreakObject()
    {
        _isDestroyed = true;
        UnregisterTileObstacle();

        if (_col != null) _col.enabled = false;

        if (objectType == ObjectType.Tree)
        {
            // Chạy hoạt ảnh cây đổ bên dưới nếu là cây thân gỗ lớn
            StartCoroutine(TreeFallDownEffect());
        }
        else
        {
            SpawnDrops();
            Destroy(gameObject);
        }
    }

    private IEnumerator TreeFallDownEffect()
    {
        if (_sr == null)
        {
            SpawnDrops();
            Destroy(gameObject);
            yield break;
        }

        // Hiệu ứng cây đổ nghiêng 90 độ đơn giản bằng Code
        float elapsed = 0f;
        float duration = 0.8f;
        Quaternion startRotation = _sr.transform.rotation;
        
        // Ngã về bên phải hoặc bên trái ngẫu nhiên
        float direction = Random.Range(0, 2) == 0 ? 1f : -1f; 
        Quaternion endRotation = Quaternion.Euler(0, 0, direction * 90f);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            _sr.transform.rotation = Quaternion.Slerp(startRotation, endRotation, elapsed / duration);
            yield return null;
        }

        SpawnDrops();
        Destroy(gameObject);
    }

    private void SpawnDrops()
    {
        if (dropItem == null) return;

        GameObject dropPrefab = null;
        if (dropItem is Product product)
        {
            dropPrefab = product.gameObj;
        }
        else if (dropItem is Plant plant)
        {
            dropPrefab = plant.gameObj;
        }

        if (dropPrefab != null)
        {
            ResourceDropper.Drop(dropPrefab, dropCount, transform.position, this);
        }
    }
}