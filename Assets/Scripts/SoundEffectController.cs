using UnityEngine;

// Plays SFX for slime actions (swallow, jump, die, movement).
// Attach this to the player (slime) root GameObject. Configure AudioClips in the inspector.
// Designed to detect animator boolean parameters and/or animator states so you do NOT need to change existing gameplay code.
[RequireComponent(typeof(AudioSource))]
public class SoundEffectController : MonoBehaviour
{
    [Header("Animator (auto-assigned if null)")]
    [SerializeField] private Animator animator;

    [Header("Parameter / State Names (match your Animator)")]
    [Tooltip("Boolean parameter or Animator state name for swallow. Prefer the bool name if your code sets it.")]
    [SerializeField] private string swallowBoolName = "Swallow";
    [SerializeField] private string swallowStateName = "Swallow";

    [Tooltip("Boolean parameter or Animator state name for jump.")]
    [SerializeField] private string jumpBoolName = "IsJumping";
    [SerializeField] private string jumpStateName = "Jump";

    [Tooltip("Animator state name for die.")]
    [SerializeField] private string dieStateName = "Die";

    [Tooltip("Boolean parameter name used for continuous movement (plays looping clip).")]
    [SerializeField] private string moveBoolName = "IsMoving";
    [SerializeField] private string moveStateName = "Move";

    [Header("Audio Clips")]
    [SerializeField] private AudioClip swallowClip;
    [SerializeField] private float swallowVolume = 1f;

    [SerializeField] private AudioClip jumpClip;
    [SerializeField] private float jumpVolume = 1f;

    [SerializeField] private AudioClip dieClip;
    [SerializeField] private float dieVolume = 1f;

    [Header("Movement (looping)")]
    [Tooltip("Looping clip for movement. Assigned to a dedicated looping AudioSource.")]
    [SerializeField] private AudioClip moveLoopClip;
    [SerializeField] private float moveLoopVolume = 1f;

    // runtime audio sources
    private AudioSource sfxSource;      // for one-shots
    private AudioSource loopSource;     // for continuous movement

    // previous states for edge detection
    private bool prevSwallow = false;
    private bool prevJump = false;
    private bool prevMove = false;
    private int prevStateHash = 0;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        // main SFX source (one-shots)
        sfxSource = GetComponent<AudioSource>();
        sfxSource.playOnAwake = false;

        // dedicated looping source (movement)
        loopSource = gameObject.AddComponent<AudioSource>();
        loopSource.playOnAwake = false;
        loopSource.loop = true;
        loopSource.spatialBlend = sfxSource.spatialBlend;
    }

    private void OnEnable()
    {
        // initialize prevStateHash to current to avoid false enter events on enable
        if (animator != null)
            prevStateHash = animator.GetCurrentAnimatorStateInfo(0).shortNameHash;
    }

    private void Update()
    {
        if (animator == null) return;

        // get current state hash once
        var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        int curStateHash = stateInfo.shortNameHash;

        // 1) Swallow - prefer boolean parameter, fallback to state entry
        bool swallowTriggered = false;
        if (!string.IsNullOrEmpty(swallowBoolName) && HasBoolParameter(swallowBoolName))
        {
            bool cur = SafeGetBool(swallowBoolName);
            swallowTriggered = cur && !prevSwallow;
            prevSwallow = cur;
        }
        else if (!string.IsNullOrEmpty(swallowStateName))
        {
            int h = Animator.StringToHash(swallowStateName);
            swallowTriggered = (curStateHash == h) && (prevStateHash != h);
        }
        if (swallowTriggered) PlayOneShot(swallowClip, swallowVolume);

        // 2) Jump - prefer boolean parameter, fallback to state entry
        bool jumpTriggered = false;
        if (!string.IsNullOrEmpty(jumpBoolName) && HasBoolParameter(jumpBoolName))
        {
            bool cur = SafeGetBool(jumpBoolName);
            jumpTriggered = cur && !prevJump;
            prevJump = cur;
        }
        else if (!string.IsNullOrEmpty(jumpStateName))
        {
            int h = Animator.StringToHash(jumpStateName);
            jumpTriggered = (curStateHash == h) && (prevStateHash != h);
        }
        if (jumpTriggered) PlayOneShot(jumpClip, jumpVolume);

        // 3) Die - detect state entry
        bool dieTriggered = false;
        if (!string.IsNullOrEmpty(dieStateName))
        {
            int h = Animator.StringToHash(dieStateName);
            dieTriggered = (curStateHash == h) && (prevStateHash != h);
        }
        if (dieTriggered) PlayOneShot(dieClip, dieVolume);

        // 4) Move - continuous. Prefer boolean parameter for start/stop loop. If none, detect entering/exiting moveState
        bool moving = false;
        if (!string.IsNullOrEmpty(moveBoolName) && HasBoolParameter(moveBoolName))
        {
            moving = SafeGetBool(moveBoolName);
        }
        else if (!string.IsNullOrEmpty(moveStateName))
        {
            int h = Animator.StringToHash(moveStateName);
            moving = (curStateHash == h);
        }

        if (moving && !prevMove)
        {
            StartMoveLoop();
        }
        else if (!moving && prevMove)
        {
            StopMoveLoop();
        }
        prevMove = moving;

        // update prevStateHash
        prevStateHash = curStateHash;
    }

    private void PlayOneShot(AudioClip clip, float volume)
    {
        if (clip == null || sfxSource == null) return;
        try
        {
            sfxSource.PlayOneShot(clip, Mathf.Clamp01(volume));
        }
        catch { }
    }

    private void StartMoveLoop()
    {
        if (moveLoopClip == null || loopSource == null) return;
        if (loopSource.clip != moveLoopClip)
            loopSource.clip = moveLoopClip;
        loopSource.volume = Mathf.Clamp01(moveLoopVolume);
        if (!loopSource.isPlaying) loopSource.Play();
    }

    private void StopMoveLoop()
    {
        if (loopSource == null) return;
        if (loopSource.isPlaying) loopSource.Stop();
    }

    // Public API so other scripts can directly trigger sounds if desired (no code changes required)
    public void PlaySwallow() => PlayOneShot(swallowClip, swallowVolume);
    public void PlayJump() => PlayOneShot(jumpClip, jumpVolume);
    public void PlayDie() => PlayOneShot(dieClip, dieVolume);
    public void StartMove() => StartMoveLoop();
    public void StopMove() => StopMoveLoop();

    // Safe helpers
    private bool HasBoolParameter(string name)
    {
        if (animator == null || string.IsNullOrEmpty(name)) return false;
        var pars = animator.parameters;
        for (int i = 0; i < pars.Length; i++)
            if (pars[i].name == name && pars[i].type == AnimatorControllerParameterType.Bool)
                return true;
        return false;
    }

    private bool SafeGetBool(string name)
    {
        try
        {
            return animator.GetBool(name);
        }
        catch
        {
            return false;
        }
    }

    private void OnDisable()
    {
        // ensure movement loop stops when component is disabled
        StopMoveLoop();
    }
}