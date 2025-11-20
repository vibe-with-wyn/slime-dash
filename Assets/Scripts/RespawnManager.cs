using System.Collections;
using UnityEngine;

// Simple singleton responsible for remembering last checkpoint and teleporting player back.
// It disables PlayerController during the respawn to avoid input/states interfering.
public class RespawnManager : MonoBehaviour
{
    public static RespawnManager Instance { get; private set; }

    [Tooltip("Delay in seconds before respawning the player (allows death animation).")]
    [SerializeField] private float respawnDelay = 0.5f;

    [Header("Predefined checkpoints (optional)")]
    [Tooltip("Optional list of checkpoint transforms you can assign in the Inspector.")]
    [SerializeField] private Transform[] predefinedCheckpoints;
    [Tooltip("Index of initial checkpoint from the list above (applied on Awake if list not empty).")]
    [SerializeField] private int initialCheckpointIndex = 0;

    [Header("Checkpoint acceptance (optional)")]
    [Tooltip("When enabled the manager will only accept a new checkpoint if it's further along the chosen axis.")]
    [SerializeField] private bool requireNewCheckpointToBeFurther = false;
    public enum Axis { X, Y }
    [SerializeField] private Axis compareAxis = Axis.X;
    public enum CompareDirection { Increasing, Decreasing }
    [SerializeField] private CompareDirection compareDirection = CompareDirection.Increasing;

    [Header("Debug")]
    [SerializeField] private bool logCheckpointEvents = false;

    private Vector3 lastCheckpoint;
    private bool hasCheckpoint = false;

    // Expose respawn delay so other systems can wait if necessary
    public float RespawnDelay => respawnDelay;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // If there are predefined checkpoints, use the selected one as initial checkpoint
            if (predefinedCheckpoints != null && predefinedCheckpoints.Length > 0)
            {
                int idx = Mathf.Clamp(initialCheckpointIndex, 0, predefinedCheckpoints.Length - 1);
                if (predefinedCheckpoints[idx] != null)
                {
                    SetCheckpoint(predefinedCheckpoints[idx].position, $"Predefined[{idx}]:{predefinedCheckpoints[idx].name}");
                }
            }
        }
        else Destroy(gameObject);
    }

    // Called by Checkpoint to set respawn position (existing API). Optional source string for clearer logs.
    public void SetCheckpoint(Vector3 worldPosition, string source = null)
    {
        if (requireNewCheckpointToBeFurther && hasCheckpoint)
        {
            float current = GetAxisValue(lastCheckpoint);
            float candidate = GetAxisValue(worldPosition);

            bool accept;
            if (compareDirection == CompareDirection.Increasing)
                accept = candidate > current;
            else
                accept = candidate < current;

            if (!accept)
            {
                if (logCheckpointEvents)
                    Debug.Log($"RespawnManager: Ignored checkpoint at {worldPosition} (current {lastCheckpoint}). Source={source}");
                return;
            }
        }

        lastCheckpoint = worldPosition;
        hasCheckpoint = true;
        if (logCheckpointEvents)
            Debug.Log($"RespawnManager: Checkpoint set to {lastCheckpoint}. Source={source}");
    }

    // New overload: set checkpoint by Transform (useful from Inspector-assigned checkpoint objects)
    public void SetCheckpoint(Transform checkpoint, string source = null)
    {
        if (checkpoint == null) return;
        SetCheckpoint(checkpoint.position, source ?? checkpoint.name);
    }

    // Respawn the given player GameObject. Safe: zeroes velocities and disables PlayerController briefly.
    public void Respawn(GameObject player)
    {
        if (player == null) return;
        StartCoroutine(RespawnCoroutine(player));
    }

    private IEnumerator RespawnCoroutine(GameObject player)
    {
        // disable input / physics side effects
        var playerController = player.GetComponent<PlayerController>();
        var rb = player.GetComponent<Rigidbody2D>();

        if (playerController != null) playerController.enabled = false;
        if (rb != null) rb.linearVelocity = Vector2.zero;

        // small delay so death animation / effects can play
        yield return new WaitForSeconds(respawnDelay);

        // move to checkpoint if present, else stay in place
        if (hasCheckpoint)
        {
            if (logCheckpointEvents)
                Debug.Log($"RespawnManager: Respawning player '{player.name}' to {lastCheckpoint}");
            // Use Rigidbody2D.position when available (safer with physics) and set transform as well.
            if (rb != null)
            {
                rb.position = lastCheckpoint;
                rb.linearVelocity = Vector2.zero;
                // force transform sync
                player.transform.position = lastCheckpoint;
            }
            else
            {
                player.transform.position = lastCheckpoint;
            }
        }
        else
        {
            if (logCheckpointEvents)
                Debug.Log($"RespawnManager: No checkpoint set - player '{player.name}' stays in place");
        }

        // reset physics and re-enable
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
        if (playerController != null) playerController.enabled = true;
    }

    private float GetAxisValue(Vector3 v)
    {
        return compareAxis == Axis.X ? v.x : v.y;
    }

    // Editor/debug helper
    [ContextMenu("Log Current Checkpoint")]
    private void LogCurrentCheckpoint()
    {
        Debug.Log($"RespawnManager: hasCheckpoint={hasCheckpoint} lastCheckpoint={lastCheckpoint}");
    }
}