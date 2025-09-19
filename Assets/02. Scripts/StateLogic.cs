using System.Collections;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class StateLogic : SIngleton2<StateLogic>
{
    public enum GameState { EnterOmok, EnterBoxing, EndOmok, EndBoxing, None }
    GameState gameState;

    [SerializeField] Canvas canvas;
    [SerializeField] GameObject boxingArena;
    [SerializeField] GameObject omokRoom;
    [SerializeField] GameObject winnerUI;
    [SerializeField] GameObject omokBoardUI;

    [SerializeField] GameObject Hpabar;
    [SerializeField] GameObject keyNotice;

    [SerializeField] GameObject scoreUI;
    [SerializeField] GameObject roundUI;  

    [SerializeField] TextMeshProUGUI scoreText;
    [SerializeField] TextMeshProUGUI battleOn; //배틀기회
    [SerializeField] TextMeshProUGUI roundText;

    [SerializeField] Button startButton;
    
    
    public GameObject playUI;
    public Transform playerStartPos;

    string AplayerName; //첫번째 플레이어의 닉네임
    string BplayerName = "알파고";

    int AplayerScore;
    int BplayerScore;

    int currRound = 0;

    int battleCount = 1; //남은 배틀 기회 (기본 1회)
    public bool isGameEnd; //스코어 매니저와 연동해서 스코어가 최종적으로 값을 넘겼는지 확인
    public bool isRestart;
    public bool isOmok;

    CameraController cameraController;
    BoardController_Omok omok;
    FightManager fightManager;
    GameObject player;
    

    private void Start()
    {
        battleOn.text = "";

        AplayerScore = 0;
        BplayerScore = 0;

        omok = FindFirstObjectByType<BoardController_Omok>();
        player = GameObject.FindWithTag("Player");              
        fightManager = FindFirstObjectByType<FightManager>();
        cameraController = FindFirstObjectByType<CameraController>();

        StartCoroutine(NameSetting());
    }

     public void GetName(string name)
    {
        AplayerName = name;       
    }

    IEnumerator NameSetting()
    {
        yield return new WaitForSeconds(2f);
        AplayerName = PlayerPrefs.GetString("UserName");       
    }

    public void RoundScore(int round, bool isRestart)
    {
        currRound += round;
        if (currRound >= 3 && isRestart) // 재시작인 경우 초기화
        {
            currRound = 1;
            AplayerScore = 0;
            BplayerScore = 0;
            scoreText.text = $"{AplayerName} {AplayerScore} vs {BplayerName} {BplayerScore}";
            omok.StartGame();            
        }
    }

    IEnumerator RoundAppear()
    {    
        scoreUI.SetActive(true);
        scoreText.text = $"{AplayerName} {AplayerScore} vs {BplayerName} {BplayerScore}";
        if(currRound >= 2)
        {
            yield return new WaitForSeconds(2f);
            roundUI.gameObject.SetActive(true);
            roundText.text = $"라운드 {currRound}";
            yield return new WaitForSeconds(2f);
            roundUI.gameObject.SetActive(false);
            if(!isGameEnd)
            {
                omok.StartGame();
            }
        }
        if(currRound >= 0)
        {
            roundUI.gameObject.SetActive(true);
            roundText.text = $"라운드 {currRound}";
            yield return new WaitForSeconds(2f);
            roundUI.gameObject.SetActive(false);        
        }        
    }

    void StartBoxing() //복싱 시작
    {
        Hpabar.SetActive(true);
        //keyNotice.SetActive(true);
        isOmok = false;
        //player.GetComponent<Animator>().SetTrigger("Stand");
        cameraController.SwitchCamera(CameraController.currCamState.EnterBoxing);
        isGameEnd = false;
        omokBoardUI.SetActive(false);
        playUI.SetActive(false);
        boxingArena.SetActive(true);
        omokRoom.SetActive(false);
    }


    public void SetState(GameState state)
    {
        Debug.Log($"[StateLogic] SetState 호출됨: {state}");
        this.gameState = state;

        switch(state)
        {
            case GameState.EnterOmok:
                isOmok = true;
                cameraController.SwitchCamera(CameraController.currCamState.EnterOmok);
                isGameEnd = false;                
                omokBoardUI.SetActive(true);
                playUI.SetActive(true);
                StartCoroutine(RoundAppear());
                StartCoroutine(battleNotice());
                break;
            case GameState.EnterBoxing:
                //anim.SetTrigger("Stand");
                //복싱 시작되면 3초 간 기다리기
                //캐릭터 움직임 꺼졋다가 켜주기
                //HP바 UI 키기
                StartBoxing();                
                StartCoroutine(fightManager.AIplayerAppear()); //복싱장 들어가고 AI 나타나기
                break;
            case GameState.EndOmok:
                startButton.gameObject.SetActive(false);
                if (isGameEnd)
                {
                    player.GetComponent<CharacterController>().enabled = true;
                    omokBoardUI.SetActive(false);                    
                    scoreUI.SetActive(false);
                    startButton.gameObject.SetActive(false);
                    cameraController.SwitchCamera(CameraController.currCamState.EndOmok);
                }
                break;
            case GameState.EndBoxing:
                SetState(GameState.EnterOmok);
                omokRoom.SetActive(true);
                boxingArena.SetActive(false);
                cameraController.SwitchCamera(CameraController.currCamState.EndOmok);
                break;
        }
    }
  
     public void turnOffBattleButton(int battleChance) //배틀기회
    {        

        if (battleCount == 1) //결투 신청 누른다면
        {
            StartCoroutine(battleNotice());
            battleCount -= 1;
            
        }
        if (battleCount == 0)
        {
            battleOn.text = $"복싱 대결 기회를 모두 소진했습니다.";
            //배틀 버튼 더 이상 나오지 않음
        }
    }
    IEnumerator battleNotice()
    {
        yield return new WaitForSeconds(2f);
        battleOn.text = $"남은 복싱 기회 : {battleCount}회";        
        yield return new WaitForSeconds(5f);
        battleOn.gameObject.SetActive(false);
    }

    public void CheckScore(int Ascore, int Bscore, string winner) //추후 스코어 매니저로 통합, 최종 스코어 결정
    {
        //추후 스코어 매니저에게 값 전달...        
        RoundScore(1, false);
        AplayerScore += Ascore;
        BplayerScore += Bscore;

        scoreText.text = $"{AplayerName} {AplayerScore} vs {BplayerName} {BplayerScore}";

        if (AplayerScore >= 2 || BplayerScore >= 2) //A나 B 중 누구라도 2점을 먼저 딴다면
        {
            isGameEnd = true;
            var nextState = GameManage.Instance.GetState(3);
            SetState(nextState);

            if (winner == "플레이어")
                OpenFinalWinner(AplayerName);

            if (winner == "컴퓨터")
                OpenFinalWinner(BplayerName); //승패팝업
        }

        else if (AplayerScore > 0 || BplayerScore > 0) //동점 (1대1)이거나 누가 1점 앞서는 상황
        {
            isGameEnd = false;

            if (AplayerScore > BplayerScore) //A가 1대0
            {
                OpenWinnerNotice(AplayerName);
            }
            else if (BplayerScore > AplayerScore) //B가 1대0
            {
                OpenWinnerNotice(BplayerName);
            }
            else // 동점 상황
            {
                if (winner == "플레이어")
                    OpenWinnerNotice(AplayerName);
                else if (winner == "컴퓨터")
                    OpenWinnerNotice(BplayerName);
            }

            SetState(GameState.EnterOmok); //게임 재시작            
            //omok.StartGame();
        }
        else
        {
            isGameEnd = false;
            //currRound++;
            //SetState(GameState.EnterOmok); //게임 재시작
            //omok.StartGame();
        }                    
    }

    public void OpenWinnerNotice(string message)
    {
        if (canvas != null)
        {
            var winnerPanel = Instantiate(winnerUI, canvas.transform);
            winnerPanel.GetComponent<WinnerPanel>().WinnerNotice(message);
            StartCoroutine(winnerPanel.GetComponent<WinnerPanel>().Hide());        
        }
    }

    public void OpenFinalWinner(string message) //최종 승자 
    {
        startButton.gameObject.SetActive(false);
        if (canvas != null)
        {
            Debug.Log($"승자는 {message} 입니다.");
            var finalWinner = Instantiate(winnerUI, canvas.transform);
            finalWinner.GetComponent<WinnerPanel>().finalWinnerNotice(message);
           
        }
    }
}
