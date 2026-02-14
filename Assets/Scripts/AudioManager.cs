using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource voiceSource;
    [SerializeField] private AudioSource bgmSource;

    [Header("Default SFX")]
    public AudioClip defaultMergeClip;
    public AudioClip dropClip;
    public AudioClip gameOverClip;

    [Header("UI SFX")]
    public AudioClip uiClickClip;
    public AudioClip uiCloseClip;

    [Header("BGM")]
    public AudioClip bgmClip;

    [Header("Settings")]
    [Range(0f, 1f)] public float sfxVolume = 1f;
    [Range(0f, 1f)] public float voiceVolume = 1f;
    [Range(0f, 1f)] public float bgmVolume = 0.5f;

    // 볼륨 getter (UI 초기화용)
    public float BGMVolume => bgmVolume;
    public float SFXVolume => sfxVolume;
    public float VoiceVolume => voiceVolume;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // 저장된 볼륨 설정 불러오기
        bgmVolume = PlayerPrefs.GetFloat("BGMVolume", 0.5f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        voiceVolume = PlayerPrefs.GetFloat("VoiceVolume", 1f);
    }

    private void Start()
    {
        if (bgmClip != null && bgmSource != null)
        {
            bgmSource.clip = bgmClip;
            bgmSource.loop = true;
            bgmSource.volume = bgmVolume;
            bgmSource.Play();
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) clip = defaultMergeClip;
        if (clip == null) return;
        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    public void PlayVoice(AudioClip clip)
    {
        if (clip == null) return;
        voiceSource.PlayOneShot(clip, voiceVolume);
    }

    public void PlayDrop()
    {
        PlaySFX(dropClip);
    }

    public void PlayGameOver()
    {
        PlaySFX(gameOverClip);
    }

    public void PlayMerge(AudioClip mergeSfx, AudioClip voiceClip)
    {
        PlaySFX(mergeSfx);
        PlayVoice(voiceClip);
    }

    public void PlayUIClick()
    {
        PlaySFX(uiClickClip);
    }

    public void PlayUIClose()
    {
        PlaySFX(uiCloseClip);
    }

    // 볼륨 설정 (슬라이더 콜백)
    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        if (bgmSource != null)
            bgmSource.volume = bgmVolume;
        PlayerPrefs.SetFloat("BGMVolume", bgmVolume);
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
    }

    public void SetVoiceVolume(float volume)
    {
        voiceVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat("VoiceVolume", voiceVolume);
    }
}
