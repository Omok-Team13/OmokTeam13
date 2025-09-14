using UnityEngine;

public class PlayerState : MonoBehaviour
{
    //코드 담당자: 최은주

    bool isFirstP;

    Constants.PlayerType playerType;
    //게임 모드 받아오기 

    //UI 패널 시리얼라이즈필드로 받아오기

    public PlayerState(bool isFirstPlayer) //생성자 
    {
        isFirstP = isFirstPlayer;
        playerType = isFirstP ? Constants.PlayerType.PlayerA : Constants.PlayerType.PlayerB;
    }

    #region 상태 메서드
    public void EnterGame()
    {
        //게임 시작하고 떠야하는 턴 정보 표시 UI. 

        //배틀 신청 버튼 UI
    }


    public void ExitGame()
    {
        //게임 나갔을 때
    }

    public void HandleMove() //수 놓기
    {

    }

    public void NextTurn() //턴 넘기기 
    {

    }

    #endregion
}
