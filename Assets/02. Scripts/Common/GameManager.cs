using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using System;

public class GameManager : Singleton<GameManager>
{
    //코드 담당자: 최은주
    [SerializeField] GameObject noticeUI;

    public delegate void OnCustom();
    public event OnCustom onCustom;

    public int loginCount; //로그인 되면 1, 로그인 아닐 시에 0
    Canvas canvas;

    //스코어 매니저 참조해서 승패 팝업 

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
