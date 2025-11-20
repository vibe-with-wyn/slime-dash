using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

/// <summary>
/// Attach this to a UI element (Image/Button) to receive pointer down/up events.
/// Use the inspector to hook OnPointerDownEvent / OnPointerUpEvent to PlayerController methods.
/// </summary>
public class MobileButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public UnityEvent OnPointerDownEvent;
    public UnityEvent OnPointerUpEvent;

    public void OnPointerDown(PointerEventData eventData)
    {
        OnPointerDownEvent?.Invoke();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        OnPointerUpEvent?.Invoke();
    }

    // Ensure release if the pointer exits while pressed
    public void OnPointerExit(PointerEventData eventData)
    {
        if (eventData != null && eventData.pointerPress != null)
            OnPointerUpEvent?.Invoke();
    }
}