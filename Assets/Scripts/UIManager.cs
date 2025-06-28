using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject settingsPanel;
    public GameObject shopPanel;

    public void PlayGame()
    {
        SceneManager.LoadScene("SagaMap");
    }

    public void OpenSettings()
    {
        settingsPanel.SetActive(true);
    }
    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
    }

    public void OpenShop()
    {
        shopPanel.SetActive(true);
    }
    public void CloseShop()
    {
        shopPanel.SetActive(false);
    }
}
