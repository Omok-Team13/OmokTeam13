using UnityEngine.SceneManagement;
using UnityEngine;
using System;
using TMPro;

public class GameManage : Singleton<GameManage>
{
    //코드 담당자: 최은주
    [SerializeField] Canvas canvas;
    [SerializeField] GameObject noticeUI;   
  
    public delegate void OnCustom();
    public event OnCustom onCustom;
 
    public int gameNumber; // 1이 복싱 0이 오목

    public int loginCount; //로그인 되면 1, 로그인 아닐 시에 0   

    //스코어 매니저 참조해서 승패 팝업           

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
        SceneManager.LoadScene("Single Room");
    }    
    public void ChangeToIntroScene()
    {
        SceneManager.LoadScene("MergeIntro");
    }

    public void ChangeToMulti()
    {
        SceneManager.LoadScene("Multi Room");
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
        Application.Quit();
        //카운트 0으로 초기화
        loginCount = 0; 
    }
}
