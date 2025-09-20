using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SFXVolumeSlider : MonoBehaviour, IEndDragHandler
{
    public Slider slider;
    public AudioClip testSfx;
    private AudioSource previewSource;

    void Start()
    {
        slider.value = SoundManager.Instance.GetSFXVolume();
        slider.onValueChanged.AddListener((v) =>
        {
            SoundManager.Instance.SetSFXVolume(v);
        });

        previewSource = gameObject.AddComponent<AudioSource>();
        previewSource.playOnAwake = false;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (testSfx != null)
        {
            previewSource.volume = slider.value;
            previewSource.PlayOneShot(testSfx);
        }
    }
}