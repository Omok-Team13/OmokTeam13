using UnityEngine;

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
        // 싱글톤 + 중복 방지
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 이동해도 유지됨

            // 저장된 볼륨 불러오기
            float bgmVol = PlayerPrefs.GetFloat("BGM_VOLUME", 1f);
            float sfxVol = PlayerPrefs.GetFloat("SFX_VOLUME", 1f);

            bgmSource.volume = bgmVol;
            sfxSource.volume = sfxVol;
        }
        else if (Instance != this)
        {
            Destroy(gameObject); // 중복 SoundManager는 제거
        }
    }

    private void Start()
    {
        if (bgmClip != null)
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

        // 혹시 꺼져 있으면 자동으로 재생
        if (!bgmSource.isPlaying && volume > 0 && bgmSource.clip != null)
        {
            bgmSource.Play();
        }
    }

    public float GetBGMVolume()
    {
        return bgmSource.volume;
    }

    public void SetSFXVolume(float volume)
    {
        sfxSource.volume = volume;
        PlayerPrefs.SetFloat("SFX_VOLUME", volume);
    }

    public float GetSFXVolume()
    {
        return sfxSource.volume;
    }
}