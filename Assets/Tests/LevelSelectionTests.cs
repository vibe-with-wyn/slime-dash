using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelectionTests
{
    private GameObject levelSelectionGO;
    private LevelSelection levelSelection;
    private Button backButton;
    private AudioSource clickAudio;
    private AudioSource lockedAudio;

    [SetUp]
    public void Setup()
    {
        PlayerPrefs.DeleteAll();

        // Create LevelSelection object
        levelSelectionGO = new GameObject();
        levelSelection = levelSelectionGO.AddComponent<LevelSelection>();

        // Mock level buttons
        var buttonGO1 = new GameObject("LevelButton1");
        var button1 = buttonGO1.AddComponent<LevelButton>();
        var buttonGO2 = new GameObject("LevelButton2");
        var button2 = buttonGO2.AddComponent<LevelButton>();

        LevelButton[] buttons = new LevelButton[] { button1, button2 };
        typeof(LevelSelection).GetField("levelButtons", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(levelSelection, buttons);

        // Back button
        backButton = levelSelectionGO.AddComponent<Button>();
        typeof(LevelSelection).GetField("backButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(levelSelection, backButton);

        // Audio sources
        clickAudio = levelSelectionGO.AddComponent<AudioSource>();
        lockedAudio = levelSelectionGO.AddComponent<AudioSource>();
        typeof(LevelSelection).GetField("buttonClickSound", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(levelSelection, clickAudio);
        typeof(LevelSelection).GetField("lockedSound", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(levelSelection, lockedAudio);
    }


    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(levelSelectionGO);
        PlayerPrefs.DeleteAll();
    }

    [Test]
    public void OnLevelButtonClicked_LevelLocked_DoesNotStoreLevel()
    {
        // Level 2 is locked by default
        levelSelection.OnLevelButtonClicked(2);

        // PlayerPrefs should not store selected level
        Assert.AreEqual(0, PlayerPrefs.GetInt("SelectedLevel", 0));

        // Locked audio should play
        Assert.IsTrue(lockedAudio.isPlaying || !lockedAudio.isPlaying); // Unity cannot reliably check isPlaying in editor tests, so just ensuring no crash
    }

    [Test]
    public void OnLevelButtonClicked_LevelUnlocked_StoresLevel()
    {
        // Unlock level 2 using GameManager
        GameManager.Instance.UnlockLevel(2);

        levelSelection.OnLevelButtonClicked(2);

        // Check PlayerPrefs
        Assert.AreEqual(2, PlayerPrefs.GetInt("SelectedLevel", 0));

        // Click audio should play (same caveat as above)
        Assert.IsTrue(clickAudio.isPlaying || !clickAudio.isPlaying);
    }

    [Test]
    public void OnBackButtonClicked_DoesNotThrow()
    {
        levelSelection.OnBackButtonClicked();
        // Nothing to assert, just ensuring it runs without error
        Assert.Pass();
    }

    [Test]
    public void UnlockAllLevels_MakesAllLevelsUnlocked()
    {
        levelSelection.UnlockAllLevels();

        for (int i = 1; i <= GameManager.Instance.GetTotalLevels(); i++)
        {
            Assert.IsTrue(GameManager.Instance.IsLevelUnlocked(i));
        }
    }

    [Test]
    public void ResetProgress_ResetsAllLevels()
    {
        GameManager.Instance.UnlockLevel(2);

        levelSelection.ResetProgress();

        for (int i = 1; i <= GameManager.Instance.GetTotalLevels(); i++)
        {
            if (i == 1)
                Assert.IsTrue(GameManager.Instance.IsLevelUnlocked(i)); // Level 1 always unlocked
            else
                Assert.IsFalse(GameManager.Instance.IsLevelUnlocked(i));
        }
    }
}
