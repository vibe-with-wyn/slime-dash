using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Button References")]
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button tutorialButton;

    [Header("Scene Settings")]
    [SerializeField] private string levelSelectionSceneName = "Level Selection";
    [SerializeField] private float transitionDelay = 0.3f;

    [Header("Optional: Audio")]
    [SerializeField] private AudioSource buttonClickSound;

    private void Start()
    {
        // Add button listeners
        if (startGameButton != null)
        {
            startGameButton.onClick.AddListener(OnStartGameClicked);
        }
        else
        {
            Debug.LogWarning("Start Game Button is not assigned in the Inspector!");
        }

        if (tutorialButton != null)
        {
            tutorialButton.onClick.AddListener(OnTutorialClicked);
        }
        else
        {
            Debug.LogWarning("Tutorial Button is not assigned in the Inspector!");
        }
    }

    private void OnDestroy()
    {
        // Clean up listeners to prevent memory leaks
        if (startGameButton != null)
        {
            startGameButton.onClick.RemoveListener(OnStartGameClicked);
        }

        if (tutorialButton != null)
        {
            tutorialButton.onClick.RemoveListener(OnTutorialClicked);
        }
    }

    public void OnStartGameClicked()
    {
        Debug.Log("Start Game button clicked! Loading Level Selection scene...");

        // Play button click sound if available
        if (buttonClickSound != null)
        {
            buttonClickSound.Play();
        }

        // Disable buttons to prevent multiple clicks
        if (startGameButton != null)
        {
            startGameButton.interactable = false;
        }

        // Load the Level Selection scene
        StartCoroutine(LoadSceneWithDelay(levelSelectionSceneName, transitionDelay));
    }

    public void OnTutorialClicked()
    {
        Debug.Log("Tutorial button clicked! (Feature coming soon...)");

        // Play button click sound if available
        if (buttonClickSound != null)
        {
            buttonClickSound.Play();
        }

        // TODO: Implement tutorial functionality later
        // For now, just log a message
        Debug.Log("Tutorial feature will be implemented in the future.");
    }

    private System.Collections.IEnumerator LoadSceneWithDelay(string sceneName, float delay)
    {
        // Optional: Add fade out or transition animation here
        yield return new WaitForSeconds(delay);

        // Load the scene
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError("Scene name is empty! Cannot load scene.");
            
            // Re-enable button if load failed
            if (startGameButton != null)
            {
                startGameButton.interactable = true;
            }
        }
    }

    // Alternative: Direct methods you can call from Unity Inspector
    // These can be used instead of the serialized button references
    public void StartGame()
    {
        OnStartGameClicked();
    }

    public void OpenTutorial()
    {
        OnTutorialClicked();
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game button clicked!");
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}