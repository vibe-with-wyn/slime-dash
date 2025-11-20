using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("GameManager");
                instance = go.AddComponent<GameManager>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    private const string LEVEL_UNLOCKED_KEY = "LevelUnlocked_";
    private const string LEVEL_COMPLETED_KEY = "LevelCompleted_";
    private const int TOTAL_LEVELS = 2; // Adjust this as you add more levels

    [Header("Coin Tracking")]
    [SerializeField]
    [Tooltip("When true the coin total will be reset when GameManager.Awake runs.")]
    private bool resetCoinsOnAwake = false;

    [SerializeField]
    [Tooltip("When resetting coins, also delete the PlayerPrefs key.")]
    private bool clearPlayerPrefsWhenReset = true;

    // Coin tracking
    private const string COINS_PREF_KEY = "CoinsCollected";
    private const string DEFAULT_UI_NAME = "Coins Collected";
    private int coinsCollected = 0;
    public int CoinsCollected => coinsCollected;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeLevels();

            if (resetCoinsOnAwake)
            {
                // Clear stored value (optionally clear PlayerPrefs)
                if (clearPlayerPrefsWhenReset)
                    PlayerPrefs.DeleteKey(COINS_PREF_KEY);

                coinsCollected = 0;
                PlayerPrefs.SetInt(COINS_PREF_KEY, coinsCollected);
                PlayerPrefs.Save();
            }
            else
            {
                // Load coins from PlayerPrefs
                coinsCollected = PlayerPrefs.GetInt(COINS_PREF_KEY, 0);
            }

            UpdateCoinsUI(coinsCollected);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void InitializeLevels()
    {
        // Level 1 is always unlocked by default
        if (!PlayerPrefs.HasKey(LEVEL_UNLOCKED_KEY + "1"))
        {
            UnlockLevel(1);
        }
    }

    // Add coins (call from coin pickups)
    public void AddCoin(int amount)
    {
        if (amount <= 0) return;

        coinsCollected += amount;
        PlayerPrefs.SetInt(COINS_PREF_KEY, coinsCollected);
        PlayerPrefs.Save();

        UpdateCoinsUI(coinsCollected);
        Debug.Log($"Coin collected. Total: {coinsCollected}");
    }

    // Reset coin counter (callable from code). If clearPlayerPrefs==true it removes the saved key as well.
    public void ResetCoinsCollected(bool clearPlayerPrefs = true)
    {
        if (clearPlayerPrefs)
            PlayerPrefs.DeleteKey(COINS_PREF_KEY);

        coinsCollected = 0;
        PlayerPrefs.SetInt(COINS_PREF_KEY, coinsCollected);
        PlayerPrefs.Save();

        UpdateCoinsUI(coinsCollected);
        Debug.Log("CoinsCollected reset to 0.");
    }

    // Allow right-click in Inspector to reset while editing
    [ContextMenu("Reset Coins Collected")]
    private void ResetCoinsCollectedContext()
    {
        ResetCoinsCollected(clearPlayerPrefsWhenReset);
    }

    // Update UI display for coins (Text or TextMeshProUGUI named "Coins Collected")
    private void UpdateCoinsUI(int value)
    {
        GameObject uiGO = GameObject.Find(DEFAULT_UI_NAME);
        if (uiGO == null) return;

        // UnityEngine.UI.Text
        var uiText = uiGO.GetComponent<Text>();
        if (uiText != null)
        {
            uiText.text = value.ToString();
            return;
        }

        // TMPro TextMeshProUGUI (avoid hard dependency via reflection)
        var tmp = uiGO.GetComponent("TMPro.TextMeshProUGUI");
        if (tmp != null)
        {
            var prop = tmp.GetType().GetProperty("text");
            if (prop != null) prop.SetValue(tmp, value.ToString(), null);
        }
    }

    // Check if a level is unlocked
    public bool IsLevelUnlocked(int levelNumber)
    {
        return PlayerPrefs.GetInt(LEVEL_UNLOCKED_KEY + levelNumber, 0) == 1;
    }

    // Unlock a specific level
    public void UnlockLevel(int levelNumber)
    {
        PlayerPrefs.SetInt(LEVEL_UNLOCKED_KEY + levelNumber, 1);
        PlayerPrefs.Save();
        Debug.Log("Level " + levelNumber + " unlocked!");
    }

    // Check if a level is completed
    public bool IsLevelCompleted(int levelNumber)
    {
        return PlayerPrefs.GetInt(LEVEL_COMPLETED_KEY + levelNumber, 0) == 1;
    }

    // Mark a level as completed and unlock the next level
    public void CompleteLevel(int levelNumber)
    {
        PlayerPrefs.SetInt(LEVEL_COMPLETED_KEY + levelNumber, 1);

        // Unlock the next level
        int nextLevel = levelNumber + 1;
        if (nextLevel <= TOTAL_LEVELS)
        {
            UnlockLevel(nextLevel);
        }

        PlayerPrefs.Save();
        Debug.Log("Level " + levelNumber + " completed! Next level unlocked.");
    }

    // Clear completion flag for a single level (runtime) and lock the next level
    public void ClearLevelCompletion(int levelNumber)
    {
        // Remove completion flag
        PlayerPrefs.DeleteKey(LEVEL_COMPLETED_KEY + levelNumber);

        // Also lock the level that was unlocked by this completion (next level)
        int nextLevel = levelNumber + 1;
        if (nextLevel <= TOTAL_LEVELS)
        {
            PlayerPrefs.DeleteKey(LEVEL_UNLOCKED_KEY + nextLevel);
            Debug.Log($"Locked level {nextLevel} because completion for level {levelNumber} was cleared.");
        }

        PlayerPrefs.Save();
        Debug.Log($"Cleared completion flag for level {levelNumber}.");
    }

    // Clear completion flags for all levels (useful in editor/testing) and lock subsequent levels
    [ContextMenu("Clear All Level Completions")]
    public void ClearAllLevelCompletions()
    {
        for (int i = 1; i <= TOTAL_LEVELS; i++)
        {
            PlayerPrefs.DeleteKey(LEVEL_COMPLETED_KEY + i);
            // Lock the next level if any
            int next = i + 1;
            if (next <= TOTAL_LEVELS)
            {
                PlayerPrefs.DeleteKey(LEVEL_UNLOCKED_KEY + next);
            }
        }

        PlayerPrefs.Save();

        // Ensure level 1 remains unlocked (convention)
        UnlockLevel(1);

        Debug.Log("Cleared completion flags for all levels and locked subsequent levels (kept level 1 unlocked).");
    }

    // Get the highest unlocked level
    public int GetHighestUnlockedLevel()
    {
        for (int i = TOTAL_LEVELS; i >= 1; i--)
        {
            if (IsLevelUnlocked(i))
            {
                return i;
            }
        }
        return 1; // Default to level 1
    }

    // Reset all progress (useful for testing)
    public void ResetAllProgress()
    {
        for (int i = 1; i <= TOTAL_LEVELS; i++)
        {
            PlayerPrefs.DeleteKey(LEVEL_UNLOCKED_KEY + i);
            PlayerPrefs.DeleteKey(LEVEL_COMPLETED_KEY + i);
        }

        // Also reset coins
        PlayerPrefs.DeleteKey(COINS_PREF_KEY);
        PlayerPrefs.Save();

        // Reinitialize levels and UI
        InitializeLevels();
        coinsCollected = PlayerPrefs.GetInt(COINS_PREF_KEY, 0);
        UpdateCoinsUI(coinsCollected);

        Debug.Log("All progress reset!");
    }

    // Get total number of levels
    public int GetTotalLevels()
    {
        return TOTAL_LEVELS;
    }
}