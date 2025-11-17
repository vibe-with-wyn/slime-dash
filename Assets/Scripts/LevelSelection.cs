using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSelection : MonoBehaviour
{
    [Header("Level Button References")]
    [SerializeField] private LevelButton[] levelButtons;

    [Header("Scene Settings")]
    [SerializeField] private string gameSceneName = "Game Environment";
    [SerializeField] private string mainMenuSceneName = "Main Menu";

    [Header("UI References")]
    [SerializeField] private Button backButton;

    [Header("Optional: Audio")]
    [SerializeField] private AudioSource buttonClickSound;
    [SerializeField] private AudioSource lockedSound;

    private void Start()
    {
        InitializeLevelButtons();
        SetupBackButton();
    }

    private void InitializeLevelButtons()
    {
        // Find all LevelButton components if not assigned
        if (levelButtons == null || levelButtons.Length == 0)
        {
            levelButtons = FindObjectsOfType<LevelButton>();
        }

        // Initialize each level button
        foreach (LevelButton button in levelButtons)
        {
            if (button != null)
            {
                button.Initialize(this);
            }
        }

        UpdateLevelButtons();
    }

    private void UpdateLevelButtons()
    {
        foreach (LevelButton button in levelButtons)
        {
            if (button != null)
            {
                int levelNumber = button.GetLevelNumber();
                bool isUnlocked = GameManager.Instance.IsLevelUnlocked(levelNumber);
                bool isCompleted = GameManager.Instance.IsLevelCompleted(levelNumber);

                button.SetLockState(!isUnlocked);
                button.SetCompletedState(isCompleted);
            }
        }
    }

    public void OnLevelButtonClicked(int levelNumber)
    {
        // Check if level is unlocked
        if (!GameManager.Instance.IsLevelUnlocked(levelNumber))
        {
            Debug.Log("Level " + levelNumber + " is locked!");
            
            // Play locked sound
            if (lockedSound != null)
            {
                lockedSound.Play();
            }
            
            return;
        }

        Debug.Log("Loading Level " + levelNumber + "...");

        // Play button click sound
        if (buttonClickSound != null)
        {
            buttonClickSound.Play();
        }

        // Store the selected level number
        PlayerPrefs.SetInt("SelectedLevel", levelNumber);
        PlayerPrefs.Save();

        // Load the game scene
        LoadGameScene();
    }

    private void LoadGameScene()
    {
        if (!string.IsNullOrEmpty(gameSceneName))
        {
            SceneManager.LoadScene(gameSceneName);
        }
        else
        {
            Debug.LogError("Game scene name is not set!");
        }
    }

    public void OnBackButtonClicked()
    {
        Debug.Log("Returning to Main Menu...");

        // Play button click sound
        if (buttonClickSound != null)
        {
            buttonClickSound.Play();
        }

        // Load main menu scene
        if (!string.IsNullOrEmpty(mainMenuSceneName))
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
        else
        {
            Debug.LogError("Main Menu scene name is not set!");
        }
    }

    private void SetupBackButton()
    {
        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackButtonClicked);
        }
    }

    private void OnDestroy()
    {
        // Clean up listeners
        if (backButton != null)
        {
            backButton.onClick.RemoveListener(OnBackButtonClicked);
        }
    }

    // Public method to call from Unity Inspector
    public void BackToMainMenu()
    {
        OnBackButtonClicked();
    }

    // Debug method to unlock all levels (for testing)
    [ContextMenu("Unlock All Levels")]
    public void UnlockAllLevels()
    {
        for (int i = 1; i <= GameManager.Instance.GetTotalLevels(); i++)
        {
            GameManager.Instance.UnlockLevel(i);
        }
        UpdateLevelButtons();
    }

    // Debug method to reset progress (for testing)
    [ContextMenu("Reset All Progress")]
    public void ResetProgress()
    {
        GameManager.Instance.ResetAllProgress();
        UpdateLevelButtons();
    }
}