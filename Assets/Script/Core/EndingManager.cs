using UnityEngine;
using UnityEngine.SceneManagement;

//
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

    [Header("=== Menu scene (leave empty if none yet) ==========")]
    [SerializeField] private string mainMenuScene = "";

    private bool _ended;

    // TRIGGER ENDING
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

    private void SaveRecord(int days, int coins, int villages, string title)
    {
        PlayerPrefs.SetInt("Record_LastDays", days);
        PlayerPrefs.SetInt("Record_LastCoins", coins);
        PlayerPrefs.SetInt("Record_LastVillages", villages);
        PlayerPrefs.SetString("Record_LastTier", title);

        int best = PlayerPrefs.GetInt("Record_BestDays", 0);
        if (best == 0 || days < best) PlayerPrefs.SetInt("Record_BestDays", days);

        PlayerPrefs.Save();
    }

    private void GoToMenu()
    {
        Time.timeScale = 1f;
        if (!string.IsNullOrEmpty(mainMenuScene))
            SceneManager.LoadScene(mainMenuScene);
        else
            Debug.Log("[Ending] Hoàn tất. Chưa gán Main Menu scene -> đứng tại chỗ. (Gán mainMenuScene khi có Menu.)");
    }
}
