using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
public class UIPanelManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject winPanel;
    public GameObject losePanel;

    [Header("Common UI")]
    public TextMeshProUGUI headerText;
    public Image[] starImages;
    public Sprite starFilledSprite;
    public Sprite starEmptySprite;
    public TextMeshProUGUI winScoreText;   
    public TextMeshProUGUI loseScoreText;
    public TextMeshProUGUI movesText;

    [Header("Scene Names")]
    public string levelSceneName = "LevelScene";
    public string sagaMapSceneName = "SagaMap";

    private int currentLevel;
    private int currentScore;
    private int currentStars;

    

    public void ShowWinPanel(int level, int starsEarned, int score)
    {
        currentLevel = level;
        currentStars = Mathf.Clamp(starsEarned, 0, starImages.Length);
        currentScore = score;

        // Header
        headerText.text = $"Level {level}";

        // Stars
        for (int i = 0; i < starImages.Length; i++)
        {
            starImages[i].sprite = (i < currentStars)
                ? starFilledSprite
                : starEmptySprite;
        }

        winScoreText.text = $"Score: {score:N0}";
        winPanel.SetActive(true);
        losePanel.SetActive(false); ;
    }
    public void ShowLosePanel(int level, int score)
    {
        currentLevel = level;
        currentStars = 0;
        currentScore = score;

        // Header
        headerText.text = $"Level {level}";

        // All stars empty
        foreach (var img in starImages)
            img.sprite = starEmptySprite;

        loseScoreText.text = $"Score: {score:N0}";
        losePanel.SetActive(true);
        winPanel.SetActive(false);
    }
    public void OnContinueButton()
    {
        PlayerPrefs.SetInt("NextLevel", currentLevel + 1);
        SceneManager.LoadScene(levelSceneName);
    }
    public void OnRetryButton()
    {
        PlayerPrefs.SetInt("NextLevel", currentLevel);
        SceneManager.LoadScene(levelSceneName);
    }
    public void OnBackToMapButton()
    {
        SceneManager.LoadScene(sagaMapSceneName);
    }
    public void CloseWinPanel()
    {
        winPanel.SetActive(false);
    }

    public void CloseLosePanel()
    {
        losePanel.SetActive(false);
    }
}

