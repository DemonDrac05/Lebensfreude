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

        Time.timeScale = 1f;
        InputBlocker.IsBlocked = false;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void ExitWithoutSaving()
    {
        Time.timeScale = 1f;
        InputBlocker.IsBlocked = false;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}