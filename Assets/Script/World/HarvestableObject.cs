using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class HarvestableObject : MonoBehaviour
{
    public enum ObjectType { Tree, Bush, Flower, Mushroom, Rock }

    [Header("=== Object classification ===")]
    public ObjectType objectType = ObjectType.Tree;

    [Header("=== Tool requirement ===")]
    [SerializeField] private ActionType requiredAction = ActionType.Chop;
    [SerializeField] private int hitsToBreak = 3;

    [Header("=== Dropped item ===")]
    [SerializeField] private BaseItem dropItem;
    [SerializeField] private int dropCount = 1;

    [Header("=== Shake effect (pixel piece bounce) ===")]
    [SerializeField] private float shakeAmount = 0.1f;
    [SerializeField] private float shakeDuration = 0.15f;

    private int _currentHits;
    private bool _isDestroyed = false;
    private Vector3 _originalSpritePos;
    private SpriteRenderer _sr;
    private Collider2D _col;

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

        Vector3Int cell = WorldBlocking.WorldToCell(transform.position);
        WorldBlocking.Unblock(cell);
    }

    private void OnMouseDown()
    {
        if (InputBlocker.IsBlocked || _isDestroyed) return;

        Debug.Log("On Mouse Down");

        Tool tool = InventoryManager.Instance != null 
            ? InventoryManager.Instance.GetSelectedItem<Tool>(false) 
            : null;

        Debug.Log(tool);

        if (requiredAction == ActionType.Chop || requiredAction == ActionType.Mine)
        {
            if (tool == null || tool.actionType != requiredAction) return;
        }

        Debug.Log("Got axe");

        if (StaminaManager.Instance != null)
        {
            if (!StaminaManager.Instance.CanUseTool) return;
            StaminaManager.Instance.Drain(5f);
        }

        Debug.Log("Stamina Manager found");

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

        float elapsed = 0f;
        float duration = 0.8f;
        Quaternion startRotation = _sr.transform.rotation;
        
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