using UnityEngine;
using UnityEngine.UI;
public class SettingsUI : MonoBehaviour
{
    public Toggle musicToggle;
    public Toggle sfxToggle;
    public Toggle vibrationToggle;

    private void Start()
    {
        // Initialize toggles from prefs
        musicToggle.isOn = PlayerPrefs.GetInt("MusicOn", 1) == 1;
        sfxToggle.isOn = PlayerPrefs.GetInt("SfxOn", 1) == 1;
        vibrationToggle.isOn = PlayerPrefs.GetInt("VibrationOn", 1) == 1;

        // Subscribe to changes
        musicToggle.onValueChanged.AddListener(AudioManager.Instance.SetMusic);
        sfxToggle.onValueChanged.AddListener(AudioManager.Instance.SetSfx);
        vibrationToggle.onValueChanged.AddListener(val => VibrationManager.VibrationOn = val);
    }
}
