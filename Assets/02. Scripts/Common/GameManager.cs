using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using System;
using TMPro;

public class GameManager : Singleton<GameManager>
{
    //코드 담당자: 최은주
    [SerializeField] GameObject noticeUI;
    [SerializeField] GameObject boxingArena;
    [SerializeField] GameObject omokRoom;
  

    public delegate void OnCustom();
    public event OnCustom onCustom;

    public delegate void OnBoxing();
    public event OnBoxing startBoxing;

    bool isGameEnd; //스코어 매니저와 연동해서 스코어가 최종적으로 값을 넘겼는지 확인
    int roundScore = 0;

    public int loginCount; //로그인 되면 1, 로그인 아닐 시에 0
    Canvas canvas;

    //스코어 매니저 참조해서 승패 팝업 

    int AplayerScore;
    int BplayerScore; 

    BoardController_Omok omok;

    Constants.GameType gameT;

    private void Awake()
    {
        canvas = FindFirstObjectByType<Canvas>();
        onCustom += ChangeToGameScene;
    }

    //public void ChangeToSinglePlay(Constants.GameType gameType)
    //{
    //    gameT = gameType;
    //    SceneManager.LoadScene("Single Room");
    //}
   
    public void SinglePlay()
    {
        onCustom?.Invoke();
    }

    private void ChangeToGameScene() //게임씬으로
    {
        SceneManager.LoadScene("Single Room");
    }

    public void OpenNoticePanel(string message) //안내문구 팝업 인스턴스 생성
    {
        if(canvas != null)
        {
            var noticePanel = Instantiate(noticeUI, canvas.transform);
            noticePanel.GetComponent<NoticePanel>().Notice(message);          
            StartCoroutine(noticePanel.GetComponent<NoticePanel>().Hide());
        }
    }

    public void IntoBoxing()
    {     
        OpenNoticePanel("플레이어가 복싱을 신청했습니다. 경기장으로 향합니다.");   
    }

    public void BackToOmok()
    {
        omokRoom.SetActive(true);
        boxingArena.SetActive(false);
    }

    public void WinorLosePanel()
    {
        //승패팝업
    }

    public void CheckScore(int Ascore, int Bscore) //추후 스코어 매니저로 통합
    {
        //추후 스코어 매니저에게 값 전달...
        AplayerScore += Ascore;
        BplayerScore += Bscore;

        if(AplayerScore >= 2 || BplayerScore >=2) //A나 B 중 누구라도 2점을 먼저 딴다면
        {
            WinorLosePanel(); //승패팝업
        }

        else if(AplayerScore == BplayerScore ) //동점 (1대1)
        {
            omok.StartGame(); //게임 재시작
        }       
    }

    protected override void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        //씬 로드
    }

    public void OnApplicationQuit()
    {
        //카운트 0으로 초기화
        loginCount = 0; 
    }
}
