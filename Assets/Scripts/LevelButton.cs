using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelButton : MonoBehaviour
{
    [Header("Level Settings")]
    [SerializeField] private int levelNumber = 1;

    [Header("UI References")]
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private GameObject lockIcon;
    [SerializeField] private GameObject completedIcon;
    [SerializeField] private Image buttonImage;

    [Header("Visual Settings")]
    [SerializeField] private Color unlockedColor = new Color(1f, 1f, 1f, 1f);
    [SerializeField] private Color lockedColor = new Color(0.5f, 0.5f, 0.5f, 1f);
    [SerializeField] private Color completedColor = new Color(0.5f, 1f, 0.5f, 1f);

    private LevelSelection levelSelection;
    private bool isLocked = true;

    private void Awake()
    {
        // Get button component if not assigned
        if (button == null)
        {
            button = GetComponent<Button>();
        }

        // Auto-find references if not assigned
        if (levelText == null)
        {
            levelText = GetComponentInChildren<TextMeshProUGUI>();
        }

        if (buttonImage == null)
        {
            buttonImage = GetComponent<Image>();
        }
    }

    public void Initialize(LevelSelection selection)
    {
        levelSelection = selection;

        // Set level number text
        if (levelText != null)
        {
            levelText.text = levelNumber.ToString();
        }

        // Add click listener
        if (button != null)
        {
            button.onClick.AddListener(OnButtonClicked);
        }
    }

    private void OnButtonClicked()
    {
        if (levelSelection != null)
        {
            levelSelection.OnLevelButtonClicked(levelNumber);
        }
    }

    public void SetLockState(bool locked)
    {
        isLocked = locked;

        // Show/hide lock icon
        if (lockIcon != null)
        {
            lockIcon.SetActive(locked);
        }

        // Update button interactability
        if (button != null)
        {
            button.interactable = !locked;
        }

        // Update visual appearance
        if (buttonImage != null)
        {
            buttonImage.color = locked ? lockedColor : unlockedColor;
        }

        // Update text visibility
        if (levelText != null)
        {
            levelText.enabled = !locked;
        }
    }

    public void SetCompletedState(bool completed)
    {
        // Show/hide completed icon
        if (completedIcon != null)
        {
            completedIcon.SetActive(completed);
        }

        // Update button color if completed
        if (completed && buttonImage != null && !isLocked)
        {
            buttonImage.color = completedColor;
        }
    }

    public int GetLevelNumber()
    {
        return levelNumber;
    }

    private void OnDestroy()
    {
        // Clean up listener
        if (button != null)
        {
            button.onClick.RemoveListener(OnButtonClicked);
        }
    }
}