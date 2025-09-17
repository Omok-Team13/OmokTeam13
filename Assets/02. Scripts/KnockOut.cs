using UnityEngine;

public class KnockOut : MonoBehaviour
{
    //ÆË¾÷Ã¢ ÆÐ³Î
    public GameObject KnockOutPanel;
    public void Start()
    {
        KnockOutPanel = GameObject.Find("KnockOutPanel");

        KnockOutPanel.SetActive(false);
    }

}
