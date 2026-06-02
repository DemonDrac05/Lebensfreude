using UnityEngine;

public class DungeonEntrance : MonoBehaviour
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
            DungeonManager.Instance?.EnterDungeon();
        }
    }

    private void OnMouseDown()
    {
        if (InputBlocker.IsBlocked) return;
        if (_inRange)
        {
            DungeonManager.Instance?.EnterDungeon();
        }
    }
}