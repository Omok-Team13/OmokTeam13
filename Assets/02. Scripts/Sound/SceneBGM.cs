using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneBGM : MonoBehaviour
{
    public AudioClip stageBgm;

    void Start()
    {
        // 씬 시작할 때 자동으로 BGM 재생
        SoundManager.Instance.PlayBGM(stageBgm);
    }
}