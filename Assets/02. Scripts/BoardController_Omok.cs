using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System;
using System.Collections.Generic;

public class BoardController_Omok : MonoBehaviour
{
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private Transform boardPanel;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Sprite blackStoneSprite;
    [SerializeField] private Sprite whiteStoneSprite;
    [SerializeField] private Sprite forbiddenSprite;

    [SerializeField] private Button placeStoneButton;
    [SerializeField] private Sprite markSprite;

    private int selectedX = -1;
    private int selectedY = -1;
    private Cell_Omok lastMarkedCell = null;

    private const int BOARD_SIZE = 19;
    private const int AI_PLAYER = 2;
    private const int AI_MAX_DEPTH = 3;

    private BoardOmok gameBoard;
    private Cell_Omok[,] cells = new Cell_Omok[BOARD_SIZE, BOARD_SIZE];

    public static Action startAction;

    void Start()
    {
        restartButton.onClick.AddListener(StartGame);
        placeStoneButton.onClick.AddListener(PlaceStone);
        if (startAction == null) startAction += StartGame;
        StartGame();
    }

    void OnDestroy()
    {
        startAction -= StartGame;
    }

    void StartGame()
    {
        gameBoard = new BoardOmok();
        statusText.text = "플레이어 (흑) 턴";
        restartButton.gameObject.SetActive(false);
        placeStoneButton.gameObject.SetActive(false);

        foreach (Transform child in boardPanel) Destroy(child.gameObject);

        for (int i = 0; i < BOARD_SIZE; i++)
        {
            for (int j = 0; j < BOARD_SIZE; j++)
            {
                GameObject cellGO = Instantiate(cellPrefab, boardPanel);
                Cell_Omok cell = cellGO.GetComponent<Cell_Omok>();
                cell.SetUp(j, i, OnCellClicked);
                cells[i, j] = cell;
            }
        }
        UpdateBoardVisuals();
    }

    void OnCellClicked(int x, int y)
    {
        if (gameBoard.GetCurrentPlayer() != 1 || gameBoard.IsGameOver() || gameBoard.GetCell(y, x) != 0) return;

        if (lastMarkedCell != null)
        {
            lastMarkedCell.SetForbidden(false, null);
        }

        cells[y, x].SetForbidden(true, markSprite);
        lastMarkedCell = cells[y, x];

        selectedX = x;
        selectedY = y;

        placeStoneButton.gameObject.SetActive(true);
    }

    void PlaceStone()
    {
        if (selectedX == -1 || selectedY == -1) return;

        if (gameBoard.IsForbiddenMove(new Move_Omok(selectedX, selectedY)))
        {
            statusText.text = "금수 위치입니다. 다시 선택하세요.";
            lastMarkedCell.SetForbidden(false, null);
            placeStoneButton.gameObject.SetActive(false);
            return;
        }

        gameBoard = (BoardOmok)gameBoard.MakeMove(new Move_Omok(selectedX, selectedY));

        if (lastMarkedCell != null) lastMarkedCell.SetForbidden(false, null);
        selectedX = -1;
        selectedY = -1;
        placeStoneButton.gameObject.SetActive(false);

        UpdateBoardVisuals();

        if (CheckForGameOver()) return;
        StartCoroutine(AITurn());
    }

    IEnumerator AITurn()
    {
        statusText.text = "컴퓨터가 생각 중입니다...";
        yield return new WaitForSeconds(0.5f);

        Move bestMove = null;
        BoardAI.Negamax(gameBoard, AI_MAX_DEPTH, Mathf.NegativeInfinity, Mathf.Infinity, ref bestMove);

        if (bestMove != null)
        {
            gameBoard = (BoardOmok)gameBoard.MakeMove(bestMove);
        }

        UpdateBoardVisuals();
        CheckForGameOver();
    }

    void UpdateBoardVisuals()
    {
        bool isBlackTurn = gameBoard.GetCurrentPlayer() == 1;

        for (int i = 0; i < BOARD_SIZE; i++)
        {
            for (int j = 0; j < BOARD_SIZE; j++)
            {
                int stone = gameBoard.GetCell(i, j);
                cells[i, j].SetStone(stone == 1 ? blackStoneSprite : (stone == 2 ? whiteStoneSprite : null));

                if (isBlackTurn && stone == 0)
                {
                    bool isForbidden = gameBoard.IsForbiddenMove(new Move_Omok(j, i));
                    if (isForbidden)
                    {
                        cells[i, j].SetForbidden(true, forbiddenSprite);
                    }
                    else
                    {
                        cells[i, j].SetForbidden(false, null);
                    }
                }
                else
                {
                    cells[i, j].SetForbidden(false, null);
                }
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
        return true;
    }
}
