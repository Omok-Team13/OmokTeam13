using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;


public class MainMenu : MonoBehaviour, IPointerEnterHandler, 
IPointerExitHandler
{
    [SerializeField] private GameObject originalBtn;
    [SerializeField] private Sprite omokpan;
    [SerializeField] private Sprite omokpan2;

    void Start()
    {
        
    }

    // Update is called once per frame

    void Update()
    {
        
    }

    public void OnClickQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void OnClickSceneChange()
    {
        SceneManager.LoadScene("omokgame");
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        originalBtn.GetComponent<Image>().sprite = omokpan2;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        originalBtn.GetComponent<Image>().sprite = omokpan;
    }

}
