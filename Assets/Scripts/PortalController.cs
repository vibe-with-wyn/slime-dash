using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class PortalController : MonoBehaviour
{
    [Header("Portal Settings")]
    [Tooltip("Player tag to detect")]
    [SerializeField] private string playerTag = "Player";
    [Tooltip("Level number to mark completed when player enters portal")]
    [SerializeField] private int levelNumber = 1;
    [Tooltip("Scene to load after completion (fallback).")]
    [SerializeField] private string completionSceneName = "Level Selection";

    [Header("Suction")]
    [Tooltip("Seconds the suction/move-to-center takes")]
    [SerializeField] private float suctionDuration = 1.0f;
    [Tooltip("Optional: shrink player while sucked")]
    [SerializeField] private bool shrinkDuringSuction = true;
    [Tooltip("Optional audio played when suction starts")]
    [SerializeField] private AudioSource portalSound;

    [Header("Animation")]
    [Tooltip("Animator on the portal (optional) - will set 'Activate' trigger if present")]
    [SerializeField] private Animator portalAnimator;
    [Tooltip("Name of trigger parameter on portal animator")]
    [SerializeField] private string activateTriggerName = "Activate";

    private bool activated = false;

    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col != null)
            col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (activated) return;
        if (other == null) return;
        if (!other.CompareTag(playerTag)) return;

        // Start the sequence — mark activated so it won't re-run
        activated = true;

        // Optional animator trigger
        if (portalAnimator != null && !string.IsNullOrEmpty(activateTriggerName))
        {
            try { portalAnimator.SetTrigger(activateTriggerName); } catch { }
        }

        if (portalSound != null) portalSound.Play();

        StartCoroutine(SuckAndCompleteRoutine(other.gameObject));
    }

    private IEnumerator SuckAndCompleteRoutine(GameObject player)
    {
        if (player == null)
        {
            FinishImmediately();
            yield break;
        }

        // Try to disable player input
        var pc = player.GetComponent<PlayerController>();
        if (pc != null) pc.SetInputEnabled(false);

        // Try to stop physics movement
        var rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            // make kinematic so we can move transform safely
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        Vector3 startPos = player.transform.position;
        Vector3 targetPos = transform.position;
        Vector3 startScale = player.transform.localScale;
        Vector3 targetScale = shrinkDuringSuction ? Vector3.zero : startScale;

        float elapsed = 0f;
        while (elapsed < suctionDuration)
        {
            float t = Mathf.Clamp01(elapsed / suctionDuration);
            // smooth step for nicer motion
            float s = Mathf.SmoothStep(0f, 1f, t);

            player.transform.position = Vector3.Lerp(startPos, targetPos, s);
            if (shrinkDuringSuction)
                player.transform.localScale = Vector3.Lerp(startScale, targetScale, s);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // ensure final position/scale
        player.transform.position = targetPos;
        if (shrinkDuringSuction) player.transform.localScale = targetScale;

        // mark level completed in GameManager (if present)
        try
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.CompleteLevel(levelNumber);
            }
        }
        catch
        {
            Debug.LogWarning("PortalController: Failed to call GameManager.Instance.CompleteLevel.");
        }

        // short delay to let portal animation/sound finish
        yield return new WaitForSeconds(0.25f);

        // load completion scene (if assigned)
        if (!string.IsNullOrEmpty(completionSceneName))
        {
            // restore time scale and load
            Time.timeScale = 1f;
            SceneManager.LoadScene(completionSceneName);
        }
        else
        {
            FinishImmediately();
        }
    }

    private void FinishImmediately()
    {
        // if no scene to load, just keep portal active and do nothing else.
        // You may want to show a victory UI here instead.
    }
}