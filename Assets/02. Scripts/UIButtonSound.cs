using UnityEngine;
using UnityEngine.UI;

public class UIButtonSound : MonoBehaviour
{
    public AudioClip clickSfx;

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            SoundManager.Instance.PlaySFX(clickSfx);
        });
    }
}