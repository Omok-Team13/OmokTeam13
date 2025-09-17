using UnityEngine;

public class Win : MonoBehaviour
{
    //ÆË¾÷Ã¢ ÆÐ³Î
    public GameObject WinPanel;
    public void Start()
    {
        WinPanel = GameObject.Find("WinPanel");

        WinPanel.SetActive(false);
    }

}
