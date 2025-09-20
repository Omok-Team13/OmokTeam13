using UnityEngine;
using UnityEngine.UI;

public class BGMMuteButton : MonoBehaviour
{
    public Image buttonImage;       // 버튼에 붙은 Image (아이콘)
    public Sprite speakerOn;        // 켜짐 아이콘
    public Sprite speakerOff;       // 꺼짐 아이콘

    private bool isMuted = false;
    private float lastVolume = 1f;

    public void ToggleMute()
    {
        if (!isMuted)
        {
            // 음소거
            lastVolume = SoundManager.Instance.GetBGMVolume();
            SoundManager.Instance.SetBGMVolume(0f);
            buttonImage.sprite = speakerOff; // 아이콘 변경
            isMuted = true;
        }
        else
        {
            // 해제
            SoundManager.Instance.SetBGMVolume(lastVolume);
            buttonImage.sprite = speakerOn; // 아이콘 변경
            isMuted = false;
        }
    }
}