using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameLaunch : MonoBehaviour
{
    [Header("Slime References")]
    [SerializeField] private Transform slimeTransform;
    [SerializeField] private Animator slimeAnimator;

    [Header("Movement Settings")]
    [SerializeField] private Vector3 startPosition;
    [SerializeField] private Vector3 jumpStartPosition;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float movementDuration = 2f;

    [Header("Jump Settings")]
    [SerializeField] private Vector3 spikePosition;
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float jumpDuration = 1f;

    [Header("Death Settings")]
    [SerializeField] private float deathDelay = 1.5f;

    [Header("Scene Transition")]
    [SerializeField] private string nextSceneName = "MainMenu";
    [SerializeField] private float sceneTransitionDelay = 2f;

    [Header("Animation Parameters")]
    [SerializeField] private string moveAnimationTrigger = "Move";
    [SerializeField] private string jumpAnimationTrigger = "Jump";
    [SerializeField] private string dieAnimationTrigger = "Die";

    [Header("Gizmo Settings")]
    [SerializeField] private Color startPositionColor = Color.green;
    [SerializeField] private Color jumpStartPositionColor = Color.yellow;
    [SerializeField] private Color spikePositionColor = Color.red;
    [SerializeField] private float gizmoSphereRadius = 0.3f;

    private enum SlimeState
    {
        Moving,
        Jumping,
        Dying,
        Complete
    }

    private SlimeState currentState = SlimeState.Moving;

    void Start()
    {
        // Initialize slime position
        if (slimeTransform != null)
        {
            slimeTransform.position = startPosition;
        }

        // Start the sequence
        StartCoroutine(GameLaunchSequence());
    }

    private IEnumerator GameLaunchSequence()
    {
        // Wait a brief moment before starting
        yield return new WaitForSeconds(0.5f);

        // Step 1: Move to jump position
        yield return StartCoroutine(MoveSlime());

        // Step 2: Jump to spike
        yield return StartCoroutine(JumpToSpike());

        // Step 3: Die animation
        yield return StartCoroutine(DieOnSpike());

        // Step 4: Transition to next scene
        yield return new WaitForSeconds(sceneTransitionDelay);
        LoadNextScene();
    }

    private IEnumerator MoveSlime()
    {
        currentState = SlimeState.Moving;

        // Trigger move animation
        if (slimeAnimator != null)
        {
            slimeAnimator.SetTrigger(moveAnimationTrigger);
        }

        float elapsedTime = 0f;

        while (elapsedTime < movementDuration)
        {
            if (slimeTransform != null)
            {
                slimeTransform.position = Vector3.Lerp(startPosition, jumpStartPosition, elapsedTime / movementDuration);
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure final position
        if (slimeTransform != null)
        {
            slimeTransform.position = jumpStartPosition;
        }
    }

    private IEnumerator JumpToSpike()
    {
        currentState = SlimeState.Jumping;

        // Trigger jump animation
        if (slimeAnimator != null)
        {
            slimeAnimator.SetTrigger(jumpAnimationTrigger);
        }

        float elapsedTime = 0f;
        Vector3 startPos = jumpStartPosition;

        while (elapsedTime < jumpDuration)
        {
            float t = elapsedTime / jumpDuration;

            // Calculate horizontal movement
            Vector3 currentPos = Vector3.Lerp(startPos, spikePosition, t);

            // Add parabolic arc for jump
            float heightOffset = jumpHeight * Mathf.Sin(Mathf.PI * t);
            currentPos.y += heightOffset;

            if (slimeTransform != null)
            {
                slimeTransform.position = currentPos;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure final position at spike
        if (slimeTransform != null)
        {
            slimeTransform.position = spikePosition;
        }
    }

    private IEnumerator DieOnSpike()
    {
        currentState = SlimeState.Dying;

        // Wait a moment before triggering death
        yield return new WaitForSeconds(deathDelay);

        // Trigger die animation
        if (slimeAnimator != null)
        {
            slimeAnimator.SetTrigger(dieAnimationTrigger);
        }

        currentState = SlimeState.Complete;
    }

    private void LoadNextScene()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogWarning("Next scene name is not set!");
        }
    }

    private void OnDrawGizmos()
    {
        // Draw start position
        Gizmos.color = startPositionColor;
        Gizmos.DrawSphere(startPosition, gizmoSphereRadius);
        Gizmos.DrawWireSphere(startPosition, gizmoSphereRadius * 1.5f);

        // Draw jump start position
        Gizmos.color = jumpStartPositionColor;
        Gizmos.DrawSphere(jumpStartPosition, gizmoSphereRadius);
        Gizmos.DrawWireSphere(jumpStartPosition, gizmoSphereRadius * 1.5f);

        // Draw spike position
        Gizmos.color = spikePositionColor;
        Gizmos.DrawSphere(spikePosition, gizmoSphereRadius);
        Gizmos.DrawWireSphere(spikePosition, gizmoSphereRadius * 1.5f);

        // Draw movement path
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(startPosition, jumpStartPosition);

        // Draw jump arc
        Gizmos.color = Color.magenta;
        Vector3 previousPoint = jumpStartPosition;
        int arcSegments = 20;

        for (int i = 1; i <= arcSegments; i++)
        {
            float t = i / (float)arcSegments;
            Vector3 point = Vector3.Lerp(jumpStartPosition, spikePosition, t);
            point.y += jumpHeight * Mathf.Sin(Mathf.PI * t);

            Gizmos.DrawLine(previousPoint, point);
            previousPoint = point;
        }

        // Draw labels (only visible in Scene view)
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(startPosition + Vector3.up * 0.5f, "Start");
        UnityEditor.Handles.Label(jumpStartPosition + Vector3.up * 0.5f, "Jump Start");
        UnityEditor.Handles.Label(spikePosition + Vector3.up * 0.5f, "Spike (Death)");
        #endif
    }
}
