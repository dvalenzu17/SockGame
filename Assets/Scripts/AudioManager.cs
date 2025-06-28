using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Clips")]
    public AudioClip bgMusic;
    public AudioClip swapSfx;
    public AudioClip matchSfx;
    public AudioClip powerupSfx;
    private bool isFading = false;

    private const string MusicPref = "MusicOn";
    private const string SfxPref = "SfxOn";

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }

        // Load prefs (default = on)
        bool musicOn = PlayerPrefs.GetInt(MusicPref, 1) == 1;
        bool sfxOn = PlayerPrefs.GetInt(SfxPref, 1) == 1;
        SetMusic(musicOn);
        SetSfx(sfxOn);
    }
    private void Start()
    {
        
            PlayMusic();
    }
    public void StopMusic()
    {
        StartCoroutine(FadeOutMusic());
    }

    public void PlayMusic()
    {
        StartCoroutine(FadeInMusic());
    }


    public void PlaySwap() { if (sfxSource && swapSfx != null) PlaySfx(swapSfx); }
    public void PlayMatch() { if (sfxSource && matchSfx != null) PlaySfx(matchSfx); }
    public void PlayPowerup() { if (sfxSource && powerupSfx != null) PlaySfx(powerupSfx); }

    private void PlaySfx(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }

    public void SetMusic(bool on)
    {
        musicSource.volume = on ? 1f : 0f;
        PlayerPrefs.SetInt(MusicPref, on ? 1 : 0);
        PlayerPrefs.Save();
        if (on) PlayMusic(); else StopMusic();
    }

    public void SetSfx(bool on)
    {
        sfxSource.volume = on ? 1f : 0f;
        PlayerPrefs.SetInt(SfxPref, on ? 1 : 0);
        PlayerPrefs.Save();
    }
    private IEnumerator FadeInMusic()
    {
        musicSource.clip = bgMusic;
        musicSource.volume = 0f;  // start at zero
        musicSource.loop = true;
        musicSource.Play();

        // Fade in the volume
        float targetVolume = 1f;
        float fadeSpeed = 0.5f; // how fast the music fades in
        while (musicSource.volume < targetVolume)
        {
            musicSource.volume += fadeSpeed * Time.deltaTime;
            yield return null;
        }

        musicSource.volume = targetVolume; // ensure it’s at full volume
    }
    private IEnumerator FadeOutMusic()
    {
        float startVolume = musicSource.volume;
        float fadeSpeed = 0.5f; // how fast the music fades out

        while (musicSource.volume > 0)
        {
            musicSource.volume -= fadeSpeed * Time.deltaTime;
            yield return null;
        }

        musicSource.Stop();
        musicSource.volume = startVolume; // reset volume after fade out
    }

}
