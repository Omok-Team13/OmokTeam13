using UnityEngine;

public class ButtonChangeBGM : MonoBehaviour
{
    public AudioClip newBGM;

    public void ChangeBGM()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayBGM(newBGM);
        }
        else
        {
            Debug.LogWarning("SoundManager가 존재하지 않습니다!");
        }
    }
}