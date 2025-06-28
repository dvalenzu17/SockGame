using UnityEngine;
using UnityEngine.UI;
using TMPro; // Only if you use TextMeshPro for labels

/// <summary>
/// Spawns N level‐button UI instances under a RectTransform parent at runtime,
/// arranging them in a vertical column that grows upward. If a ScrollRect is assigned,
/// it will automatically scroll to the bottom so level 1 is visible first.
/// </summary>
public class SagaMapManager : MonoBehaviour
{
    [Header("Prefab & Parent")]
    [Tooltip("Prefab for each level button. Must have a RectTransform, Button, and LevelButton component.")]
    public GameObject levelButtonPrefab;

    [Tooltip("RectTransform that acts as the Content of a ScrollRect. Its Pivot should be (0.5, 0) (bottom-center).")]
    public RectTransform buttonParent;

    [Header("ScrollRect (optional)")]
    [Tooltip("If you want auto-scroll behavior, drag your ScrollRect here. Otherwise leave null.")]
    public ScrollRect scrollRect;

    [Header("Layout Settings")]
    [Tooltip("How many levels to generate (e.g. 30)")]
    public int maxLevel = 30;

    [Tooltip("Vertical spacing (in UI units) between adjacent buttons.")]
    public float ySpacing = 160f;

    [Tooltip("Horizontal offset (in UI units) for a simple zig-zag effect. Set to 0 for a straight column.")]
    public float xOffset = 0f;

    [Header("Unlock Settings")]
    [Tooltip("PlayerPrefs key for tracking how many levels are unlocked.")]
    public string unlockedKey = "LevelUnlocked";

    private void Start()
    {
        if (levelButtonPrefab == null || buttonParent == null)
        {
            Debug.LogError($"[{nameof(SagaMapManager)}] Missing references! Assign levelButtonPrefab and buttonParent in the Inspector.");
            return;
        }

        // Get how many levels are unlocked (default = 1)
        int unlocked = PlayerPrefs.GetInt(unlockedKey, 1);

        // Instantiate buttons 1..maxLevel
        for (int i = 1; i <= maxLevel; i++)
        {
            // 1) Instantiate under the UI parent (so it’s in the ScrollRect’s content)
            GameObject btnGO = Instantiate(levelButtonPrefab, buttonParent);
            btnGO.name = $"LevelButton_{i}";

            // 2) Get RectTransform so we can set anchoredPosition
            RectTransform rt = btnGO.GetComponent<RectTransform>();
            if (rt == null)
            {
                Debug.LogWarning($"[{nameof(SagaMapManager)}] The prefab does not have a RectTransform!");
                continue;
            }

            // 3) Compute a vertical position that grows upward:
            //    Since buttonParent’s pivot is (0.5, 0), (0,0) is bottom-center.
            //    We place level 1 at y=0, level 2 at y=+ySpacing, level 3 at y=+2*ySpacing, etc.
            float posY = (i - 1) * ySpacing;

            // 4) Optional zig-zag: odd levels at +xOffset, even at –xOffset
            float posX = 0f;
            if (xOffset != 0f)
                posX = (i % 2 == 1) ? +xOffset : -xOffset;

            rt.anchoredPosition = new Vector2(posX, posY);

            // 5) Assign the LevelButton script’s levelIndex
            LevelButton lvlBtn = btnGO.GetComponent<LevelButton>();
            if (lvlBtn == null)
            {
                Debug.LogWarning($"[{nameof(SagaMapManager)}] Instantiated prefab '{btnGO.name}' is missing LevelButton component.");
            }
            else
            {
                lvlBtn.levelIndex = i;
            }

            // 6) Update the label text (TextMeshProUGUI or UI.Text)
            TextMeshProUGUI labelTMP = btnGO.GetComponentInChildren<TextMeshProUGUI>();
            if (labelTMP != null)
            {
                labelTMP.text = $"{i}";
            }
            else
            {
                Text labelUI = btnGO.GetComponentInChildren<Text>();
                if (labelUI != null)
                    labelUI.text = $"{i}";
            }

            // 7) Enable/disable the button based on unlocked count
            Button unityBtn = btnGO.GetComponent<Button>();
            if (unityBtn == null)
            {
                Debug.LogWarning($"[{nameof(SagaMapManager)}] '{btnGO.name}' is missing a Button component.");
            }
            else
            {
                if (i > unlocked)
                {
                    // Locked: non-interactable + gray out
                    unityBtn.interactable = false;
                    SetButtonGray(btnGO);
                }
                else
                {
                    // Unlocked: wire OnClick to LevelButton.OnButtonClicked
                    unityBtn.interactable = true;
                    unityBtn.onClick.AddListener(() =>
                    {
                        if (lvlBtn != null)
                            lvlBtn.OnButtonClicked();
                    });
                }
            }
        }

        // 8) If using a ScrollRect, force it to show the bottom (level 1) initially
        AdjustScrollViewHeight(maxLevel);

        // 9) Force ScrollRect to show the bottom (level 1) initially if ScrollRect is assigned
        if (scrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f; // pin the content to the bottom (level 1 visible first)
        }
    }
    private void AdjustScrollViewHeight(int totalLevels)
    {
        RectTransform contentRect = buttonParent.GetComponent<RectTransform>();

        // Calculate total height based on number of levels
        float totalHeight = totalLevels * (ySpacing);
        contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, totalHeight); // update the content's height
    }

    /// <summary>
    /// Applies a “grayed-out” look to a locked button (dims both Image and child text).
    /// </summary>
    private void SetButtonGray(GameObject btnGO)
    {
        // Dim the Button’s background image (if any)
        Image img = btnGO.GetComponent<Image>();
        if (img != null)
        {
            Color c = img.color;
            c.a = 0.4f;
            img.color = c;
        }

        // Dim the label text (TextMeshProUGUI)
        TextMeshProUGUI labelTMP = btnGO.GetComponentInChildren<TextMeshProUGUI>();
        if (labelTMP != null)
        {
            Color c = labelTMP.color;
            c.a = 0.4f;
            labelTMP.color = c;
            return;
        }

        // Fallback: dim UnityEngine.UI.Text if you’re not using TextMeshPro
        Text labelUI = btnGO.GetComponentInChildren<Text>();
        if (labelUI != null)
        {
            Color c = labelUI.color;
            c.a = 0.4f;
            labelUI.color = c;
        }
    }
}
