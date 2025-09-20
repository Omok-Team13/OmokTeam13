using System;
using System.Collections.Generic;
using UnityEngine;

public enum LinePattern
{
    None, HalfOpenThree, OpenThree, HalfOpenFour, OpenFour, Five
}

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

    public BoardOmok(int[,] boardState, int player, bool isRenjuRule)
    {
        this.board = boardState;
        this.player = player;
        this.isRenjuRule = isRenjuRule;
    }

    // --- AI 지원 함수 ---

    // [핵심 수정] 후보 수를 찾는 로직을 더 안정적인 방식으로 완전히 변경
    public List<Move> GetRelevantMoves()
    {
        // 1. 고유한 좌표를 숫자로 저장할 HashSet (y * 100 + x)
        HashSet<int> relevantCoords = new HashSet<int>();
        var finalMoves = new List<Move>();
        bool hasAnyStone = false;

        for (int y = 0; y < BOARD_SIZE; y++)
        {
            for (int x = 0; x < BOARD_SIZE; x++)
            {
                if (board[y, x] != 0) // 돌이 있는 곳 주변을 탐색
                {
                    hasAnyStone = true;
                    for (int dy = -2; dy <= 2; dy++)
                    {
                        for (int dx = -2; dx <= 2; dx++)
                        {
                            if (dx == 0 && dy == 0) continue;

                            int ny = y + dy;
                            int nx = x + dx;

                            // 보드 범위 안이고 빈칸일 경우
                            if (ny >= 0 && ny < BOARD_SIZE && nx >= 0 && nx < BOARD_SIZE && board[ny, nx] == 0)
                            {
                                relevantCoords.Add(ny * 100 + nx); // 고유 숫자로 변환하여 추가
                            }
                        }
                    }
                }
            }
        }

        // 2. 만약 돌이 하나도 없다면, 중앙에 두도록 함
        if (!hasAnyStone)
        {
            finalMoves.Add(new Move_Omok(BOARD_SIZE / 2, BOARD_SIZE / 2));
            return finalMoves;
        }

        // 3. 고유한 숫자 좌표들을 다시 Move_Omok 객체로 변환
        foreach (int coord in relevantCoords)
        {
            int y = coord / 100;
            int x = coord % 100;
            finalMoves.Add(new Move_Omok(x, y));
        }

        Debug.Log($"GetRelevantMoves가 찾은 후보 수: {finalMoves.Count}");
        return finalMoves;
    }


    // --- 이하 다른 함수들은 이전과 동일합니다 ---

    public override Move[] GetMoves() { /* ...기존 코드... */ return null; }
    public override Board MakeMove(Move m)
    {
        Move_Omok move = m as Move_Omok;
        if (move == null || board[move.y, move.x] != 0) return this;
        int nextPlayer = (this.player == 1) ? 2 : 1;
        int[,] boardCopy = (int[,])board.Clone();
        boardCopy[move.y, move.x] = this.player;
        return new BoardOmok(boardCopy, nextPlayer, this.isRenjuRule);
    }
    public int GetCell(int row, int col) { if (row < 0 || row >= BOARD_SIZE || col < 0 || col >= BOARD_SIZE) return -1; return board[row, col]; }
    public int GetCellOnBoard(int[,] boardState, int row, int col) { if (row < 0 || row >= BOARD_SIZE || col < 0 || col >= BOARD_SIZE) return -1; return boardState[row, col]; }
    public int[,] GetBoardState() { return board; }

    public bool CheckIfMoveWins(int x, int y, int playerToCheck)
    {
        if (GetCell(y, x) != 0) return false;
        int[,] tempBoard = (int[,])board.Clone();
        tempBoard[y, x] = playerToCheck;
        int[] dx = { 1, 0, 1, 1 };
        int[] dy = { 0, 1, 1, -1 };
        for (int i = 0; i < 4; i++)
        {
            int lineLength = CountStonesInLineOnBoard(tempBoard, y, x, dy[i], dx[i], playerToCheck);
            if (lineLength >= 5)
            {
                if (playerToCheck == 1 && isRenjuRule && lineLength > 5) { continue; }
                return true;
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
            if (currentPattern > bestPattern) { bestPattern = currentPattern; }
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

    public override int CheckWinner() { /* ...기존 코드... */ return 0; }
    public bool IsForbiddenMove(Move_Omok m) { /* ...기존 코드... */ return false; }
    private bool IsDoubleThreeOrFour(int[,] boardState, int y, int x) { /* ...기존 코드... */ return false; }
    private bool CheckLine(int[,] boardState, int y, int x, int dy, int dx, int targetLength) { /* ...기존 코드... */ return false; }
    private int CountStonesInLine(int y, int x, int dy, int dx, int p) { return CountStonesInLineOnBoard(this.board, y, x, dy, dx, p); }
    private int CountStonesInLineOnBoard(int[,] boardState, int y, int x, int dy, int dx, int p)
    {
        int count = 1;
        for (int i = 1; i < 6; i++) { if (GetCellOnBoard(boardState, y + dy * i, x + dx * i) != p) break; count++; }
        for (int i = 1; i < 6; i++) { if (GetCellOnBoard(boardState, y - dy * i, x - dx * i) != p) break; count++; }
        return count;
    }
    public override float Evaluate(int forPlayer) { return 0; }
}

// MoveComparer는 이제 GetRelevantMoves에서 사용되지 않지만, 다른 곳에서 필요할 수 있으므로 남겨둡니다.
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