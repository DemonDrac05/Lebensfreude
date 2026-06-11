using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

//
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

    // CONFIG  (Inspector)
    [SerializeField] private CanvasGroup overlay;
    [SerializeField] private TextMeshProUGUI overlayText;

    // RUNTIME
    private readonly Queue<string> _queue = new();
    private Action _onAllDone;
    private bool _awaitingClick;
    private bool _skipFrame;

    private void Update()
    {
        if (!_awaitingClick) return;
        if (_skipFrame) { _skipFrame = false; return; }
        if (Input.GetMouseButtonDown(0)) Advance();
    }

    // PUBLIC API
    public void Show(string message, Action onDone = null) => ShowSequence(new[] { message }, onDone);

    public void ShowSequence(IEnumerable<string> messages, Action onAllDone = null)
    {
        foreach (var m in messages) _queue.Enqueue(m ?? "");
        _onAllDone = onAllDone;

        if (overlay == null)
        {
            foreach (var m in _queue) Debug.Log($"[Overlay] {m}");
            _queue.Clear();
            var cb = _onAllDone; _onAllDone = null;
            cb?.Invoke();
            return;
        }

        if (!_awaitingClick) Begin();
    }

    // FLOW
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
        _skipFrame = true;
    }

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
