using TMPro;
using UnityEngine;

public class PlayerState : MonoBehaviour
{
    public TextMeshPro nickName;
    //코드 담당자: 최은주

    bool isFirstP;

    Constants.PlayerType playerType;

    string sendName;
    //게임 모드 받아오기 

    //UI 패널 시리얼라이즈필드로 받아오기

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

    //public PlayerState(bool isFirstPlayer) //생성자 
    //{
    //    isFirstP = isFirstPlayer;
    //    playerType = isFirstP ? Constants.PlayerType.PlayerA : Constants.PlayerType.PlayerB;       
    //}

    #region 상태 메서드
    public void EnterGame()
    {
        GameManage.Instance.OpenNoticePanel("게임이 시작되었습니다.");
        //게임 시작하고 떠야하는 턴 정보 표시 UI. 

        //배틀 신청 버튼 UI
    }


    public void ExitGame()
    {
        //게임 나갔을 때
    }


    #endregion
}
