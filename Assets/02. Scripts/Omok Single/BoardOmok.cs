using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 오목 라인 패턴의 종류를 정의합니다. AI의 가치 판단에 사용됩니다.
/// </summary>
public enum LinePattern
{
    None,
    HalfOpenThree,
    OpenThree,
    HalfOpenFour,
    OpenFour,
    Five
}

public class BoardOmok : Board
{
    private readonly int[,] board;
    private const int BOARD_SIZE = 15;
    private readonly bool isRenjuRule;

    public BoardOmok(bool applyRenjuRule = true) : base()
    {
        this.player = 1;
        board = new int[BOARD_SIZE, BOARD_SIZE];
        this.isRenjuRule = applyRenjuRule;
    }

    public BoardOmok(int[,] boardState, int player, bool isRenjuRule)
    {
        this.board = boardState;
        this.player = player;
        this.isRenjuRule = isRenjuRule;
    }

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

    public int GetCellOnBoard(int[,] boardState, int row, int col)
    {
        if (row < 0 || row >= BOARD_SIZE || col < 0 || col >= BOARD_SIZE) return -1;
        return boardState[row, col];
    }

    public int[,] GetBoardState()
    {
        return board;
    }

    public List<Move> GetRelevantMoves()
    {
        HashSet<int> relevantCoords = new HashSet<int>();
        var finalMoves = new List<Move>();
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
                            int ny = y + dy;
                            int nx = x + dx;
                            if (ny >= 0 && ny < BOARD_SIZE && nx >= 0 && nx < BOARD_SIZE && board[ny, nx] == 0)
                            {
                                relevantCoords.Add(ny * 100 + nx);
                            }
                        }
                    }
                }
            }
        }
        if (!hasAnyStone)
        {
            finalMoves.Add(new Move_Omok(BOARD_SIZE / 2, BOARD_SIZE / 2));
            return finalMoves;
        }
        foreach (int coord in relevantCoords)
        {
            int y = coord / 100;
            int x = coord % 100;
            finalMoves.Add(new Move_Omok(x, y));
        }
        return finalMoves;
    }

    public bool CheckIfMoveWins(int x, int y, int playerToCheck)
    {
        if (GetCell(y, x) != 0) return false;
        int[,] tempBoard = (int[,])board.Clone();
        tempBoard[y, x] = playerToCheck;
        int[] dx = { 1, 0, 1, 1 };
        int[] dy = { 0, 1, 1, -1 };
        for (int i = 0; i < 4; i++)
        {
            int lineLength = CountStonesInLineOnBoard(tempBoard, y, x, dy[i], dx[i], playerToCheck, true);
            if (playerToCheck == 1 && isRenjuRule)
            {
                if (lineLength == 5) return true;
            }
            else
            {
                if (lineLength >= 5) return true;
            }
        }
        return false;
    }

    public LinePattern GetPatternAfterMove(int x, int y, int player)
    {
        if (GetCell(y, x) != 0) return LinePattern.None;
        int[,] tempBoard = (int[,])board.Clone();
        tempBoard[y, x] = player;
        LinePattern bestPattern = LinePattern.None;
        int[] dx = { 1, 0, 1, 1 };
        int[] dy = { 0, 1, 1, -1 };
        for (int i = 0; i < 4; i++)
        {
            var currentPattern = AnalyzeLinePattern(tempBoard, y, x, player, dy[i], dx[i]);
            if (currentPattern > bestPattern)
            {
                bestPattern = currentPattern;
            }
        }
        return bestPattern;
    }

    private LinePattern AnalyzeLinePattern(int[,] currentBoard, int y, int x, int player, int dy, int dx)
    {
        int count = 1;
        int headX = x, headY = y, tailX = x, tailY = y;
        for (int i = 1; i < 5; i++) { if (GetCellOnBoard(currentBoard, y + dy * i, x + dx * i) == player) { count++; headY = y + dy * i; headX = x + dx * i; } else break; }
        for (int i = 1; i < 5; i++) { if (GetCellOnBoard(currentBoard, y - dy * i, x - dx * i) == player) { count++; tailY = y - dy * i; tailX = x - dx * i; } else break; }
        if (count >= 5) return LinePattern.Five;
        int openEnds = 0;
        if (GetCellOnBoard(currentBoard, headY + dy, headX + dx) == 0) openEnds++;
        if (GetCellOnBoard(currentBoard, tailY - dy, tailX - dx) == 0) openEnds++;
        if (count == 4) return openEnds == 2 ? LinePattern.OpenFour : (openEnds == 1 ? LinePattern.HalfOpenFour : LinePattern.None);
        if (count == 3) return openEnds == 2 ? LinePattern.OpenThree : (openEnds == 1 ? LinePattern.HalfOpenThree : LinePattern.None);
        return LinePattern.None;
    }

    public override int CheckWinner()
    {
        bool hasEmptyCell = false;
        for (int y = 0; y < BOARD_SIZE; y++)
        {
            for (int x = 0; x < BOARD_SIZE; x++)
            {
                if (board[y, x] == 0)
                {
                    hasEmptyCell = true;
                    continue;
                }
                int p = board[y, x];
                int[] dx = { 1, 0, 1, 1 };
                int[] dy = { 0, 1, 1, -1 };
                for (int k = 0; k < 4; k++)
                {
                    int lineLength = CountStonesInLine(y, x, dy[k], dx[k], p);
                    if (p == 1 && isRenjuRule)
                    {
                        if (lineLength == 5) return p;
                    }
                    else
                    {
                        if (lineLength >= 5) return p;
                    }
                }
            }
        }
        if (!hasEmptyCell) return 3;
        return 0;
    }

    /// <summary>
    /// 해당 수가 현재 플레이어에게 금수인지 판정합니다. (흑돌에게만 적용)
    /// </summary>
    public bool IsForbiddenMove(Move_Omok m)
    {
        if (player != 1 || !isRenjuRule || board[m.y, m.x] != 0)
        {
            return false;
        }

        int[,] tempBoard = (int[,])board.Clone();
        tempBoard[m.y, m.x] = 1;

        int[] dx = { 1, 0, 1, 1 };
        int[] dy = { 0, 1, 1, -1 };
        for (int i = 0; i < 4; i++)
        {
            if (CountStonesInLineOnBoard(tempBoard, m.y, m.x, dy[i], dx[i], 1) > 5)
            {
                return true;
            }
        }

        return IsDoubleThreeOrFour(tempBoard, m.y, m.x);
    }

    /// <summary>
    /// (y, x)에 돌을 놓았을 때 3-3 또는 4-4가 되는지 확인합니다.
    /// </summary>
    private bool IsDoubleThreeOrFour(int[,] boardState, int y, int x)
    {
        int openThreeCount = 0;
        int fourCount = 0;
        int[] dx = { 1, 0, 1, 1 };
        int[] dy = { 0, 1, 1, -1 };

        for (int i = 0; i < 4; i++)
        {
            if (CountStonesInLineOnBoard(boardState, y, x, dy[i], dx[i], 1, true) == 5)
            {
                return false;
            }
        }

        for (int i = 0; i < 4; i++)
        {
            if (IsOpenThree(boardState, y, x, dy[i], dx[i]))
            {
                openThreeCount++;
            }
            if (IsFour(boardState, y, x, dy[i], dx[i]))
            {
                fourCount++;
            }
        }

        return openThreeCount >= 2 || fourCount >= 2;
    }

    /// <summary>
    /// (y, x)에 놓은 돌을 포함하는 라인이 '열린 3'인지 판별합니다.
    /// '열린 3'이란, 한 수를 더해 '열린 4'를 만들 수 있는 모양을 의미합니다.
    /// </summary>
    private bool IsOpenThree(int[,] boardState, int y, int x, int dy, int dx)
    {
        string pattern = "";
        for (int i = -4; i <= 4; i++)
        {
            int ny = y + i * dy;
            int nx = x + i * dx;
            int cellState = GetCellOnBoard(boardState, ny, nx);

            if (cellState == 1) pattern += "O";
            else if (cellState == 0) pattern += "_";
            else pattern += "X";
        }

        if (pattern.Contains("_OOO__")) return true;
        if (pattern.Contains("__OOO_")) return true;
        if (pattern.Contains("_O_OO_")) return true;
        if (pattern.Contains("_OO_O_")) return true;

        return false;
    }

    /// <summary>
    /// (y, x)에 놓은 돌을 포함하는 라인이 '4'인지 판별합니다.
    /// </summary>
    private bool IsFour(int[,] boardState, int y, int x, int dy, int dx)
    {
        int count = CountStonesInLineOnBoard(boardState, y, x, dy, dx, 1);

        if (count != 4) return false;

        int head_y = y, head_x = x;
        int tail_y = y, tail_x = x;

        while (GetCellOnBoard(boardState, head_y + dy, head_x + dx) == 1) { head_y += dy; head_x += dx; }
        while (GetCellOnBoard(boardState, tail_y - dy, tail_x - dx) == 1) { tail_y -= dy; tail_x -= dx; }

        if (GetCellOnBoard(boardState, head_y + dy, head_x + dx) == 0 || GetCellOnBoard(boardState, tail_y - dy, tail_x - dx) == 0)
        {
            return true;
        }

        return false;
    }

    private int CountStonesInLine(int y, int x, int dy, int dx, int p)
    {
        return CountStonesInLineOnBoard(this.board, y, x, dy, dx, p);
    }

    private int CountStonesInLineOnBoard(int[,] boardState, int y, int x, int dy, int dx, int p, bool exactCount = false)
    {
        int count = 1;
        int limit = exactCount ? 5 : 6;

        for (int i = 1; i < limit; i++) { if (GetCellOnBoard(boardState, y + dy * i, x + dx * i) != p) break; count++; }
        for (int i = 1; i < limit; i++) { if (GetCellOnBoard(boardState, y - dy * i, x - dx * i) != p) break; count++; }
        return count;
    }

    public override float Evaluate(int forPlayer)
    {
        return 0;
    }
}

/// <summary>
/// HashSet에서 Move_Omok 객체를 좌표 기반으로 비교하기 위한 클래스 (현재 GetRelevantMoves에서는 사용되지 않음)
/// </summary>
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