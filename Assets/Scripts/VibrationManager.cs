using UnityEngine;

public static class VibrationManager
{
    private const string VibPref = "VibrationOn";

    public static bool VibrationOn
    {
        get => PlayerPrefs.GetInt(VibPref, 1) == 1;
        set { PlayerPrefs.SetInt(VibPref, value ? 1 : 0); PlayerPrefs.Save(); }
    }

    public static void Vibrate()
    {
        if (!VibrationOn) return;
#if UNITY_IOS || UNITY_ANDROID
        Handheld.Vibrate();
#endif
    }
}
