using UnityEngine;

public class DungeonExit : MonoBehaviour
{
    private bool _inRange;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponentInParent<Player>() != null) _inRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponentInParent<Player>() != null) _inRange = false;
    }

    private void Update()
    {
        if (InputBlocker.IsBlocked) return;
        if (_inRange && Input.GetKeyDown(KeyCode.E))
        {
            DungeonManager.Instance?.ExitDungeon();
        }
    }

    private void OnMouseDown()
    {
        if (InputBlocker.IsBlocked) return;
        if (_inRange)
        {
            DungeonManager.Instance?.ExitDungeon();
        }
    }
}