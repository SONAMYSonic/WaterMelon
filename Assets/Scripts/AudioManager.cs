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

    [Header("BGM")]
    public AudioClip bgmClip;

    [Header("Settings")]
    [Range(0f, 1f)] public float sfxVolume = 1f;
    [Range(0f, 1f)] public float voiceVolume = 1f;
    [Range(0f, 1f)] public float bgmVolume = 0.5f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
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
}
