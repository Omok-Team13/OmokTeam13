using TMPro;
using UnityEngine;

public class PlayerState : MonoBehaviour
{
    public TextMeshPro nickName;
    //코드 담당자: 최은주 

    string sendName;

    private void Awake()
    {
        nickName.text = "";
    }
    private void Start()
    {
        nameSet(sendName);
    }

    public void nameSet(string message)
    {
        nickName.text = message;
        sendName = message;
        StateLogic.Instance.GetName(sendName);
    }
}
