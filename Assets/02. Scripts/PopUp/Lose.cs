using UnityEngine;

public class Lose : MonoBehaviour
{
    //ÆË¾÷Ã¢ ÆÐ³Î
    public GameObject LosePanel;
    public void Start()
    {
        LosePanel = GameObject.Find("Lose");

        LosePanel.SetActive(false);
    }

}
