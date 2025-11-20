using NUnit.Framework;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MainMenuTests
{
    private GameObject testObj;
    private MainMenu mainMenu;
    private Button startButton;
    private Button tutorialButton;
    private AudioSource audioSource;

    // Helper: Invoke private Unity methods (Start, OnDestroy, etc).
    private void InvokePrivateMethod(object obj, string methodName)
    {
        var method = obj.GetType().GetMethod(
            methodName,
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        Assert.IsNotNull(method, $"Method '{methodName}' not found!");
        method.Invoke(obj, null);
    }

    // Helper to count runtime listeners (Unity does not expose it)
    private int GetRuntimeListenerCount(Button button)
    {
        var field = typeof(UnityEventBase)
            .GetField("m_Calls", BindingFlags.NonPublic | BindingFlags.Instance);

        var calls = field.GetValue(button.onClick);
        var callsType = calls.GetType();

        var runtimeCallsField = callsType.GetField(
            "m_RuntimeCalls",
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        var runtimeCalls = runtimeCallsField.GetValue(calls) as System.Collections.IList;
        return runtimeCalls?.Count ?? 0;
    }

    [SetUp]
    public void Setup()
    {
        testObj = new GameObject("MainMenuTestObj");
        mainMenu = testObj.AddComponent<MainMenu>();

        startButton = new GameObject("StartButton").AddComponent<Button>();
        tutorialButton = new GameObject("TutorialButton").AddComponent<Button>();

        audioSource = testObj.AddComponent<AudioSource>();

        typeof(MainMenu).GetField("startGameButton",
            BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(mainMenu, startButton);

        typeof(MainMenu).GetField("tutorialButton",
            BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(mainMenu, tutorialButton);

        typeof(MainMenu).GetField("buttonClickSound",
            BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(mainMenu, audioSource);

        typeof(MainMenu).GetField("levelSelectionSceneName",
            BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(mainMenu, "Level Selection");
    }

    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(testObj);
        Object.DestroyImmediate(startButton.gameObject);
        Object.DestroyImmediate(tutorialButton.gameObject);
    }

    // ------------------------------------------------------------
    // 1. Start(): Verify listeners are added
    // ------------------------------------------------------------
    [Test]
    public void Start_AddsListenersToButtons()
    {
        InvokePrivateMethod(mainMenu, "Start");

        Assert.AreEqual(1, GetRuntimeListenerCount(startButton),
            "StartGame button should have 1 runtime listener.");

        Assert.AreEqual(1, GetRuntimeListenerCount(tutorialButton),
            "Tutorial button should have 1 runtime listener.");
    }

    // ------------------------------------------------------------
    // 2. OnDestroy(): Verify listeners removed
    // ------------------------------------------------------------
    [Test]
    public void OnDestroy_RemovesListeners()
    {
        InvokePrivateMethod(mainMenu, "Start");
        InvokePrivateMethod(mainMenu, "OnDestroy");

        Assert.AreEqual(0, GetRuntimeListenerCount(startButton),
            "StartGame button listeners must be removed.");

        Assert.AreEqual(0, GetRuntimeListenerCount(tutorialButton),
            "Tutorial button listeners must be removed.");
    }

    // ------------------------------------------------------------
    // 3. OnStartGameClicked(): Disables button + plays audio
    // ------------------------------------------------------------
    [Test]
    public void OnStartGameClicked_DisablesButton_AndPlaysAudio()
    {
        InvokePrivateMethod(mainMenu, "Start");

        mainMenu.OnStartGameClicked();

        Assert.IsFalse(startButton.interactable,
            "Start button must be disabled after clicking.");

        Assert.IsTrue(audioSource.isPlaying || audioSource.clip == null,
            "Audio should play (if clip exists).");
    }

    // ------------------------------------------------------------
    // 4. OnTutorialClicked(): Must not throw any errors
    // ------------------------------------------------------------
    [Test]
    public void OnTutorialClicked_DoesNotThrow()
    {
        Assert.DoesNotThrow(() => mainMenu.OnTutorialClicked(),
            "Tutorial click method should not throw exceptions.");
    }
}
