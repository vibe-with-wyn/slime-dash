using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Plays a selection SFX when a UI Button (or any Selectable) is selected, clicked or submitted.
// Hover/pointer-enter no longer triggers sound. Click sound only plays when the element is currently selected.
public class ButtonSelectSfx : MonoBehaviour, ISelectHandler, IPointerEnterHandler, IPointerClickHandler, ISubmitHandler
{
    [Tooltip("Clip played when the UI element is selected (keyboard/gamepad) or activated.")]
    [SerializeField] private AudioClip selectClip;

    [Tooltip("Volume for the selection clip (0..1).")]
    [SerializeField][Range(0f, 1f)] private float volume = 1f;

    [Tooltip("Optional AudioSource to play the clip. If null, PlayClipAtPoint will be used.")]
    [SerializeField] private AudioSource audioSource;

    [Tooltip("Play clip when element is selected via EventSystem (keyboard/gamepad).")]
    [SerializeField] private bool playOnSelect = true;

    [Tooltip("Play clip when the element is clicked (mouse) or pressed (touch) — only when the element is selected.")]
    [SerializeField] private bool playOnClick = true;

    private Selectable selectable;

    private void Awake()
    {
        selectable = GetComponent<Selectable>();
        // prefer an AudioSource on this GameObject, then on parent, if none provided
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>() ?? GetComponentInParent<AudioSource>();
    }

    private void Start()
    {
        if (EventSystem.current == null)
        {
            Debug.LogWarning("ButtonSelectSfx: No EventSystem found in scene. Pointer/Select events won't fire without one.");
        }
    }

    // Play when the element becomes selected (keyboard/gamepad navigation)
    public void OnSelect(BaseEventData eventData)
    {
        if (!playOnSelect) return;
        if (!IsInteractable()) return;
        PlaySelectClip();
    }

    // Hover is intentionally ignored to avoid playing hover sounds.
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Intentionally left blank: hover should not play any sound per requirements.
    }

    // Click only plays sound if the element is currently selected in the EventSystem.
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!playOnClick) return;
        if (!IsInteractable()) return;

        var es = EventSystem.current;
        if (es == null)
        {
            // No event system — be conservative and do not play click sound.
            return;
        }

        // Only play click sound when this GameObject is the currently selected object.
        if (es.currentSelectedGameObject == this.gameObject)
        {
            PlaySelectClip();
        }
    }

    // Submit (keyboard/gamepad confirm) should behave like selection/activation.
    public void OnSubmit(BaseEventData eventData)
    {
        if (!playOnSelect) return;
        if (!IsInteractable()) return;
        PlaySelectClip();
    }

    private bool IsInteractable()
    {
        // if there's a Selectable (Button/Toggle/etc) ensure it's interactable before playing
        if (selectable != null)
            return selectable.interactable;
        return true;
    }

    private void PlaySelectClip()
    {
        if (selectClip == null)
        {
            Debug.LogWarning($"ButtonSelectSfx: no selectClip assigned on '{gameObject.name}'.");
            return;
        }

        if (audioSource != null)
        {
            try
            {
                audioSource.PlayOneShot(selectClip, Mathf.Clamp01(volume));
            }
            catch
            {
                // fallback
                var listenerPos = GetAudioListenerPosition();
                AudioSource.PlayClipAtPoint(selectClip, listenerPos, Mathf.Clamp01(volume));
            }
            return;
        }

        // Fallback: 2D one-shot at AudioListener position
        var pos = GetAudioListenerPosition();
        AudioSource.PlayClipAtPoint(selectClip, pos, Mathf.Clamp01(volume));
    }

    private static Vector3 GetAudioListenerPosition()
    {
        var listener = Object.FindFirstObjectByType<AudioListener>();
        if (listener != null)
            return listener.transform.position;
        if (Camera.main != null)
            return Camera.main.transform.position;
        return Vector3.zero;
    }
}