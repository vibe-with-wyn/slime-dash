using UnityEngine;

public class PauseButton : MonoBehaviour
{
    // Called by UI Button OnClick
    public void OnPausePressed()
    {
        var pm = FindObjectOfType<PauseManager>();
        if (pm != null)
        {
            pm.TogglePause();
        }
        else
        {
            Debug.LogWarning("PauseButton: No PauseManager found in scene.");
        }
    }
}