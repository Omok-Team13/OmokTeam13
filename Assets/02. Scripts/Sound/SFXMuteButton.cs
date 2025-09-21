using UnityEngine;
using UnityEngine.UI;

public class SFXMuteButton : MonoBehaviour
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
            lastVolume = SoundManager.Instance.GetSFXVolume();
            SoundManager.Instance.SetSFXVolume(0f);
            buttonImage.sprite = speakerOff;     
            isMuted = true;
        }
        else
        {
            //     
            SoundManager.Instance.SetSFXVolume(lastVolume);
            buttonImage.sprite = speakerOn; //            
            isMuted = false;
        }
    }
}

