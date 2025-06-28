using UnityEngine;
using UnityEngine.UI;


public class BottomNavBar : MonoBehaviour
{
    [Header("Nav Buttons")]
    public Button mapButton;
    public Button shopButton;
    public Button socialButton;

    [Header("Panels to Toggle")]
    public GameObject shopPanel;
    public GameObject socialPanel;
    void Start()
    {
        // Wire up click events
        mapButton.onClick.AddListener(ShowMap);
        shopButton.onClick.AddListener(ShowShop);
        socialButton.onClick.AddListener(ShowSocial);

        // Initialize default
        ShowMap();
    }
 public void ShowMap()
    {
       
        shopPanel.SetActive(false);
        socialPanel.SetActive(false);
        Highlight(mapButton);
    }
    public void ShowShop()
    {
        shopPanel.SetActive(true);
        socialPanel.SetActive(false);
        Highlight(shopButton);
    }
    public void ShowSocial()
    {
        shopPanel.SetActive(false);
        socialPanel.SetActive(true);
        Highlight(socialButton);
    }
    private void Highlight(Button active)
    {
        // Example: scale active button up slightly, reset others
        float onScale = 1.1f;
        float offScale = 1.0f;

        mapButton.transform.localScale = (active == mapButton) ? Vector3.one * onScale : Vector3.one * offScale;
        shopButton.transform.localScale = (active == shopButton) ? Vector3.one * onScale : Vector3.one * offScale;
        socialButton.transform.localScale = (active == socialButton) ? Vector3.one * onScale : Vector3.one * offScale;
    }
}
