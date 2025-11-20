using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float jumpForce = 25f;

    [Header("Ground Check (tag-only, offset)")]
    [Tooltip("Local offset from the player used to position the ground check (X = horizontal, Y = vertical).")]
    [SerializeField] private Vector2 groundCheckOffset = new Vector2(0f, -0.5f);
    [Tooltip("Half-width of the horizontal detection area.")]
    [SerializeField] private float groundCheckHalfWidth = 0.3f;
    [Tooltip("Height of the detection area (small value).")]
    [SerializeField] private float groundCheckHeight = 0.08f;
    [SerializeField] private string groundTag = "Ground";
    [Tooltip("When enabled the ground check will draw debug shapes and log hits.")]
    [SerializeField] private bool debugGroundCheck = false;

    [Header("Animator")]
    [SerializeField] private Animator slimeAnimator;
    [Tooltip("Animator bool name to indicate movement.")]
    [SerializeField] private string moveBoolName = "IsMoving";

    private Rigidbody2D rb;
    private bool moveLeftPressed;
    private bool moveRightPressed;
    private bool isFacingRight = true;

    // Ground state
    private bool isGrounded;
    private bool isJumping;

    // Input control
    private bool inputEnabled = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (slimeAnimator == null)
            slimeAnimator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        // initial grounding attempt in case player starts on ground
        ForceGroundingCheck();
    }

    private void Update()
    {
        // Movement animator bool
        bool isMoving = moveLeftPressed || moveRightPressed;
        if (slimeAnimator != null && !string.IsNullOrEmpty(moveBoolName))
        {
            slimeAnimator.SetBool(moveBoolName, isMoving);
        }

        // Update grounded/jumping animator booleans
        if (slimeAnimator != null)
        {
            slimeAnimator.SetBool("IsGrounded", isGrounded);
            slimeAnimator.SetBool("IsJumping", isJumping);
        }

        // flip sprite if needed based on direction
        if (isMoving)
        {
            if (moveRightPressed && !isFacingRight)
                Flip();
            else if (moveLeftPressed && isFacingRight)
                Flip();
        }
    }

    private void FixedUpdate()
    {
        float x = 0f;
        if (inputEnabled)
        {
            if (moveLeftPressed) x = -1f;
            if (moveRightPressed) x = 1f;
        }
        else
        {
            // ensure no horizontal movement while input disabled
            x = 0f;
        }

        Vector2 vel = rb.linearVelocity;
        vel.x = x * moveSpeed;
        rb.linearVelocity = vel;
    }

    // Ground check using an overlap box (more reliable for Tilemap/CompositeCollider).
    // This is used as a fallback check; primary grounded state is updated by collision callbacks.
    private bool IsGroundedOverlapBox()
    {
        Vector2 center = (Vector2)transform.position + groundCheckOffset;
        Vector2 size = new Vector2(groundCheckHalfWidth * 2f, groundCheckHeight);

        // Optional debug draw (wire box)
        if (debugGroundCheck)
        {
            Vector3 topLeft = center + new Vector2(-size.x / 2f, size.y / 2f);
            Vector3 topRight = center + new Vector2(size.x / 2f, size.y / 2f);
            Vector3 bottomLeft = center + new Vector2(-size.x / 2f, -size.y / 2f);
            Vector3 bottomRight = center + new Vector2(size.x / 2f, -size.y / 2f);
            Debug.DrawLine(topLeft, topRight, Color.yellow, 0.1f);
            Debug.DrawLine(topRight, bottomRight, Color.yellow, 0.1f);
            Debug.DrawLine(bottomRight, bottomLeft, Color.yellow, 0.1f);
            Debug.DrawLine(bottomLeft, topLeft, Color.yellow, 0.1f);
        }

        Collider2D[] hits = Physics2D.OverlapBoxAll(center, size, 0f);
        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D c = hits[i];
            if (c == null) continue;
            if (c.gameObject == gameObject) continue;
            if (debugGroundCheck)
                Debug.Log($"GroundCheck hit: {c.gameObject.name} tag={c.gameObject.tag}");
            if (c.CompareTag(groundTag))
                return true;
        }

        return false;
    }

    private void ForceGroundingCheck()
    {
        // Use overlap box to set initial grounded state
        isGrounded = IsGroundedOverlapBox();
        if (isGrounded)
        {
            isJumping = false;
            if (debugGroundCheck)
                Debug.Log("ForceGroundingCheck: grounded = true");
        }
        else
        {
            if (debugGroundCheck)
                Debug.Log("ForceGroundingCheck: grounded = false");
        }
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 s = transform.localScale;
        s.x *= -1f;
        transform.localScale = s;
    }

    // Methods meant to be connected to UI (MobileButton) events or called from UI Buttons
    public void MoveLeftPress()
    {
        if (!inputEnabled) return;
        moveLeftPressed = true;
    }
    public void MoveLeftRelease()
    {
        if (!inputEnabled) { moveLeftPressed = false; return; }
        moveLeftPressed = false;
    }
    public void MoveRightPress()
    {
        if (!inputEnabled) return;
        moveRightPressed = true;
    }
    public void MoveRightRelease()
    {
        if (!inputEnabled) { moveRightPressed = false; return; }
        moveRightPressed = false;
    }

    // Jump called on button down. Uses boolean IsJumping (no trigger). Matches reference logic:
    // - Only allow jump when grounded
    // - Set isJumping true and clear isGrounded to prevent double jump
    public void JumpPress()
    {
        if (!inputEnabled) return;

        // check collision-maintained grounded state first, fallback to overlap box
        if (isGrounded || IsGroundedOverlapBox())
        {
            Vector2 v = rb.linearVelocity;
            v.y = jumpForce;
            rb.linearVelocity = v;

            isJumping = true;
            isGrounded = false; // immediately mark jumping to avoid double jumps

            // set animator boolean (Update also synchronizes)
            if (slimeAnimator != null)
                slimeAnimator.SetBool("IsJumping", true);

            if (debugGroundCheck)
                Debug.Log("Jump executed (grounded).");
        }
        else
        {
            if (debugGroundCheck)
                Debug.Log("Jump blocked (not grounded).");
        }
    }

    // Collision callbacks update grounded state reliably when colliding with physics colliders.
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(groundTag))
        {
            // consider contact normals to make sure we landed on top (optional)
            foreach (ContactPoint2D cp in collision.contacts)
            {
                if (cp.normal.y > 0.5f)
                {
                    isGrounded = true;
                    isJumping = false;
                    if (slimeAnimator != null)
                    {
                        // animator booleans are updated in Update()
                    }
                    if (debugGroundCheck)
                        Debug.Log($"OnCollisionEnter2D: grounded by {collision.gameObject.name}");
                    break;
                }
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(groundTag))
        {
            // Exit may be triggered by side contacts; perform a quick overlap check to ensure fully off ground
            if (!IsGroundedOverlapBox())
            {
                isGrounded = false;
                if (debugGroundCheck)
                    Debug.Log($"OnCollisionExit2D: left ground from {collision.gameObject.name}");
            }
        }
    }

    // Optional: expose as inspector buttons for quick debug
    public void StopAllMovement()
    {
        moveLeftPressed = moveRightPressed = false;
        if (slimeAnimator != null && !string.IsNullOrEmpty(moveBoolName))
            slimeAnimator.SetBool(moveBoolName, false);
    }

    // Public API to enable/disable player input (used by RespawnManager/Coin)
    public void SetInputEnabled(bool enabled)
    {
        inputEnabled = enabled;
        if (!inputEnabled)
            StopAllMovement();
    }

    // Public helper to restore the animator to an idle-like pose after respawn/die.
    // Optionally pass the exact idle state name to Play if you prefer a direct crossfade.
    public void ResetAnimatorToIdle(string idleStateName = null)
    {
        if (slimeAnimator == null) slimeAnimator = GetComponentInChildren<Animator>();
        if (slimeAnimator == null) return;

        // Reset common parameters used by this project
        try { slimeAnimator.ResetTrigger("Die"); } catch { }
        if (!string.IsNullOrEmpty(moveBoolName))
        {
            try { slimeAnimator.SetBool(moveBoolName, false); } catch { }
        }
        try { slimeAnimator.SetBool("IsJumping", false); } catch { }
        try { slimeAnimator.SetBool("IsGrounded", true); } catch { }

        // If you want to force a specific state, provide idleStateName in the call.
        if (!string.IsNullOrEmpty(idleStateName))
        {
            try { slimeAnimator.Play(idleStateName); } catch { }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Show the ground check position and the detection box using the offset.
        Vector2 center = (Vector2)transform.position + groundCheckOffset;
        Gizmos.color = Color.yellow;

        // Draw short cross at center so you can align X and Y
        float crossSize = 0.05f;
        Gizmos.DrawLine(center + Vector2.up * crossSize, center - Vector2.up * crossSize);
        Gizmos.DrawLine(center + Vector2.right * crossSize, center - Vector2.right * crossSize);

        // Draw box representing the detection area
        Vector3 size = new Vector3(groundCheckHalfWidth * 2f, groundCheckHeight, 0f);
        Gizmos.DrawWireCube(center, size);
    }
}