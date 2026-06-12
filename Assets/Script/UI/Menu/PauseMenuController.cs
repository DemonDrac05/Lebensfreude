using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuController : MonoBehaviour
{
    [Header("=== UI Panel ===")]
    [SerializeField] private GameObject pauseMenuPanel;

    [Header("=== Buttons ===")]
    [SerializeField] private Button btnSaveAndExit;
    [SerializeField] private Button btnExitWithoutSave;
    [SerializeField] private Button btnCancel;

    [Header("=== Destination Scene ===")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private bool _isPaused = false;

    private void Start()
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);

        if (btnSaveAndExit != null) btnSaveAndExit.onClick.AddListener(SaveAndExit);
        if (btnExitWithoutSave != null) btnExitWithoutSave.onClick.AddListener(ExitWithoutSaving);
        if (btnCancel != null) btnCancel.onClick.AddListener(ResumeGame);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_isPaused)
            {
                ResumeGame();
            }
            else
            {
                TryOpenPauseMenu();
            }
        }
    }

    private void TryOpenPauseMenu()
    {
        if (InputManager.Instance != null)
        {
            if (!InputManager.Instance.toolBar.activeSelf)
            {
                Debug.Log("[PauseMenu] Không thể mở menu khi đang bận giao dịch hoặc chế tạo.");
                return;
            }
        }

        _isPaused = true;
        Time.timeScale = 0f;
        InputBlocker.IsBlocked = true;

        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
    }

    public void ResumeGame()
    {
        _isPaused = false;
        Time.timeScale = 1f;
        InputBlocker.IsBlocked = false;

        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
    }

    private void SaveAndExit()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.Save();
        }
        else
        {
            Debug.LogError("[PauseMenu] Không tìm thấy SaveManager trong scene - chưa lưu được.");
        }

        ReturnToMenu();
    }

    private void ExitWithoutSaving()
    {
        ReturnToMenu();
    }

    private void ReturnToMenu()
    {
        // Always hand control back to the menu in a clean global state: time running,
        // input unblocked, and no stale load intent so the next New Game starts fresh.
        Time.timeScale = 1f;
        InputBlocker.IsBlocked = false;
        GameSession.IsLoadingSave = false;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
