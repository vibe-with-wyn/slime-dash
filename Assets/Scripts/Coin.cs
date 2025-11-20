using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Attach to coin GameObject (prefab). Requires CircleCollider2D with "Is Trigger" checked.
public class Coin : MonoBehaviour
{
    [Header("Collection")]
    [Tooltip("Tag used to identify the player object")]
    [SerializeField] private string playerTag = "Player";
    [Tooltip("Animator boolean name on the slime to play swallow animation (must be a bool parameter)")]
    [SerializeField] private string swallowTrigger = "Swallow";
    [Tooltip("Seconds to animate the coin shrinking before it is removed")]
    [SerializeField] private float swallowDuration = 0.45f;
    [Tooltip("Amount added to the collected coins counter when picked up (always forced to 1)")]
    [SerializeField] private int coinValue = 1;

    [Header("Optional Audio")]
    [SerializeField] private AudioClip collectSound;
    [SerializeField] private float collectVolume = 1f;

    [Header("UI (optional)")]
    [Tooltip("Direct reference to a UnityEngine.UI.Text that shows the collected coins. Assign one of these instead of using the scene name lookup.")]
    [SerializeField] private Text coinsUIText;
    [Tooltip("Direct reference to a TextMeshProUGUI that shows the collected coins. Assign one of these instead of using the scene name lookup.")]
    [SerializeField] private TextMeshProUGUI coinsTMPText;

    // Prevent double-collection
    private bool isCollected = false;

    private AudioSource audioSource;
    private Collider2D col2d;
    private Vector3 originalScale;

    private const string COINS_PREFS_KEY = "CoinsCollected";
    private const string DEFAULT_UI_NAME = "Coins Collected";

    private Animator collectedAnimatorRef;
    private PlayerController collectedPlayerController;

    private void OnValidate()
    {
        // Ensure coinValue remains 1 in the editor
        if (coinValue != 1)
        {
            coinValue = 1;
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }

    private void Awake()
    {
        // Force coinValue to 1 at runtime as well (defensive)
        coinValue = 1;

        col2d = GetComponent<Collider2D>();
        originalScale = transform.localScale;

        // Ensure collider exists (coins already have CircleCollider2D per your setup)
        if (col2d == null)
            Debug.LogWarning($"{name}: No Collider2D found. Coin will not detect player.");

        // Optional audio source for coin sound
        if (collectSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.clip = collectSound;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isCollected) return;
        if (other == null) return;

        if (!other.CompareTag(playerTag))
            return;

        isCollected = true;

        // Try to get animator from player (child or on the player)
        Animator slimeAnimator = other.GetComponentInChildren<Animator>();
        collectedAnimatorRef = slimeAnimator;

        // store PlayerController so we can block input during swallow
        collectedPlayerController = other.GetComponent<PlayerController>();

        if (collectedPlayerController != null)
        {
            collectedPlayerController.SetInputEnabled(false);
        }

        if (slimeAnimator != null && !string.IsNullOrEmpty(swallowTrigger))
        {
            // Set the boolean parameter (user changed to bool)
            try
            {
                slimeAnimator.SetBool(swallowTrigger, true);
            }
            catch
            {
                // ignore animator parameter issues
            }
        }

        // Disable collider immediately so we don't collect again
        if (col2d != null) col2d.enabled = false;

        // Play optional sound
        if (audioSource != null)
        {
            audioSource.volume = collectVolume;
            audioSource.Play();
        }

        // Start vanish/collect coroutine
        StartCoroutine(SwallowAndDestroy());
    }

    private IEnumerator SwallowAndDestroy()
    {
        float elapsed = 0f;
        Vector3 startScale = transform.localScale;
        Vector3 targetScale = Vector3.zero;

        // Smoothly shrink the coin while swallow animation plays on slime
        while (elapsed < swallowDuration)
        {
            float t = Mathf.Clamp01(elapsed / swallowDuration);
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Finalize scale
        transform.localScale = targetScale;

        // Reset slime swallow bool (if we set it)
        if (collectedAnimatorRef != null && !string.IsNullOrEmpty(swallowTrigger))
        {
            try
            {
                collectedAnimatorRef.SetBool(swallowTrigger, false);
            }
            catch
            {
                // ignore
            }
        }

        // Re-enable player input (if we disabled it)
        if (collectedPlayerController != null)
        {
            collectedPlayerController.SetInputEnabled(true);
        }

        // Update persistent coins counter
        int current = PlayerPrefs.GetInt(COINS_PREFS_KEY, 0);
        current += coinValue; // coinValue is forced to 1
        PlayerPrefs.SetInt(COINS_PREFS_KEY, current);
        PlayerPrefs.Save();

        // Update UI if present (prefer inspector references, fallback to safe scene search)
        UpdateCoinsUI(current);

        // Remove coin object
        Destroy(gameObject);
    }

    private void UpdateCoinsUI(int value)
    {
        // Prefer direct inspector-assigned references when available
        if (coinsUIText != null)
        {
            coinsUIText.text = value.ToString();
            return;
        }

        if (coinsTMPText != null)
        {
            coinsTMPText.text = value.ToString();
            return;
        }

        // Fallback: safe scene search that avoids updating a coin's own child UI
        // 1) Search for UI.Text objects with the expected name that are NOT children of a Coin instance
        var allTexts = Object.FindObjectsByType<Text>(FindObjectsSortMode.None);
        foreach (var t in allTexts)
        {
            if (t.gameObject.name != DEFAULT_UI_NAME) continue;
            // exclude if this Text is a child of any Coin (including this coin)
            if (t.gameObject.GetComponentInParent<Coin>() != null) continue;
            t.text = value.ToString();
            return;
        }

        // 2) Search for TMP objects with the expected name that are NOT children of a Coin instance
        var allTmps = Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);
        foreach (var tm in allTmps)
        {
            if (tm.gameObject.name != DEFAULT_UI_NAME) continue;
            if (tm.gameObject.GetComponentInParent<Coin>() != null) continue;
            tm.text = value.ToString();
            return;
        }
    }
}