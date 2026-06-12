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
            // Ask the file directly. The menu has no SaveManager instance, and relying on one
            // meant the Load button stayed greyed out on a fresh launch even with a save present.
            btnLoadGame.interactable = SaveManager.SaveFileExists();
        }
        if (btnExit != null) btnExit.onClick.AddListener(ExitGame);
    }

    public void NewGame()
    {
        // Fresh start: the gameplay scene will seed InventoryManager.initialItems.
        GameSession.IsLoadingSave = false;
        SaveManager.DeleteSaveFile();
        InventoryManager.token = 15000;
        SceneManager.LoadScene(gameplaySceneName);
    }

    public void LoadGame()
    {
        if (!SaveManager.SaveFileExists()) return;
        // Tell the gameplay scene to restore from disk instead of seeding initial items.
        // The SaveManager that loads with that scene performs the actual restore.
        GameSession.IsLoadingSave = true;
        SceneManager.LoadScene(gameplaySceneName);
    }

    public void ExitGame()
    {
        Debug.Log("[MainMenu] Thoát trò chơi.");
        Application.Quit();
    }
}
