using UnityEngine;
using UnityEngine.UI;

public class BGMMuteButton : MonoBehaviour
{
    public Image buttonImage;       
    public Sprite speakerOn;              
    public Sprite speakerOff;           

    private bool isMuted = false;
    private float lastVolume = 1f;

    public void ToggleMute()
    {
        if (!isMuted)
        {
            //    ? 
            lastVolume = SoundManager.Instance.GetBGMVolume();
            SoundManager.Instance.SetBGMVolume(0f);
            buttonImage.sprite = speakerOff;     
            isMuted = true;
        }
        else
        {
            //     
            SoundManager.Instance.SetBGMVolume(lastVolume);
            buttonImage.sprite = speakerOn; //            
            isMuted = false;
        }
    }
}

