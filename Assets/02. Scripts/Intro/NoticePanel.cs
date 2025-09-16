using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NoticePanel : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI messageText;

    Canvas canvas;

    public void Notice(string message)
    {
        messageText.text = message;
        
    }
    public IEnumerator Hide()
    {
        yield return new WaitForSeconds(2f);
        this.gameObject.SetActive(false);
    }
}
