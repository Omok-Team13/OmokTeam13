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

    private const int BOARD_SIZE = 19;
    private const int AI_PLAYER = 2;
    private const int AI_MAX_DEPTH = 3;

    private BoardOmok gameBoard;
    private Cell_Omok[,] cells = new Cell_Omok[BOARD_SIZE, BOARD_SIZE];

    public static Action startAction;

    void Start()
    {
        restartButton.onClick.AddListener(StartGame);
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
        statusText.text = "�÷��̾� (��) ��";
        restartButton.gameObject.SetActive(false);

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
        if (gameBoard.GetCurrentPlayer() == 1)
        {
            Move_Omok playerMove = new Move_Omok(y * BOARD_SIZE + x);
            if (gameBoard.IsForbiddenMove(playerMove))
            {
                statusText.text = "�ݼ� ��ġ�Դϴ�. �ٽ� �����ϼ���.";
                return;
            }
        }

        if (gameBoard.GetCurrentPlayer() != 1 || gameBoard.IsGameOver() || gameBoard.GetCell(y, x) != 0) return;

        gameBoard = (BoardOmok)gameBoard.MakeMove(new Move_Omok(y * BOARD_SIZE + x));
        UpdateBoardVisuals();

        if (CheckForGameOver()) return;

        StartCoroutine(AITurn());
    }

    IEnumerator AITurn()
    {
        statusText.text = "��ǻ�Ͱ� ���� ���Դϴ�...";
        yield return new WaitForSeconds(0.5f);

        Move bestMove = null;
        // ������ �κ�: ����-��Ÿ ����ġ�� ���� �߰�
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
                    bool isForbidden = gameBoard.IsForbiddenMove(new Move_Omok(i * BOARD_SIZE + j));
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
            statusText.text = gameBoard.GetCurrentPlayer() == 1 ? "�÷��̾� (��) ��" : "��ǻ�� (��) ��";
            return false;
        }

        if (winner == 3) statusText.text = "���º��Դϴ�!";
        else if (winner == 1) statusText.text = "�÷��̾� (��) �¸�!";
        else statusText.text = "��ǻ�� (��) �¸�!";

        restartButton.gameObject.SetActive(true);
        return true;
    }
}