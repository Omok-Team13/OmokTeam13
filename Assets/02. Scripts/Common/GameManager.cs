using UnityEngine.SceneManagement;
using UnityEngine;
using System;
using TMPro;

public enum State { Boxing, Omok }

public class GameManager : Singleton<GameManager>
{
    //코드 담당자: 최은주
    [SerializeField] Canvas canvas;
    [SerializeField] GameObject noticeUI;   
  
    public delegate void OnCustom();
    public event OnCustom onCustom;

    bool isGameEnd; //스코어 매니저와 연동해서 스코어가 최종적으로 값을 넘겼는지 확인
    int roundScore = 0;

    public int gameNumber; // 1이 복싱 0이 오목

    public int loginCount; //로그인 되면 1, 로그인 아닐 시에 0   

    //스코어 매니저 참조해서 승패 팝업 
  
    
    Constants.GameType gameT;

    private void Awake()
    {
        onCustom += ChangeToGameScene;        
    } 

    public void SinglePlay()
    {
        onCustom?.Invoke();
    }

    private void ChangeToGameScene() //게임씬으로
    {
        SceneManager.LoadScene("Single Room 1");        
    }    

    public StateLogic.GameState GetState(int state)
    {
        switch(state)
        {
            case 0: return StateLogic.GameState.EnterOmok;
            case 1: return StateLogic.GameState.EnterBoxing;
            case 2: return StateLogic.GameState.EndBoxing;
            case 3: return StateLogic.GameState.EndOmok;
            default: return StateLogic.GameState.None;
        }    
    }
    public void OpenNoticePanel(string message) //안내문구 팝업 인스턴스 생성
    {
        if (canvas != null)
        {
            var noticePanel = Instantiate(noticeUI, canvas.transform);
            noticePanel.GetComponent<NoticePanel>().Notice(message);
            StartCoroutine(noticePanel.GetComponent<NoticePanel>().Hide());
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
