using UnityEngine.SceneManagement;
using UnityEngine;
using System;
using TMPro;

public enum State { Boxing, Omok }


public class GameManager : Singleton<GameManager>
{
    //코드 담당자: 최은주
    [SerializeField] GameObject boxingArena;
    [SerializeField] GameObject noticeUI;   
    [SerializeField] GameObject omokRoom;
    [SerializeField] TextMeshProUGUI scoreText;
    [SerializeField] TextMeshProUGUI battleCount;

    public delegate void OnCustom();
    public event OnCustom onCustom;

    public delegate void OnBoxing();
    public event OnBoxing startBoxing;

    bool isGameEnd; //스코어 매니저와 연동해서 스코어가 최종적으로 값을 넘겼는지 확인
    int roundScore = 0;

    int boxingCount = 1;

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
        omok = FindFirstObjectByType<BoardController_Omok>();
    }

    //public void ChangeToSinglePlay(Constants.GameType gameType)
    //{
    //    gameT = gameType;
    //    SceneManager.LoadScene("Single Room");
    //}

    private void Update()
    {
        scoreText.text = $"A플레이어 {AplayerScore} vs B플레이어 {BplayerScore}";
    }

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

    public void IntoBoxing(int state)
    {     
        if(state == 1) //결투 신청 누른다면
        {
            battleCount.text = $"복싱 대결을 신청할 수 있는 기회는 {state}회 입니다.";
            boxingCount -= state;
        }        
        if(boxingCount == 0)
        {
            battleCount.text = $"복싱 대결 기회를 모두 소진했습니다.";
            //배틀 버튼 더 이상 나오지 않게 
        }
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
        else if(AplayerScore > 0 ||  BplayerScore > 0) //동점 (1대1)
        {
            omok.StartGame(); //게임 재시작
        }
        else
        {
            omok.StartGame();
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
