using TMPro;
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

    [SerializeField] TextMeshProUGUI scoreText;
    [SerializeField] TextMeshProUGUI battleOn;
  
    public GameObject playUI;

    string AplayerName; //첫번째 플레이어의 닉네임
    string BplayerName = "알파고";

    int AplayerScore;
    int BplayerScore;

    int battleCount = 1; //남은 배틀 기회 (기본 1회)
    bool isGameEnd; //스코어 매니저와 연동해서 스코어가 최종적으로 값을 넘겼는지 확인

    PlayerState player;
    BoardController_Omok omok;
    FightManager fightManager;


    private void Awake()
    {
        fightManager = FindFirstObjectByType<FightManager>();        
        omok = FindFirstObjectByType<BoardController_Omok>();
        player = FindFirstObjectByType<PlayerState>();
    }

    private void Update()
    {
        //scoreText.text = $"A플레이어 {AplayerScore} vs B플레이어 {BplayerScore}";
    }
    public void GetName(string name)
    {
        AplayerName = name;
    }

    public void SetState(GameState state)
    {
        Debug.Log($"[StateLogic] SetState 호출됨: {state}");
        this.gameState = state;

        switch(state)
        {
            case GameState.EnterOmok:                
                break;
            case GameState.EnterBoxing:
                boxingArena.SetActive(true);
                omokRoom.SetActive(false);
                break;
            case GameState.EndOmok:
                if (isGameEnd)
                {
                    omokBoardUI.SetActive(false);                  
                }
                break;
            case GameState.EndBoxing:
                SetState(GameState.EnterOmok);
                omokRoom.SetActive(true);
                boxingArena.SetActive(false);
                break;
        }
    }

    public void turnOffBattleButton()
    {        
        if (battleCount == 1) //결투 신청 누른다면
        {
            battleOn.text = $"복싱 대결을 신청할 수 있는 기회는 {battleCount}회 입니다.";
            battleCount -= 1;

            StartCoroutine(fightManager.AIplayerAppear()); //복싱장 들어가고 AI 나타나기
        }
        if (battleCount == 0)
        {
            battleOn.text = $"복싱 대결 기회를 모두 소진했습니다.";
            //배틀 버튼 더 이상 나오지 않게 
        }
    }

    public void CheckScore(int Ascore, int Bscore) //추후 스코어 매니저로 통합, 최종 스코어 결정
    {
        //추후 스코어 매니저에게 값 전달...        

        AplayerScore += Ascore;
        BplayerScore += Bscore;

        if (AplayerScore >= 2 || BplayerScore >= 2) //A나 B 중 누구라도 2점을 먼저 딴다면
        {
            isGameEnd = true;
            var nextState = GameManager.Instance.GetState(3);
            SetState(nextState);

            if (AplayerScore > BplayerScore)
                OpenFinalWinner(AplayerName);

            if (BplayerScore > AplayerScore)
                OpenFinalWinner(BplayerName); //승패팝업
        }

        else if (AplayerScore > 0 || BplayerScore > 0) //동점 (1대1)이거나 누가 1점 앞서는 상황
        {
            isGameEnd = false;

            if (AplayerScore > BplayerScore)
                OpenWinnerNotice(AplayerName);

            if (BplayerScore > AplayerScore)
                OpenWinnerNotice(BplayerName);

            omok.StartGame(); //게임 재시작
        }
        else
        {
            isGameEnd = false;
            omok.StartGame();
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
        if (canvas != null)
        {
            Debug.Log($"승자는 {message} 입니다.");
            var finalWinner = Instantiate(winnerUI, canvas.transform);
            finalWinner.GetComponent<WinnerPanel>().finalWinnerNotice(message);
            StartCoroutine(finalWinner.GetComponent<WinnerPanel>().Hide());
        }
    }
}
