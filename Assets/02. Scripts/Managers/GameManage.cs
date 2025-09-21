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
    public CameraController camController;
    public int loginCount; //로그인 되면 1, 로그인 아닐 시에 0   

    public GameObject player;
    public Transform startPos;
    public GameObject cube;
    public CamRotate cam;
    public DummyPlayerHealth playerHp;
    public DummyPlayerHealth_UI playerHpUi;

    //스코어 매니저 참조해서 승패 팝업           

    private void Awake()
    {
        onCustom += ChangeToGameScene;
        player = GameObject.FindWithTag("Player");
        camController = FindFirstObjectByType<CameraController>();
        cam = FindFirstObjectByType<CamRotate>();

        playerHp = player.GetComponent<DummyPlayerHealth>();
        playerHpUi = player.GetComponent<DummyPlayerHealth_UI>();

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

    public void MouseUnlock()
    {
        cam.MouseUnlock();
    }

    public void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        ResetGameState();
    }

    public void ResetGameState()
    {
        cube.SetActive(false);
        player.GetComponent<CharacterController>().enabled = false;
        player.transform.position = startPos.position;
        camController.SwitchCamera(CameraController.currCamState.EndOmok);
        player.GetComponent<CharacterController>().enabled = true;
        playerHp.HpPreset();
        playerHpUi.UIHpPreset();
    }

    public void OnApplicationQuit()
    {
        Application.Quit();
        //카운트 0으로 초기화
        loginCount = 0; 
    }
}
