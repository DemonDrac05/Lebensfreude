using System;
using UnityEngine;
using TMPro;

//
//
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

    // CONFIG  (Inspector)
    [Header("=== Full-screen black overlay ==========")]
    [SerializeField] private CanvasGroup overlay;
    [SerializeField] private TextMeshProUGUI overlayText;
    [SerializeField] private bool showIntroOnStart = true;

    // RUNTIME
    private bool   _busy;
    private bool   _awaitingClick;
    private bool   _skipFrame;
    private Action _afterDismiss;

    private void Start()
    {
        if (showIntroOnStart) ShowIntro();
    }

    private void Update()
    {
        if (!_awaitingClick) return;
        if (_skipFrame) { _skipFrame = false; return; }
        if (Input.GetMouseButtonDown(0)) Dismiss();
    }

    // PUBLIC ENTRY
    public void ShowIntro()
    {
        string lore = DreamHintSystem.Instance != null ? DreamHintSystem.Instance.GetIntroText() : "";
        BeginOverlay(lore, afterDismiss: null);
    }

    public void Sleep()
    {
        if (_busy) return;
        string hint = DreamHintSystem.Instance != null 
                        ? DreamHintSystem.Instance.RollSleepHint() 
                        : "Glory awaits another legend";
        BeginOverlay(hint, afterDismiss: FinishSleep);
    }

    // OVERLAY FLOW
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
        InputManager.Instance?.ForceCloseActivePanel();
        Time.timeScale = 0f;

        overlay.alpha = 1f;
        overlay.blocksRaycasts = true;
        if (overlayText != null) overlayText.text = text ?? "";

        _afterDismiss  = afterDismiss;
        _awaitingClick = true;
        _skipFrame     = true;
    }

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

    private void FinishSleep()
    {
        StaminaManager.Instance?.RestoreFull();
        TimeManager.Instance?.SleepToNextMorning();
    }
}
