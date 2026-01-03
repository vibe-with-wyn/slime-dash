using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Manages life UI images (hearts). Attach to the Lives parent GameObject and assign the heart Images in order.
public class LivesController : MonoBehaviour
{
    [Header("Lives UI")]
    [Tooltip("UI Images for hearts in order (left => right). They will be disabled as lives are lost.")]
    [SerializeField] private Image[] heartImages;

    [Tooltip("Alternative: drag the heart GameObjects (each must have an Image). Used when heartImages is not assigned.")]
    [SerializeField] private GameObject[] heartObjects;

    [Tooltip("Name of scene to load when lives reach zero. Leave empty to just log Game Over.")]
    [SerializeField] private string gameOverSceneName = "Level Selection";

    [Tooltip("Optional delay before showing Game Over scene (seconds).")]
    [SerializeField] private float gameOverDelay = 1f;

    [Tooltip("Animator trigger name to play on player when they die.")]
    [SerializeField] private string dieAnimatorTrigger = "Die";

    [Header("Game Over Panel (optional)")]
    [Tooltip("If assigned, this panel will be shown when lives reach zero instead of loading the Game Over scene.")]
    [SerializeField] private GameObject gameOverPanel;
    [Tooltip("Retry button inside the gameOverPanel. If assigned, will reload the current scene when pressed.")]
    [SerializeField] private Button retryButton;

    private int maxLives;
    private int currentLives;

    // Per-player lock to avoid double-processing collisions
    private HashSet<GameObject> processing = new HashSet<GameObject>();

    private void Awake()
    {
        if (heartImages == null || heartImages.Length == 0)
        {
            if (heartObjects != null && heartObjects.Length > 0)
            {
                heartImages = heartObjects.Select(go =>
                {
                    if (go == null) return null;
                    var img = go.GetComponent<Image>();
                    if (img == null)
                        Debug.LogWarning($"LivesController: heart GameObject '{go.name}' has no Image component.");
                    return img;
                }).Where(i => i != null).ToArray();
            }
            else
            {
                heartImages = GetComponentsInChildren<Image>(true)
                    .Where(i => i.gameObject != this.gameObject)
                    .ToArray();
            }
        }

        maxLives = Mathf.Max(heartImages != null ? heartImages.Length : 0, 0);
        currentLives = maxLives;
        RefreshHearts();

        if (retryButton != null)
            retryButton.onClick.AddListener(OnRetryPressed);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (retryButton != null)
            retryButton.onClick.RemoveListener(OnRetryPressed);
    }

    // Call this when the player dies (e.g. enters Spike or DeathZone)
    public void HandlePlayerDeath(GameObject player)
    {
        if (player == null) return;
        if (processing.Contains(player)) return;

        processing.Add(player);

        if (currentLives <= 0)
        {
            processing.Remove(player);
            return;
        }

        currentLives = Mathf.Max(0, currentLives - 1);
        RefreshHearts();

        // Play die animation and block input
        var animator = player.GetComponentInChildren<Animator>();
        if (animator != null && !string.IsNullOrEmpty(dieAnimatorTrigger))
        {
            try { animator.SetTrigger(dieAnimatorTrigger); } catch { }
        }

        var pc = player.GetComponent<PlayerController>();
        if (pc != null)
        {
            pc.SetInputEnabled(false);
        }

        if (currentLives > 0)
        {
            // Respawn player via RespawnManager (RespawnManager handles the delay/teleport)
            if (RespawnManager.Instance != null)
            {
                RespawnManager.Instance.Respawn(player);
                // release lock after respawn delay + small buffer
                StartCoroutine(ReleaseLockAfterDelay(player, RespawnManager.Instance.RespawnDelay + 0.2f));
            }
            else
            {
                // If no RespawnManager, re-enable input and release lock immediately
                if (pc != null) pc.SetInputEnabled(true);
                processing.Remove(player);
            }
        }
        else
        {
            // Game Over path
            StartCoroutine(HandleGameOver(player));
        }
    }

    private IEnumerator ReleaseLockAfterDelay(GameObject player, float delay)
    {
        yield return new WaitForSeconds(delay);
        processing.Remove(player);
        // If RespawnManager re-enabled the component, no need to re-enable input here.
        // If player component still exists and is disabled, attempt to re-enable input flag.
        var pc = player.GetComponent<PlayerController>();
        if (pc != null && pc.enabled)
        {
            // ensure input is allowed after respawn (RespawnManager re-enables component)
            pc.SetInputEnabled(true);
            // restore animator to idle-like state
            pc.ResetAnimatorToIdle();
        }
    }

    private IEnumerator HandleGameOver(GameObject player)
    {
        Debug.Log("Game Over - no lives remaining.");
        // Optionally disable player controls
        if (player != null)
        {
            var pc = player.GetComponent<PlayerController>();
            if (pc != null) pc.SetInputEnabled(false);
        }

        // Small delay so player can see last state / die animation
        yield return new WaitForSeconds(gameOverDelay);

        // If a panel is assigned, show it and do NOT load the Level Selection scene.
        if (gameOverPanel != null)
        {
            // Show panel and pause time so player can choose Retry
            gameOverPanel.SetActive(true);
            Time.timeScale = 0f;
        }
        else
        {
            if (!string.IsNullOrEmpty(gameOverSceneName))
                SceneManager.LoadScene(gameOverSceneName);
        }

        // release lock (scene may change)
        if (player != null) processing.Remove(player);
    }

    private void RefreshHearts()
    {
        if (heartImages == null) return;
        for (int i = 0; i < heartImages.Length; i++)
        {
            if (heartImages[i] == null) continue;
            heartImages[i].enabled = i < currentLives;
        }
    }

    // Retry button handler: reload current scene
    private void OnRetryPressed()
    {
        // restore timescale in case it was paused
        Time.timeScale = 1f;

        // Optionally hide the panel before reloading
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        // Reset lives in this controller (optional if scene reloads; kept for safety)
        ResetLives();

        // Reload the current active scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Helper to reset lives (useful in tests or level start)
    public void ResetLives()
    {
        currentLives = maxLives;
        RefreshHearts();
    }
}