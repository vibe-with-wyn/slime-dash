using UnityEngine;

// Attach to your Death Zone game object (BoxCollider2D set Is Trigger = true).
// It will detect player entering and call LivesController to handle life decrement + respawn.
[RequireComponent(typeof(Collider2D))]
public class DeathZone : MonoBehaviour
{
    [Tooltip("Player tag (default 'Player').")]
    [SerializeField] private string playerTag = "Player";

    // Prevent multiple triggers in quick succession per player
    private readonly System.Collections.Generic.HashSet<GameObject> processing = new System.Collections.Generic.HashSet<GameObject>();

    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null) return;
        if (!other.CompareTag(playerTag)) return;

        GameObject player = other.gameObject;

        if (processing.Contains(player)) return;
        processing.Add(player);

        var lives = Object.FindFirstObjectByType<LivesController>();
        if (lives != null)
        {
            lives.HandlePlayerDeath(player);
        }
        else
        {
            Debug.LogWarning("DeathZone: No LivesController found in scene. Player will be respawned without life change.");
            // Respawn anyway if respawn manager exists
            if (RespawnManager.Instance != null)
                RespawnManager.Instance.Respawn(player);
        }

        // remove processing lock after a short time so repeated passes are allowed later
        StartCoroutine(ReleaseLock(player, 0.5f));
    }

    private System.Collections.IEnumerator ReleaseLock(GameObject player, float delay)
    {
        yield return new WaitForSeconds(delay);
        processing.Remove(player);
    }
}