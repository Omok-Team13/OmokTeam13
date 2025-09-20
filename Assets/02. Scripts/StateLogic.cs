using Controller;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StateLogic : SIngleton2<StateLogic>
{
    public enum GameState { EnterOmok, EnterBoxing, EndOmok, EndBoxing, Restart }
    public GameState gameState;

    [SerializeField] GameObject omokBoard;
    [SerializeField] Transform omokPos;

    [SerializeField] Canvas canvas;
    [SerializeField] GameObject boxingArena;
    [SerializeField] GameObject omokRoom;
    [SerializeField] GameObject winnerUI;
    [SerializeField] GameObject omokBoardUI;

    [SerializeField] GameObject Hpabar;
    [SerializeField] GameObject keyNotice;

    [SerializeField] GameObject scoreUI;
    [SerializeField] GameObject roundUI;
    [SerializeField] GameObject emotionUI;

    [SerializeField] TextMeshProUGUI scoreText;
    [SerializeField] TextMeshProUGUI battleOn; //배틀기회
    [SerializeField] TextMeshProUGUI roundText;

    [SerializeField] TextMeshProUGUI Aname;
    [SerializeField] TextMeshProUGUI Bname;

    [SerializeField] Button startButton;
    [SerializeField] Transform omokStartPos;

    [SerializeField] GameObject chair;
    [SerializeField] Transform chairResetPos;
    [SerializeField] Transform chairOriginPos;
    
    
    public GameObject playUI;

    string AplayerName; //첫번째 플레이어의 닉네임
    string BplayerName = "알파고";

    public int AplayerScore;
    public int BplayerScore;

    public int currRound = 0;

    int battleCount = 1; //남은 배틀 기회 (기본 1회)
    public bool isGameEnd; //스코어 매니저와 연동해서 스코어가 최종적으로 값을 넘겼는지 확인
    public bool isRestart;
    public bool isOmok;
    public bool isWithdraw; //오목 배틀이 끝나기 전에 배틀 신청한 경우
    //public bool  //매 라운드 종료

    CameraController cameraController;
    BoardController_Omok omok;
    FightManager fightManager;
    GameObject player;
    CharacterMover playerMove;

    CharacterController cc;
    Vector3 center;
    Animator playerAnim;



    private void Start()
    {
        battleOn.text = "";

        AplayerScore = 0;
        BplayerScore = 0;

        omok = FindFirstObjectByType<BoardController_Omok>();            
        fightManager = FindFirstObjectByType<FightManager>();
        cameraController = FindFirstObjectByType<CameraController>();

        //player = GameObject.FindWithTag("Player");
        //playerMove = player.GetComponent<CharacterMover>();

        StartCoroutine(NameSetting());
        Bname.text = BplayerName;
    }

     public void GetName(string name)
    {
        AplayerName = name;       
    }

    IEnumerator NameSetting()
    {
        yield return new WaitForSeconds(2f);
        AplayerName = PlayerPrefs.GetString("UserName");
        Aname.text = AplayerName;
    }

    public void RestartOmokfromOmok()
    {
        chair.transform.position = chairOriginPos.position;
        playerAnim.SetTrigger("Idle");
        SetState(GameState.EnterOmok);
    }

    public void RoundScore(int round, bool isRestart, bool isBoxing)
    {
        if(!isBoxing) //복싱한게 아닌 경우만 라운드 추가             
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
        if(currRound >= 3)
        {
            roundUI.gameObject.SetActive(true);
            roundText.text = $"라운드 {currRound}";
            yield return new WaitForSeconds(1f);
            roundUI.gameObject.SetActive(false);
            if (!isGameEnd)
            {
                omok.StartGame();
            }
        }        
        if(currRound >= 2)
        {
            yield return new WaitForSeconds(2f);
            roundUI.gameObject.SetActive(true);
            roundText.text = $"라운드 {currRound}";
            yield return new WaitForSeconds(1f);
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

    public void SetState(GameState state)
    {
        GameObject player = GameObject.FindWithTag("Player");
        Animator playerAnim = player.GetComponent<Animator>();

        Debug.Log($"[StateLogic] SetState 호출됨: {state}");
        this.gameState = state;

        switch(state)
        {
            case GameState.EnterOmok:
                isOmok = true;
                isGameEnd = false;                
                omokBoardUI.SetActive(true);
                playUI.SetActive(true);
                emotionUI.SetActive(true);
                cameraController.SwitchCamera(CameraController.currCamState.EnterOmok);
                StartCoroutine(RoundAppear());
                StartCoroutine(battleNotice());
                break;
            case GameState.EnterBoxing:
                cc = player.GetComponent<CharacterController>();
                center = cc.center;
                StartCoroutine(StartBoxing());                
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
                    cameraController.SwitchCamera(CameraController.currCamState.OmokWinner);
                    chair.transform.position = chairResetPos.position;
                    emotionUI.SetActive(false);                    
                }
                break;
            case GameState.EndBoxing:                                                     
                if(isGameEnd)
                {
                    cameraController.SwitchCamera(CameraController.currCamState.BoxingWinner);                                
                }
                if(!isGameEnd)
                {
                    StartCoroutine(EndBoxingState());
                    cameraController.SwitchCamera(CameraController.currCamState.EndOmok);
                }                
                break;
            case GameState.Restart:
                StartCoroutine(RestartOmokFromBoxing());
                cameraController.SwitchCamera(CameraController.currCamState.EndOmok);
                break;

        }
    }

    IEnumerator RestartOmokFromBoxing()
    {
        isOmok = true;
        isGameEnd = false;
        playerAnim.SetTrigger("Idle");
        Hpabar.SetActive(false);
        keyNotice.SetActive(false);
        omokBoard.transform.position = omokPos.position;               
        StartCoroutine(fightManager.EndBoxing());        
        player = GameObject.FindWithTag("Player");
        CharacterController cc = player.GetComponent<CharacterController>();
        cc.enabled = false;
        player.transform.position = omokStartPos.position;
        omokRoom.SetActive(true);
        boxingArena.SetActive(false);
        cc.enabled = true;
        FindFirstObjectByType<WallAnimControll>()?.ResetWallsAnim();
        yield return new WaitForSeconds(2f);
    }
    IEnumerator StartBoxing() //복싱 시작
    {
        isOmok = false;    
        isGameEnd = false;
        emotionUI.SetActive(false);
        omokBoardUI.SetActive(false);        
        cameraController.SwitchCamera(CameraController.currCamState.EnterBoxing);
        boxingArena.SetActive(true);
        playUI.SetActive(false);        
        omokRoom.SetActive(false);        

        yield return new WaitForSeconds(3.0f);       
        center.y = 0.95f;
        cc.center = center;
        Hpabar.SetActive(true);
        keyNotice.SetActive(true);
    }  
    IEnumerator EndBoxingState() //복싱 끝
    {
        
        omokBoard.transform.position = omokPos.position;
        //playerMove.isBoxing = false;
        Hpabar.SetActive(false);
        keyNotice.SetActive(false);
        yield return new WaitForSeconds(2f);
        StartCoroutine(fightManager.EndBoxing());
        yield return new WaitForSeconds(2f);
        player = GameObject.FindWithTag("Player");
        CharacterController cc = player.GetComponent<CharacterController>();
        cc.enabled = false;
        player.transform.position = omokStartPos.position;
        omokRoom.SetActive(true);
        scoreUI.SetActive(true);
        boxingArena.SetActive(false);
        FindFirstObjectByType<WallAnimControll>()?.ResetWallsAnim();

        cc.enabled = true;
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

    public void CheckScore(int Ascore, int Bscore, string winner, bool isBoxing) //최종 스코어 결정
    {          
        RoundScore(1, false, isBoxing);
        AplayerScore += Ascore;
        BplayerScore += Bscore;

        scoreText.text = $"{AplayerName} {AplayerScore} vs {BplayerName} {BplayerScore}";

        if (AplayerScore >= 2 || BplayerScore >= 2) //A나 B 중 누구라도 2점을 먼저 딴다면, 게임 끝남
        {
            isGameEnd = true;
            if(!isBoxing)
            {                
                if (winner == "플레이어")
                {
                    OpenFinalWinner(AplayerName, false);
                    playerAnim.SetTrigger("Dance");
                }

                if (winner == "컴퓨터")
                {
                    OpenFinalWinner(BplayerName, false); //승패팝업
                    //절망하는 애니메이션
                }

                SetState(GameState.EndOmok);
            }
            else if(isBoxing)
            {                               
                if(winner == AplayerName)
                {
                    OpenFinalWinner(AplayerName, true);
                    playerAnim.SetTrigger("Dance");
                }
                if(winner == BplayerName)
                {
                    OpenFinalWinner(BplayerName, true);
                    //사망 애니메이션
                }
                SetState(GameState.EndBoxing);
            }                   
        }

        else if (AplayerScore > 0 || BplayerScore > 0) //동점 (1대1)이거나 누가 1점 앞서는 상황
        {
            //isGameEnd = false;

            if (isBoxing)
            {
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
                    if (winner == AplayerName)
                        OpenWinnerNotice(AplayerName);
                    else if (winner == BplayerName)
                        OpenWinnerNotice(BplayerName);
                }
                SetState(GameState.EndBoxing);
            }
            if(isOmok)
            {
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
                SetState(GameState.EnterOmok);
            }      
        }
        else
        {
            isGameEnd = false;         
        }                    
    }

    public void CheckHP(bool isADead, bool isBDead)
    {
        if (isADead) //플레이어가 사망했을 때
        {
            CheckScore(0, 1, BplayerName, true);            
        }
        if (isBDead)
        {
            CheckScore(1, 0, AplayerName, true);
        }      
    }

    public void OpenWinnerNotice(string message)
    {
        if (canvas != null)
        {
            var winnerPanel = Instantiate(winnerUI, canvas.transform);
            winnerPanel.GetComponent<PopUpPanel>().WinnerNotice(message);
            StartCoroutine(winnerPanel.GetComponent<PopUpPanel>().Hide());        
        }
    }

    public void OpenFinalWinner(string message, bool isBoxing) //최종 승자 
    {
        startButton.gameObject.SetActive(false);
        if (canvas != null && !isBoxing)
        {
            
            Debug.Log($"승자는 {message} 입니다.");
            var finalWinner = Instantiate(winnerUI, canvas.transform);
            finalWinner.GetComponent<PopUpPanel>().finalWinnerNotice(message);                          
        }
        if(canvas != null && isBoxing)
        {
            Debug.Log($"승자는 {message} 이며 오목룸으로 돌아갑니다.");
            var boxingWinner = Instantiate(winnerUI, canvas.transform);
            boxingWinner.GetComponent<PopUpPanel>().finalBoxingWinner(message);
        }
    }
}
