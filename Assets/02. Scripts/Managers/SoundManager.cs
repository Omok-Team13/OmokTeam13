using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("오디오 소스")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;

    [Header("기본 BGM")]
    public AudioClip bgmClip;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 저장된 볼륨 불러오기
            float bgmVol = PlayerPrefs.GetFloat("BGM_VOLUME", 1f);
            float sfxVol = PlayerPrefs.GetFloat("SFX_VOLUME", 1f);

            bgmSource.volume = bgmVol;
            sfxSource.volume = sfxVol;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        PlayBGM(bgmClip);
    }

    // ===== BGM =====
    public void PlayBGM(AudioClip clip)
    {
        if (clip == null) return;

        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }

    // ===== SFX =====
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip);
    }

    public void StopSFX()
    {
        sfxSource.Stop();
    }

    // ===== 볼륨 조절 =====
    public void SetBGMVolume(float volume)
    {
        bgmSource.volume = volume;
        PlayerPrefs.SetFloat("BGM_VOLUME", volume);
        Debug.Log("BGM Volume : " + volume);
        bgmSource.volume = volume;
        // 혹시 꺼져 있으면 자동으로 재생
        if (!bgmSource.isPlaying && volume > 0)
        {
            bgmSource.Play();
        }
    }

    public void SetSFXVolume(float volume)
    {
        sfxSource.volume = volume;
        PlayerPrefs.SetFloat("SFX_VOLUME", volume);
    }
}