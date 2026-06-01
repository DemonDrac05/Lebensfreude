using System;
using UnityEngine;
using TMPro;

// ─────────────────────────────────────────
// SLEEP MANAGER  (overlay đen + chặn input + điều phối ngủ/intro)
// ─────────────────────────────────────────
// Singleton SỐNG QUA SCENE. Sở hữu MÀN ĐEN full-screen + text. Khi ngủ/intro:
//   bật InputBlocker + Time.timeScale = 0 + ForceClose panel đang mở -> chặn TẤT CẢ input,
//   chỉ CLICK CHUỘT để đóng (đã chốt). Có guard bỏ qua frame mở để chống cascade.
//
// THỨ TỰ NGỦ (đã chốt): roll hint -> hiện màn đen + (hint hoặc rỗng) -> ĐỢI click -> hồi stamina -> sang ngày.
// Intro: hiện lore đầu game, chỉ đóng (không hồi stamina / không sang ngày).
//
// Liên kết: Bonfire (Sleep), DreamHintSystem (GetIntroText/RollSleepHint), StaminaManager (RestoreFull),
//           TimeManager (SleepToNextMorning), InputManager (ForceCloseActivePanel), InputBlocker.
public class SleepManager : MonoBehaviour
{
    public static SleepManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
    }

    // ─────────────────────────────────────────
    // CONFIG  (Inspector)
    // ─────────────────────────────────────────
    [Header("=== Overlay đen full-screen ==========")]
    [SerializeField] private CanvasGroup overlay;        // alpha 1 = che màn; nên đặt trên Canvas persistent
    [SerializeField] private TextMeshProUGUI overlayText;
    [SerializeField] private bool showIntroOnStart = true;

    // ─────────────────────────────────────────
    // RUNTIME
    // ─────────────────────────────────────────
    private bool   _busy;          // đang trong overlay
    private bool   _awaitingClick; // đang đợi click để đóng
    private bool   _skipFrame;     // bỏ qua frame mở (chống cascade input khởi động)
    private Action _afterDismiss;  // việc làm sau khi đóng (vd FinishSleep)

    private void Start()
    {
        if (showIntroOnStart) ShowIntro();
    }

    // Click chuột để đóng overlay (chạy cả khi timeScale = 0 vì Update không phụ thuộc timeScale).
    private void Update()
    {
        if (!_awaitingClick) return;
        if (_skipFrame) { _skipFrame = false; return; } // bỏ qua frame vừa mở
        if (Input.GetMouseButtonDown(0)) Dismiss();
    }

    // ─────────────────────────────────────────
    // PUBLIC ENTRY
    // ─────────────────────────────────────────
    // Lore mở màn đầu game (chỉ che màn + đóng, không hồi stamina / không sang ngày).
    public void ShowIntro()
    {
        string lore = DreamHintSystem.Instance != null ? DreamHintSystem.Instance.GetIntroText() : "";
        BeginOverlay(lore, afterDismiss: null);
    }

    // Ngủ tại Bonfire. Dùng trong: Bonfire.Update().
    public void Sleep()
    {
        if (_busy) return;
        string hint = DreamHintSystem.Instance != null 
                        ? DreamHintSystem.Instance.RollSleepHint() 
                        : "Glory awaits another legend";
        BeginOverlay(hint, afterDismiss: FinishSleep);
    }

    // ─────────────────────────────────────────
    // OVERLAY FLOW
    // ─────────────────────────────────────────
    // Bật màn đen + chặn input + pause game. Thiếu overlay -> không che, chạy afterDismiss ngay (không kẹt).
    private void BeginOverlay(string text, Action afterDismiss)
    {
        if (overlay == null)
        {
            Debug.Log($"[Sleep] (no overlay assigned) {text}");
            afterDismiss?.Invoke();
            return;
        }

        _busy = true;
        InputBlocker.IsBlocked = true;
        InputManager.Instance?.ForceCloseActivePanel(); // phòng cùng frame inventory lỡ bật
        Time.timeScale = 0f;

        overlay.alpha = 1f;
        overlay.blocksRaycasts = true;
        if (overlayText != null) overlayText.text = text ?? "";

        _afterDismiss  = afterDismiss;
        _awaitingClick = true;
        _skipFrame     = true; // bỏ qua frame mở
    }

    // Đóng overlay -> trả input + thời gian -> chạy afterDismiss (vd FinishSleep). Dùng trong: Update (click).
    private void Dismiss()
    {
        _awaitingClick = false;
        if (overlay != null)
        {
            overlay.alpha = 0f;
            overlay.blocksRaycasts = false;
        }
        Time.timeScale = 1f;
        InputBlocker.IsBlocked = false;
        _busy = false;

        var cb = _afterDismiss; _afterDismiss = null;
        cb?.Invoke();
    }

    // Sau khi đóng overlay ngủ: hồi stamina đầy rồi sang ngày mới. Dùng trong: callback của Sleep().
    private void FinishSleep()
    {
        StaminaManager.Instance?.RestoreFull();
        TimeManager.Instance?.SleepToNextMorning();
    }
}
