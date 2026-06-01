using UnityEngine;
using UnityEngine.SceneManagement;

// ─────────────────────────────────────────
// ENDING MANAGER  (chọn tier theo số ngày + hiện record + lưu lại)
// ─────────────────────────────────────────
// Singleton SỐNG QUA SCENE. LegendaryHall gọi TriggerEnding() khi đủ 3 seal.
// Đọc TimeManager.TotalDays -> tier; hiện màn ending (ngày + tier + coins + làng + câu kết) qua MessageOverlay;
// LƯU record vào PlayerPrefs (mở rộng sau khi bạn làm Menu/Save). Click xong -> về Menu (nếu đã gán scene).
//
// Liên kết: LegendaryHall (TriggerEnding), TimeManager (TotalDays), InventoryManager (token),
//           ArtifactManager (EarnedCount = số làng hồi sinh), MessageOverlay (hiện), SceneManager (về menu).
public class EndingManager : MonoBehaviour
{
    public static EndingManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
    }

    [Header("=== Scene Menu (để trống nếu chưa có) ==========")]
    [SerializeField] private string mainMenuScene = "";

    private bool _ended; // chống trigger 2 lần

    // ─────────────────────────────────────────
    // TRIGGER ENDING
    // ─────────────────────────────────────────
    // Dùng trong: LegendaryHall.AfterLore() khi AllInserted.
    public void TriggerEnding()
    {
        if (_ended) return;
        _ended = true;

        int days     = TimeManager.TotalDays;
        int coins    = InventoryManager.token;
        int villages = ArtifactManager.Instance != null ? ArtifactManager.Instance.EarnedCount : 0;
        (string title, string flavor) = TierFor(days);

        string record =
            $"★  {title}  ★\n\n" +
            $"Days elapsed: {days}\n" +
            $"Coins: {coins}\n" +
            $"Villages revived: {villages}/3\n\n" +
            $"{flavor}";

        SaveRecord(days, coins, villages, title);
        MessageOverlay.Instance?.Show(record, GoToMenu);
    }

    // ─────────────────────────────────────────
    // TIER  (theo Full Design Document, mục 12)
    // ─────────────────────────────────────────
    private (string title, string flavor) TierFor(int days)
    {
        if (days <= 60)  return ("Legendary Merchant",
            "The ancestors appear. The villages send representatives. The Hall fills with light — you are the Merchant God of a new age.");
        if (days <= 100) return ("Master Trader",
            "The villages cheer. The Hall opens fully. Your name is inscribed above the door.");
        if (days <= 150) return ("Skilled Merchant",
            "The Hall opens. The villages are restored. The world is better for your work.");
        return ("Wandering Merchant",
            "Some merchants take the long road. The world still needed you. The Hall opens.");
    }

    // ─────────────────────────────────────────
    // SAVE RECORD  (PlayerPrefs — bản đơn giản, mở rộng sau)
    // ─────────────────────────────────────────
    private void SaveRecord(int days, int coins, int villages, string title)
    {
        PlayerPrefs.SetInt("Record_LastDays", days);
        PlayerPrefs.SetInt("Record_LastCoins", coins);
        PlayerPrefs.SetInt("Record_LastVillages", villages);
        PlayerPrefs.SetString("Record_LastTier", title);

        // Best = số ngày ít nhất (nhanh nhất). 0 = chưa có.
        int best = PlayerPrefs.GetInt("Record_BestDays", 0);
        if (best == 0 || days < best) PlayerPrefs.SetInt("Record_BestDays", days);

        PlayerPrefs.Save();
    }

    // Click xong màn ending -> về Menu (nếu đã gán scene). Dùng trong: callback của MessageOverlay.Show.
    private void GoToMenu()
    {
        Time.timeScale = 1f;
        if (!string.IsNullOrEmpty(mainMenuScene))
            SceneManager.LoadScene(mainMenuScene);
        else
            Debug.Log("[Ending] Hoàn tất. Chưa gán Main Menu scene -> đứng tại chỗ. (Gán mainMenuScene khi có Menu.)");
    }
}
