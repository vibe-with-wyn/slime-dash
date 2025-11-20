using UnityEngine;

// Attach to checkpoint GameObjects (give them a small trigger collider).
// When the player touches a checkpoint it becomes the new respawn point.
[RequireComponent(typeof(Collider2D))]
public class Checkpoint : MonoBehaviour
{
    [Tooltip("Optional offset to apply when respawning the player")]
    [SerializeField] private Vector3 respawnOffset = Vector3.zero;

    private void Reset()
    {
        // ensure trigger by default
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (RespawnManager.Instance != null)
            RespawnManager.Instance.SetCheckpoint(transform.position + respawnOffset);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.25f);
        Gizmos.color = new Color(0, 1, 1, 0.1f);
        Gizmos.DrawSphere(transform.position, 0.25f);
    }
}