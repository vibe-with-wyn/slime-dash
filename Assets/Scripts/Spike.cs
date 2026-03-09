using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Handles both trigger and collision cases.
[RequireComponent(typeof(Collider2D))]
public class Spike : MonoBehaviour
{
    [Tooltip("Player tag (default 'Player')")]
    [SerializeField] private string playerTag = "Player";

    // small per-player lock to avoid duplicate processing
    private readonly HashSet<GameObject> processing = new HashSet<GameObject>();

    private void Reset()
    {
        // don't force trigger state; spikes often use non-trigger collision
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryHandleCollision(collision.collider);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryHandleCollision(other);
    }

    private void TryHandleCollision(Collider2D other)
    {
        if (other == null) return;
        if (!other.CompareTag(playerTag)) return;

        var player = other.gameObject;
        if (processing.Contains(player)) return;
        processing.Add(player);

        var lives = Object.FindFirstObjectByType<LivesController>();
        if (lives != null)
        {
            lives.HandlePlayerDeath(player);
        }
        else
        {
            // fallback: respawn via RespawnManager (no life change)
            if (RespawnManager.Instance != null)
                RespawnManager.Instance.Respawn(player);
        }

        StartCoroutine(ReleaseLock(player, 0.5f));
    }

    private IEnumerator ReleaseLock(GameObject player, float delay)
    {
        yield return new WaitForSeconds(delay);
        processing.Remove(player);
    }
}
