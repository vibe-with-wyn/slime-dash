using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.TestTools;

public class GameLaunchTests
{
    private GameObject testObject;
    private GameLaunch gameLaunch;

    private Transform slimeTransform;
    private Animator slimeAnimator;

    [SetUp]
    public void Setup()
    {
        // Create test game object
        testObject = new GameObject("GameLaunchTestObj");
        gameLaunch = testObject.AddComponent<GameLaunch>();

        // Fake slime transform
        GameObject slimeObj = new GameObject("Slime");
        slimeTransform = slimeObj.transform;

        // Fake animator
        slimeAnimator = slimeObj.AddComponent<Animator>();

        // Assign required references
        typeof(GameLaunch)
            .GetField("slimeTransform", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(gameLaunch, slimeTransform);

        typeof(GameLaunch)
            .GetField("slimeAnimator", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(gameLaunch, slimeAnimator);

        // Example test values
        typeof(GameLaunch)
            .GetField("startPosition", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(gameLaunch, new Vector3(1, 2, 3));
    }

    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(testObject);
    }

    // ---------------------------------------------------
    // 1. Start() should set slime position to startPosition
    // ---------------------------------------------------
    [Test]
    public void Start_SetsSlimePositionCorrectly()
    {
        gameLaunch.SendMessage("Start");

        Assert.AreEqual(
            new Vector3(1, 2, 3),
            slimeTransform.position,
            "Slime initial position should match startPosition."
        );
    }

    // ---------------------------------------------------
    // 2. LoadNextScene() attempts to load a scene
    // ---------------------------------------------------
    private bool sceneLoaded = false;

    [UnityTest]
    public IEnumerator LoadNextScene_LoadsScene()
    {
        sceneLoaded = false;

        // Set scene name
        typeof(GameLaunch)
            .GetField("nextSceneName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(gameLaunch, "Main Menu");

        // Listen to scene load event
        SceneManager.sceneLoaded += OnSceneLoaded;

        gameLaunch.SendMessage("LoadNextScene");

        // Wait a few frames
        yield return null;

        // Unsubscribe
        SceneManager.sceneLoaded -= OnSceneLoaded;

        Assert.IsTrue(sceneLoaded, "LoadNextScene should load the MainMenu scene.");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        sceneLoaded = true;
    }

    // ---------------------------------------------------
    // 3. Animator SetTrigger is called safely
    // ---------------------------------------------------
    [Test]
    public void Animator_DoesNotThrow_WhenTriggerCalled()
    {
        Assert.DoesNotThrow(() => {
            slimeAnimator.SetTrigger("Move");
        });
    }
}
