using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class BoardController_Omok : MonoBehaviour
{
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private Transform gridContainer;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Sprite blackStoneSprite;
    [SerializeField] private Sprite whiteStoneSprite;

    // --- '선택 후 착수' 방식을 위한 UI 요소 ---
    [SerializeField] private Button placeStoneButton; // '착수' 버튼
    [SerializeField] private Sprite markSprite;       // 선택 위치에 표시할 이미지 (스프라이트)

    private const int BOARD_SIZE = 15;
    private const int AI_PLAYER = 2;
    private const int AI_MAX_DEPTH = 3;

    private BoardOmok gameBoard;
    private Cell_Omok[,] cells = new Cell_Omok[BOARD_SIZE, BOARD_SIZE];

    // --- 선택한 위치를 저장할 변수들 ---
    private int selectedX = -1;
    private int selectedY = -1;
    private Cell_Omok lastMarkedCell = null; // 이전에 마크했던 셀을 기억
    private bool isPlayerTurn = true;

    void Start()
    {
        restartButton.onClick.AddListener(StartGame);
        placeStoneButton.onClick.AddListener(PlaceStone); // '착수' 버튼에 PlaceStone 함수 연결
        StartGame();
    }

    void StartGame()
    {
        gameBoard = new BoardOmok();
        statusText.text = "플레이어 (흑) 턴";
        restartButton.gameObject.SetActive(false);
        placeStoneButton.gameObject.SetActive(false); // 게임 시작 시 '착수' 버튼 숨기기
        isPlayerTurn = true;
        selectedX = -1;
        selectedY = -1;
        lastMarkedCell = null;

        foreach (Transform child in gridContainer) Destroy(child.gameObject);

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
        UpdateBoardVisuals();
    }

    /// <summary>
    /// 이 함수는 이제 돌을 놓지 않고 '선택'하는 역할만 합니다.
    /// </summary>
    void OnCellClicked(int x, int y)
    {
        if (!isPlayerTurn || gameBoard.IsGameOver() || gameBoard.GetCell(y, x) != 0) return;

        // 이전에 선택했던 표시가 있다면 지웁니다.
        if (lastMarkedCell != null)
        {
            lastMarkedCell.SetMark(false, null);
        }

        // 새로 클릭한 위치에 표시합니다.
        cells[y, x].SetMark(true, markSprite);
        lastMarkedCell = cells[y, x];

        // 선택한 좌표를 저장합니다.
        selectedX = x;
        selectedY = y;

        // '착수' 버튼을 보여줍니다.
        placeStoneButton.gameObject.SetActive(true);
    }

    /// <summary>
    /// '착수' 버튼을 눌렀을 때 실행되는 함수입니다.
    /// </summary>
    void PlaceStone()
    {
        if (selectedX == -1 || selectedY == -1) return;

        var move = new Move_Omok(selectedX, selectedY);

        if (gameBoard.IsForbiddenMove(move))
        {
            StartCoroutine(ShowStatusMessage("금수 위치입니다. 다시 선택하세요."));
            // 선택 표시와 착수 버튼을 숨깁니다.
            if (lastMarkedCell != null) lastMarkedCell.SetMark(false, null);
            placeStoneButton.gameObject.SetActive(false);
            return;
        }

        isPlayerTurn = false;
        gameBoard = (BoardOmok)gameBoard.MakeMove(move);
        UpdateBoardVisuals();

        // 선택 관련 변수들을 초기화합니다.
        if (lastMarkedCell != null) lastMarkedCell.SetMark(false, null);
        placeStoneButton.gameObject.SetActive(false);
        selectedX = -1;
        selectedY = -1;

        if (CheckForGameOver()) return;

        StartCoroutine(AITurn());
    }

    IEnumerator AITurn()
    {
        statusText.text = "컴퓨터가 생각 중입니다...";
        yield return new WaitForSeconds(0.5f);

        // (수정) 'FindBestMove' 호출을 기존 'Negamax' 메소드를 사용하도록 변경하여 오류를 해결합니다.
        Move bestMove = null;
        BoardAI.Negamax(gameBoard, AI_MAX_DEPTH, Mathf.NegativeInfinity, Mathf.Infinity, ref bestMove);

        if (bestMove != null) gameBoard = (BoardOmok)gameBoard.MakeMove(bestMove);
        UpdateBoardVisuals();
        if (!CheckForGameOver()) isPlayerTurn = true;
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

    bool CheckForGameOver()
    {
        int winner = gameBoard.CheckWinner();
        if (winner == 0)
        {
            statusText.text = gameBoard.GetCurrentPlayer() == 1 ? "플레이어 (흑) 턴" : "컴퓨터 (백) 턴";
            return false;
        }
        if (winner == 3) statusText.text = "무승부입니다!";
        else if (winner == 1) statusText.text = "플레이어 (흑) 승리!";
        else statusText.text = "컴퓨터 (백) 승리!";
        restartButton.gameObject.SetActive(true);
        placeStoneButton.gameObject.SetActive(false);
        isPlayerTurn = false;
        return true;
    }

    IEnumerator ShowStatusMessage(string message)
    {
        string originalText = statusText.text;
        statusText.text = message;
        yield return new WaitForSeconds(1.5f);
        statusText.text = originalText;
    }
}