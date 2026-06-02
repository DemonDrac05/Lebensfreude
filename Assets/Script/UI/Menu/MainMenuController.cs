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
            // Vô hiệu hóa nút Load nếu chưa từng có file lưu
            btnLoadGame.interactable = SaveManager.Instance != null && SaveManager.Instance.HasSave();
        }
        if (btnExit != null) btnExit.onClick.AddListener(ExitGame);
    }

    // Nút 1: Chơi Mới - Xóa file lưu cũ, đặt lại dữ liệu và tải Scene chơi game
    public void NewGame()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.DeleteSave();
        }

        // Reset tiền khởi đầu mặc định
        InventoryManager.token = 15000;

        SceneManager.LoadScene(gameplaySceneName);
    }

    // Nút 2: Tải Game - Tải Scene chơi trước, sau đó kích hoạt hàm nạp dữ liệu cũ
    public void LoadGame()
    {
        if (SaveManager.Instance != null && SaveManager.Instance.HasSave())
        {
            // Lắng nghe sự kiện nạp Scene thành công để kích hoạt nạp File lưu
            SceneManager.sceneLoaded += OnGameplaySceneLoaded;
            SceneManager.LoadScene(gameplaySceneName);
        }
    }

    private void OnGameplaySceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Hủy đăng ký lắng nghe sự kiện ngay lập tức để tránh trùng lặp cho các lần nạp sau
        SceneManager.sceneLoaded -= OnGameplaySceneLoaded;

        // Kích hoạt nạp dữ liệu lưu từ SaveManager
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.Load();
        }
    }

    // Nút 3: Thoát ứng dụng hoàn toàn
    public void ExitGame()
    {
        Debug.Log("[MainMenu] Thoát trò chơi.");
        Application.Quit();
    }
}