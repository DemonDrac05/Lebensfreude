using UnityEngine;

// ─────────────────────────────────────────
// BONFIRE  (vật thể để ngủ — thay cho giường)
// ─────────────────────────────────────────
// Theo pattern tương tác của main: người chơi tới GẦN (vào trigger) rồi NHẤN PHÍM để ngủ.
// Cần: 1 Collider2D đặt Is Trigger = true bao quanh lửa trại; Player có Collider2D + Rigidbody2D.
//
// Liên kết: SleepManager.Sleep().
public class Bonfire : MonoBehaviour
{
    [Header("=== Phím tương tác ==========")]
    [SerializeField] private KeyCode sleepKey = KeyCode.E;

    private bool _playerInRange;

    // Người chơi bước vào vùng lửa trại. Dùng trong: vật lý trigger.
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponentInParent<Player>() != null) _playerInRange = true;
    }

    // Người chơi rời vùng. Dùng trong: vật lý trigger.
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponentInParent<Player>() != null) _playerInRange = false;
    }

    // Ở gần + nhấn phím -> gọi SleepManager. Dùng trong: vòng lặp game.
    private void Update()
    {
        if (InputBlocker.IsBlocked) return; // đang ngủ/overlay -> không cho trigger lại
        if (_playerInRange && Input.GetKeyDown(sleepKey))
            SleepManager.Instance?.Sleep();
    }
}
