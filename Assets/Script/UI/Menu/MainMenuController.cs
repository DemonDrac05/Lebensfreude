using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("=== Gameplay Scene ===")]
    [SerializeField] private string gameplaySceneName = "GameplayScene";

    [Header("=== UI Buttons ===")]
    [SerializeField] private Button btnNewGame;
    [SerializeField] private Button btnLoadGame;
    [SerializeField] private Button btnExit;

    private void Start()
    {
        if (btnNewGame != null) btnNewGame.onClick.AddListener(NewGame);
        if (btnLoadGame != null)
        {
            btnLoadGame.onClick.AddListener(LoadGame);
            btnLoadGame.interactable = SaveManager.Instance != null && SaveManager.Instance.HasSave();
        }
        if (btnExit != null) btnExit.onClick.AddListener(ExitGame);
    }

    public void NewGame()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.DeleteSave();
        }

        InventoryManager.token = 15000;

        SceneManager.LoadScene(gameplaySceneName);
    }

    public void LoadGame()
    {
        if (SaveManager.Instance != null && SaveManager.Instance.HasSave())
        {
            SceneManager.sceneLoaded += OnGameplaySceneLoaded;
            SceneManager.LoadScene(gameplaySceneName);
        }
    }

    private void OnGameplaySceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnGameplaySceneLoaded;

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.Load();
        }
    }

    public void ExitGame()
    {
        Debug.Log("[MainMenu] Thoát trò chơi.");
        Application.Quit();
    }
}