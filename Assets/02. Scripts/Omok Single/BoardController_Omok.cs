using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Threading.Tasks;

public class BoardController_Omok : MonoBehaviour
{
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private Transform gridContainer;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private Button startButton;
    [SerializeField] private Sprite blackStoneSprite;
    [SerializeField] private Sprite whiteStoneSprite;
    [SerializeField] private Button placeStoneButton;
    [SerializeField] private Sprite markSprite;
    [SerializeField] private Sprite forbiddenSprite;

    private const int BOARD_SIZE = 15;
    private const int AI_PLAYER = 2;
    // (수정) 이 변수는 이제 BoardAI가 아닌 AITurn 함수에서 직접 관리합니다.
    // private const int AI_MAX_DEPTH = 3; 
    private const float AI_THINK_TIME = 4.0f; // AI의 생각 시간 (초)

    private BoardOmok gameBoard;
    private Cell_Omok[,] cells;
    private int selectedX = -1;
    private int selectedY = -1;
    private Cell_Omok lastMarkedCell = null;
    private bool isPlayerTurn = true;

    void Start()
    {
        startButton.onClick.AddListener(StartGame);
        placeStoneButton.onClick.AddListener(PlaceStone);        
    }

    public void StartGame()
    {
        GameManager.Instance.OpenNoticePanel("게임을 시작합니다.");

        gameBoard = new BoardOmok();
        statusText.text = "플레이어 (흑) 턴";
        startButton.gameObject.SetActive(false);
        placeStoneButton.gameObject.SetActive(false);
        isPlayerTurn = true;
        selectedX = -1;
        selectedY = -1;
        lastMarkedCell = null;

        foreach (Transform child in gridContainer) Destroy(child.gameObject);

        cells = new Cell_Omok[BOARD_SIZE, BOARD_SIZE];
        for (int i = 0; i < BOARD_SIZE; i++)
        {
            for (int j = 0; j < BOARD_SIZE; j++)
            {
                GameObject cellGO = Instantiate(cellPrefab, gridContainer);
                Cell_Omok cell = cellGO.GetComponent<Cell_Omok>();
                cell.SetUp(j, i, OnCellClicked);
                cells[i, j] = cell;
            }
        }
        UpdateForbiddenMarks();
        UpdateBoardVisuals();
    }

    void OnCellClicked(int x, int y)
    {
        if (!isPlayerTurn || gameBoard.IsGameOver() || gameBoard.GetCell(y, x) != 0) return;

        if (lastMarkedCell != null)
        {
            lastMarkedCell.SetMark(false, null);
        }
        cells[y, x].SetMark(true, markSprite);
        lastMarkedCell = cells[y, x];
        selectedX = x;
        selectedY = y;
        placeStoneButton.gameObject.SetActive(true);
    }

    void PlaceStone()
    {
        if (selectedX == -1 || selectedY == -1) return;
        var move = new Move_Omok(selectedX, selectedY);

        if (gameBoard.IsForbiddenMove(move))
        {
            StartCoroutine(ShowStatusMessage("금수 위치입니다. 다시 선택하세요."));
            if (lastMarkedCell != null) lastMarkedCell.SetMark(false, null);
            placeStoneButton.gameObject.SetActive(false);
            selectedX = -1;
            selectedY = -1;
            return;
        }

        isPlayerTurn = false;
        gameBoard = (BoardOmok)gameBoard.MakeMove(move);
        UpdateBoardVisuals();

        if (lastMarkedCell != null) lastMarkedCell.SetMark(false, null);
        placeStoneButton.gameObject.SetActive(false);
        selectedX = -1;
        selectedY = -1;

        if (CheckForGameOver()) return;
        StartCoroutine(AITurn());
    }

    /// <summary>
    /// (수정) 새로운 비동기 방식의 AI를 호출하도록 변경합니다.
    /// </summary>
    IEnumerator AITurn()
    {
        statusText.text = "컴퓨터가 생각 중입니다...";
        // AI가 생각하는 동안 플레이어가 클릭하지 못하도록 즉시 턴을 넘김
        isPlayerTurn = false;

        // 비동기 Task를 시작하고 Coroutine에서 완료를 기다립니다.
        Task<Move> aiTask = BoardAI.FindBestMoveAsync(gameBoard, AI_THINK_TIME);

        // Task가 끝날 때까지 매 프레임 대기 (UI가 멈추지 않음)
        while (!aiTask.IsCompleted)
        {
            yield return null;
        }

        // Task의 결과를 가져옵니다.
        Move bestMove = aiTask.Result;

        if (bestMove != null)
        {
            gameBoard = (BoardOmok)gameBoard.MakeMove(bestMove);
        }

        UpdateBoardVisuals();
        if (!CheckForGameOver())
        {
            // 게임이 끝나지 않았을 때만 플레이어 턴으로 전환하고 금수 표시를 업데이트합니다.
            isPlayerTurn = true;
            UpdateForbiddenMarks();
        }
    }

    void UpdateBoardVisuals()
    {
        for (int i = 0; i < BOARD_SIZE; i++)
        {
            for (int j = 0; j < BOARD_SIZE; j++)
            {
                int stone = gameBoard.GetCell(i, j);
                Sprite stoneSprite = null;
                if (stone == 1) stoneSprite = blackStoneSprite;
                else if (stone == 2) stoneSprite = whiteStoneSprite;
                cells[i, j].SetStone(stoneSprite);
            }
        }
    }

    void UpdateForbiddenMarks()
    {
        bool isBlackTurn = gameBoard.GetCurrentPlayer() == 1;
        for (int i = 0; i < BOARD_SIZE; i++)
        {
            for (int j = 0; j < BOARD_SIZE; j++)
            {
                if (gameBoard.GetCell(i, j) == 0 && isBlackTurn)
                {
                    bool isForbidden = gameBoard.IsForbiddenMove(new Move_Omok(j, i));
                    cells[i, j].SetForbidden(isForbidden, forbiddenSprite);
                }
                else
                {
                    cells[i, j].SetForbidden(false, null);
                }
            }
        }
    }

    public bool CheckForGameOver()
    {
        int winner = gameBoard.CheckWinner();

        if (winner == 0)
        {
            statusText.text = gameBoard.GetCurrentPlayer() == 1 ? "플레이어 (흑) 턴" : "컴퓨터 (백) 턴";
            return false;
        }

        if (winner == 3)
        {
            statusText.text = "무승부입니다!";
            StateLogic.Instance.CheckScore(0, 0);
        }

        else if (winner == 1)
        {
            statusText.text = "플레이어 (흑) 승리!";
            StateLogic.Instance.CheckScore(1, 0);
        }
        else
        {
            statusText.text = "컴퓨터 (백) 승리!";
            StateLogic.Instance.CheckScore(0, 1);
        }

        startButton.gameObject.SetActive(true);
        placeStoneButton.gameObject.SetActive(false);
        isPlayerTurn = false;
        return true;
    }

    IEnumerator ShowStatusMessage(string message)
    {
        string originalText = statusText.text;
        statusText.text = message;
        yield return new WaitForSeconds(1.5f);
        if (statusText.text == message) statusText.text = originalText;
    }
}