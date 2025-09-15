using System;
using System.Collections.Generic;
using UnityEngine;

public class BoardOmok : Board
{
    private readonly int[,] board;
    private const int BOARD_SIZE = 15;
    private readonly bool isRenjuRule;

    // --- 생성자 ---
    public BoardOmok(bool applyRenjuRule = true) : base()
    {
        this.player = 1;
        board = new int[BOARD_SIZE, BOARD_SIZE];
        this.isRenjuRule = applyRenjuRule;
    }

    private BoardOmok(int[,] boardState, int player, bool isRenjuRule)
    {
        this.board = boardState;
        this.player = player;
        this.isRenjuRule = isRenjuRule;
    }

    // --- 기본 함수 ---
    public override Move[] GetMoves()
    {
        var moves = new List<Move>();
        for (int i = 0; i < BOARD_SIZE; i++)
        {
            for (int j = 0; j < BOARD_SIZE; j++)
            {
                if (board[i, j] == 0) moves.Add(new Move_Omok(j, i));
            }
        }
        return moves.ToArray();
    }

    /// <summary>
    /// (오류 수정) override 키워드를 추가하여 상속 멤버를 올바르게 구현합니다.
    /// </summary>
    public override Board MakeMove(Move m)
    {
        Move_Omok move = m as Move_Omok;
        if (move == null || board[move.y, move.x] != 0) return this;
        int nextPlayer = (this.player == 1) ? 2 : 1;
        int[,] boardCopy = (int[,])board.Clone();
        boardCopy[move.y, move.x] = this.player;
        return new BoardOmok(boardCopy, nextPlayer, this.isRenjuRule);
    }

    public int GetCell(int row, int col)
    {
        if (row < 0 || row >= BOARD_SIZE || col < 0 || col >= BOARD_SIZE) return -1;
        return board[row, col];
    }

    // --- AI 최적화 지원 함수 ---
    public List<Move> GetRelevantMoves()
    {
        var moves = new HashSet<Move_Omok>(new MoveComparer());
        bool hasAnyStone = false;
        for (int y = 0; y < BOARD_SIZE; y++)
        {
            for (int x = 0; x < BOARD_SIZE; x++)
            {
                if (board[y, x] != 0)
                {
                    hasAnyStone = true;
                    for (int dy = -2; dy <= 2; dy++)
                    {
                        for (int dx = -2; dx <= 2; dx++)
                        {
                            if (dx == 0 && dy == 0) continue;
                            int ny = y + dy, nx = x + dx;
                            if (ny >= 0 && ny < BOARD_SIZE && nx >= 0 && nx < BOARD_SIZE && board[ny, nx] == 0)
                            {
                                moves.Add(new Move_Omok(nx, ny));
                            }
                        }
                    }
                }
            }
        }
        if (!hasAnyStone) moves.Add(new Move_Omok(BOARD_SIZE / 2, BOARD_SIZE / 2));
        return new List<Move>(moves);
    }

    public int[,] GetBoardState() { return board; }

    public int CheckLineLengthAfterMove(int x, int y, int player)
    {
        int maxLength = 0;
        int[,] tempBoard = (int[,])board.Clone();
        tempBoard[y, x] = player;

        int[] dx = { 1, 0, 1, 1 };
        int[] dy = { 0, 1, 1, -1 };
        for (int i = 0; i < 4; i++)
        {
            int length = CountStonesInLineOnBoard(tempBoard, y, x, dy[i], dx[i], player);
            if (length > maxLength) maxLength = length;
        }
        return maxLength;
    }

    // --- AI 평가 함수 (핵심) ---
    public override float Evaluate(int forPlayer)
    {
        int winner = CheckWinner();
        if (winner == forPlayer) return 99999;
        if (winner != 0 && winner != 3) return -99999;
        if (winner == 3) return 0;

        return EvaluateBoardState(forPlayer) - EvaluateBoardState((forPlayer == 1) ? 2 : 1);
    }

    private float EvaluateBoardState(int player)
    {
        float totalScore = 0;
        for (int y = 0; y < BOARD_SIZE; y++)
        {
            for (int x = 0; x < BOARD_SIZE; x++)
            {
                totalScore += EvaluateLine(y, x, 1, 0, player);
                totalScore += EvaluateLine(y, x, 0, 1, player);
                totalScore += EvaluateLine(y, x, 1, 1, player);
                totalScore += EvaluateLine(y, x, 1, -1, player);
            }
        }
        return totalScore;
    }

    private float EvaluateLine(int y, int x, int dy, int dx, int player)
    {
        if (GetCell(y - dy, x - dx) == player) return 0;

        int opponent = (player == 1) ? 2 : 1;
        int myStones = 0;
        int openEnds = 0;
        int length = 0;

        for (int k = 0; k < 6; k++)
        {
            int ny = y + k * dy;
            int nx = x + k * dx;

            if (ny < 0 || ny >= BOARD_SIZE || nx < 0 || nx >= BOARD_SIZE || GetCell(ny, nx) == opponent)
            {
                break;
            }
            if (GetCell(ny, nx) == player) myStones++;
            length++;
        }

        if (GetCell(y - dy, x - dx) == 0) openEnds++;
        if (length < 6 && GetCell(y + length * dy, x + length * dx) == 0) openEnds++;

        return GetScoreForPattern(myStones, openEnds);
    }

    private float GetScoreForPattern(int count, int openEnds)
    {
        if (openEnds == 0 && count < 5) return 0;
        switch (count)
        {
            case 5: return 50000;
            case 4:
                if (openEnds == 2) return 4000;
                if (openEnds == 1) return 500;
                break;
            case 3:
                if (openEnds == 2) return 200;
                if (openEnds == 1) return 50;
                break;
            case 2:
                if (openEnds == 2) return 7;
                break;
        }
        return 0;
    }

    // --- 게임 규칙 (승리 & 금수) ---

    /// <summary>
    /// (오류 수정) override 키워드를 추가하여 상속 멤버를 올바르게 구현합니다.
    /// </summary>
    public override int CheckWinner()
    {
        bool hasEmptyCell = false;
        for (int i = 0; i < BOARD_SIZE; i++)
        {
            for (int j = 0; j < BOARD_SIZE; j++)
            {
                if (board[i, j] == 0)
                {
                    hasEmptyCell = true;
                    continue;
                }
                int p = board[i, j];
                int[] dx = { 1, 0, 1, 1 };
                int[] dy = { 0, 1, 1, -1 };
                for (int k = 0; k < 4; k++)
                {
                    int lineLength = CountStonesInLine(i, j, dy[k], dx[k], p);
                    if (lineLength == 5)
                    {
                        if (p == 1 && isRenjuRule && CountStonesInLine(i, j, dy[k], dx[k], p) > 5)
                        {
                            continue;
                        }
                        return p;
                    }
                }
            }
        }
        if (!hasEmptyCell) return 3;
        return 0;
    }

    public bool IsForbiddenMove(Move_Omok m)
    {
        if (player != 1 || !isRenjuRule || board[m.y, m.x] != 0) return false;
        int[,] tempBoard = (int[,])board.Clone();
        tempBoard[m.y, m.x] = 1;

        int[] dx = { 1, 0, 1, 1 };
        int[] dy = { 0, 1, 1, -1 };
        for (int i = 0; i < 4; i++)
        {
            if (CountStonesInLineOnBoard(tempBoard, m.y, m.x, dy[i], dx[i], 1) > 5) return true;
        }
        return IsDoubleThreeOrFour(tempBoard, m.y, m.x);
    }

    private bool IsDoubleThreeOrFour(int[,] boardState, int y, int x)
    {
        int threeCount = 0;
        int fourCount = 0;
        int[] dx = { 1, 0, 1, 1 };
        int[] dy = { 0, 1, 1, -1 };
        for (int i = 0; i < 4; i++)
        {
            if (CheckLine(boardState, y, x, dy[i], dx[i], 3)) threeCount++;
            if (CheckLine(boardState, y, x, dy[i], dx[i], 4)) fourCount++;
        }
        return threeCount >= 2 || fourCount >= 2;
    }

    private bool CheckLine(int[,] boardState, int y, int x, int dy, int dx, int targetLength)
    {
        int p = boardState[y, x];
        int countForward = 0;
        for (int i = 1; i < 5; i++)
        {
            if (GetCellOnBoard(boardState, y + dy * i, x + dx * i) == p) countForward++;
            else break;
        }
        int countBackward = 0;
        for (int i = 1; i < 5; i++)
        {
            if (GetCellOnBoard(boardState, y - dy * i, x - dx * i) == p) countBackward++;
            else break;
        }
        int totalLength = countForward + countBackward + 1;
        if (totalLength != targetLength) return false;

        bool isOpenForward = GetCellOnBoard(boardState, y + dy * (countForward + 1), x + dx * (countForward + 1)) == 0;
        bool isOpenBackward = GetCellOnBoard(boardState, y - dy * (countBackward + 1), x - dx * (countBackward + 1)) == 0;

        if (targetLength == 3) return isOpenForward && isOpenBackward;
        if (targetLength == 4) return isOpenForward || isOpenBackward;
        return false;
    }

    private int CountStonesInLineOnBoard(int[,] boardState, int y, int x, int dy, int dx, int p)
    {
        int count = 1;
        for (int i = 1; i < 6; i++)
        {
            if (GetCellOnBoard(boardState, y + dy * i, x + dx * i) != p) break;
            count++;
        }
        for (int i = 1; i < 6; i++)
        {
            if (GetCellOnBoard(boardState, y - dy * i, x - dx * i) != p) break;
            count++;
        }
        return count;
    }

    private int CountStonesInLine(int y, int x, int dy, int dx, int p)
    {
        return CountStonesInLineOnBoard(this.board, y, x, dy, dx, p);
    }

    private int GetCellOnBoard(int[,] boardState, int row, int col)
    {
        if (row < 0 || row >= BOARD_SIZE || col < 0 || col >= BOARD_SIZE) return -1;
        return boardState[row, col];
    }
}

public class MoveComparer : IEqualityComparer<Move_Omok>
{
    public bool Equals(Move_Omok m1, Move_Omok m2)
    {
        if (m1 == null && m2 == null) return true;
        if (m1 == null || m2 == null) return false;
        return m1.x == m2.x && m1.y == m2.y;
    }
    public int GetHashCode(Move_Omok obj)
    {
        return obj.x.GetHashCode() ^ obj.y.GetHashCode();
    }
}