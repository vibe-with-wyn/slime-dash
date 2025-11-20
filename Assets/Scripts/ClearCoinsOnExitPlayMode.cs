#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class ClearCoinsOnExitPlayMode
{
    private const string EDITOR_PREF_KEY = "ClearCoinsOnExitPlayMode.Enabled";
    private const string COINS_PREF_KEY = "CoinsCollected";

    static ClearCoinsOnExitPlayMode()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        // When returning to Edit mode after Play mode ended
        if (state == PlayModeStateChange.EnteredEditMode)
        {
            if (EditorPrefs.GetBool(EDITOR_PREF_KEY, false))
            {
                PlayerPrefs.DeleteKey(COINS_PREF_KEY);
                PlayerPrefs.Save();
                Debug.Log("ClearCoinsOnExitPlayMode: Deleted '" + COINS_PREF_KEY + "' PlayerPrefs key.");
                // If you also want to reset any on-screen UI, you can refresh it here by finding objects in the scene.
            }
        }
    }

    // Menu toggle at Tools > Clear Coins On Exit Play Mode
    [MenuItem("Tools/Clear Coins On Exit Play Mode")]
    private static void ToggleMenu()
    {
        bool current = EditorPrefs.GetBool(EDITOR_PREF_KEY, false);
        EditorPrefs.SetBool(EDITOR_PREF_KEY, !current);
        Menu.SetChecked("Tools/Clear Coins On Exit Play Mode", !current);
    }

    // Ensure menu checked state reflects stored value
    [MenuItem("Tools/Clear Coins On Exit Play Mode", true)]
    private static bool ToggleMenuValidate()
    {
        Menu.SetChecked("Tools/Clear Coins On Exit Play Mode", EditorPrefs.GetBool(EDITOR_PREF_KEY, false));
        return true;
    }
}
#endif
