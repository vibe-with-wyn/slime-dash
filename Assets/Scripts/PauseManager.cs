using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    [Header("Assign in Inspector (no auto UI)")]
    [Tooltip("Root GameObject for the pause modal (enable/disable to show/hide)")]
    [SerializeField] private GameObject pauseUIRoot;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button muteButton;
    [SerializeField] private Button quitButton;

    [Header("Settings")]
    [Tooltip("Scene name to load when Quit is pressed")]
    [SerializeField] private string mainMenuSceneName = "Main Menu";

    private bool isPaused;
    private bool audioMuted;
    private const string AUDIO_MUTED_PREF = "AudioMuted";

    public bool IsPaused => isPaused;

    private void Awake()
    {
        // load persisted mute state
        audioMuted = PlayerPrefs.GetInt(AUDIO_MUTED_PREF, 0) == 1;
        AudioListener.pause = audioMuted;

        // wire buttons if assigned
        if (resumeButton != null) resumeButton.onClick.AddListener(OnResumePressed);
        if (muteButton != null) muteButton.onClick.AddListener(OnMutePressed);
        if (quitButton != null) quitButton.onClick.AddListener(OnQuitPressed);

        UpdateMuteButtonLabel();
        // ensure UI hidden on start
        SetPauseUIActive(false);
    }

    private void OnDestroy()
    {
        if (resumeButton != null) resumeButton.onClick.RemoveListener(OnResumePressed);
        if (muteButton != null) muteButton.onClick.RemoveListener(OnMutePressed);
        if (quitButton != null) quitButton.onClick.RemoveListener(OnQuitPressed);
    }

    public void OnPauseButtonPressed()
    {
        TogglePause();
    }

    public void TogglePause()
    {
        if (isPaused) Resume();
        else Pause();
    }

    public void Pause()
    {
        if (isPaused) return;
        isPaused = true;

        // stop time
        Time.timeScale = 0f;

        // disable player input if present
        var player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            var pc = player.GetComponent<PlayerController>();
            if (pc != null) pc.SetInputEnabled(false);
        }

        SetPauseUIActive(true);
    }

    public void Resume()
    {
        if (!isPaused) return;
        isPaused = false;

        // resume time
        Time.timeScale = 1f;

        // re-enable player input
        var player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            var pc = player.GetComponent<PlayerController>();
            if (pc != null) pc.SetInputEnabled(true);
        }

        SetPauseUIActive(false);
    }

    // Button callbacks
    private void OnResumePressed() => Resume();

    private void OnMutePressed()
    {
        audioMuted = !audioMuted;
        AudioListener.pause = audioMuted;
        PlayerPrefs.SetInt(AUDIO_MUTED_PREF, audioMuted ? 1 : 0);
        PlayerPrefs.Save();
        UpdateMuteButtonLabel();
    }

    private void OnQuitPressed()
    {
        // ensure timescale restored before switching scenes
        Time.timeScale = 1f;
        PlayerPrefs.Save();

        if (!string.IsNullOrEmpty(mainMenuSceneName))
            SceneManager.LoadScene(mainMenuSceneName);
        else
            Debug.LogWarning("PauseManager: mainMenuSceneName is not set.");
    }

    private void UpdateMuteButtonLabel()
    {
        if (muteButton == null) return;
        var txt = muteButton.GetComponentInChildren<Text>();
        if (txt != null) txt.text = audioMuted ? "Unmute" : "Mute";
    }

    private void SetPauseUIActive(bool active)
    {
        if (pauseUIRoot != null)
            pauseUIRoot.SetActive(active);
    }
}