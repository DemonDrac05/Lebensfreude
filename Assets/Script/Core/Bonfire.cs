using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Bonfire : MonoBehaviour
{
    private bool _playerInRange;
    private Collider2D _col;

    private void Awake()
    {
        _col = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponentInParent<Player>() != null) _playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponentInParent<Player>() != null) _playerInRange = false;
    }

    private void Update()
    {
        if (InputBlocker.IsBlocked) return; 

        if (_playerInRange && Input.GetMouseButtonDown(1))
        {
            if (IsMouseOverBonfire())
            {
                SleepManager.Instance?.Sleep();
            }
        }
    }

    private bool IsMouseOverBonfire()
    {
        if (_col == null || !_col.enabled) return false;
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;
        return _col.OverlapPoint(mouseWorld);
    }
}