using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;    // Or UnityEngine.UI if you use UI.Text
using UnityEngine.UI;

/// <summary>
/// Attach this to your LevelButton prefab. 
/// On click, it writes NextLevel to PlayerPrefs and loads the LevelScene.
/// </summary>
[RequireComponent(typeof(Button))]
public class LevelButton : MonoBehaviour
{
    [Tooltip("Which level this button represents (1..N)")]
    public int levelIndex;

    [Header("Scene Settings")]
    [Tooltip("Exact name of your scene that contains BoardManager. E.g. 'LevelScene'")]
    public string levelSceneName = "LevelScene";

    private Button btn;

    private void Awake()
    {
        btn = GetComponent<Button>();
    }

    /// <summary>
    /// Called by SagaMapManager when wiring up the onClick.
    /// </summary>
    public void OnButtonClicked()
    {
        // Save the clicked level for BoardManager to pick up
        PlayerPrefs.SetInt("NextLevel", levelIndex);

        // Optionally save “LevelUnlocked” here if you want to
        // e.g. to allow replaying the same level without re‐unlocking logic.
        // PlayerPrefs.SetInt("LevelUnlocked", Mathf.Max(PlayerPrefs.GetInt("LevelUnlocked", 1), levelIndex));

        // Load the level scene
        SceneManager.LoadScene(levelSceneName);
    }
}
