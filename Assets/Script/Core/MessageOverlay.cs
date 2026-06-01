using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// ─────────────────────────────────────────
// MESSAGE OVERLAY  (màn đen + text + click để qua)  — dùng cho Hall lore & Ending
// ─────────────────────────────────────────
// Singleton SỐNG QUA SCENE. Cơ chế GIỐNG overlay khi ngủ (chặn input + pause + click đóng),
// nhưng TÁCH RIÊNG để không đụng SleepManager (theo lựa chọn đã chốt).
// Hỗ trợ xếp HÀNG nhiều message liên tiếp (mỗi click qua 1 cái), xong hết thì gọi onAllDone.
//
// Liên kết: LegendaryHall (hiện lore khi cắm artifact), EndingManager (màn ending -> về menu).
public class MessageOverlay : MonoBehaviour
{
    public static MessageOverlay Instance { get; private set; }

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
    [SerializeField] private CanvasGroup overlay;          // màn đen full-screen (Canvas persistent)
    [SerializeField] private TextMeshProUGUI overlayText;

    // ─────────────────────────────────────────
    // RUNTIME
    // ─────────────────────────────────────────
    private readonly Queue<string> _queue = new();
    private Action _onAllDone;
    private bool _awaitingClick;
    private bool _skipFrame;

    // Click chuột để qua message kế (chạy cả khi timeScale = 0). Dùng trong: vòng lặp.
    private void Update()
    {
        if (!_awaitingClick) return;
        if (_skipFrame) { _skipFrame = false; return; }
        if (Input.GetMouseButtonDown(0)) Advance();
    }

    // ─────────────────────────────────────────
    // PUBLIC API
    // ─────────────────────────────────────────
    // Hiện 1 message. Dùng trong: LegendaryHall (1 dòng lore mỗi lần cắm).
    public void Show(string message, Action onDone = null) => ShowSequence(new[] { message }, onDone);

    // Hiện chuỗi message liên tiếp; xong hết -> onAllDone. Dùng trong: EndingManager.
    public void ShowSequence(IEnumerable<string> messages, Action onAllDone = null)
    {
        foreach (var m in messages) _queue.Enqueue(m ?? "");
        _onAllDone = onAllDone;

        if (overlay == null)
        {
            // Không có overlay -> log rồi chạy callback ngay (không kẹt).
            foreach (var m in _queue) Debug.Log($"[Overlay] {m}");
            _queue.Clear();
            var cb = _onAllDone; _onAllDone = null;
            cb?.Invoke();
            return;
        }

        if (!_awaitingClick) Begin();
    }

    // ─────────────────────────────────────────
    // FLOW
    // ─────────────────────────────────────────
    private void Begin()
    {
        InputBlocker.IsBlocked = true;
        Time.timeScale = 0f;
        InputManager.Instance?.ForceCloseActivePanel();
        overlay.alpha = 1f;
        overlay.blocksRaycasts = true;
        ShowNext();
    }

    private void ShowNext()
    {
        if (overlayText != null) overlayText.text = _queue.Dequeue();
        _awaitingClick = true;
        _skipFrame = true; // bỏ qua frame mở/đổi
    }

    // Sang message kế; hết hàng -> đóng overlay + trả input + gọi onAllDone. Dùng trong: Update (click).
    private void Advance()
    {
        if (_queue.Count > 0) { ShowNext(); return; }

        _awaitingClick = false;
        if (overlay != null) { overlay.alpha = 0f; overlay.blocksRaycasts = false; }
        Time.timeScale = 1f;
        InputBlocker.IsBlocked = false;

        var cb = _onAllDone; _onAllDone = null;
        cb?.Invoke();
    }
}
