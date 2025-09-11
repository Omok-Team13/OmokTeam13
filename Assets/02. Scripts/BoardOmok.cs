using System;
using System.Collections.Generic;
using UnityEngine;

public class BoardOmok : Board
{
    private readonly int[,] board;
    private const int BOARD_SIZE = 19;
    private readonly bool isRenjuRule;

    public BoardOmok(bool applyRenjuRule = true)
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

    public override Move[] GetMoves()
    {
        var moves = new List<Move>();
        for (int i = 0; i < BOARD_SIZE; i++)
        {
            for (int j = 0; j < BOARD_SIZE; j++)
            {
                if (board[i, j] == 0)
                {
                    var newMove = new Move_Omok(i * BOARD_SIZE + j);
                    if (isRenjuRule && GetCurrentPlayer() == 1)
                    {
                        if (!IsForbiddenMove(newMove))
                        {
                            moves.Add(newMove);
                        }
                    }
                    else
                    {
                        moves.Add(newMove);
                    }
                }
            }
        }
        return moves.ToArray();
    }

    public override Board MakeMove(Move m)
    {
        Move_Omok move = m as Move_Omok;
        if (move == null)
        {
            Debug.LogError("잘못된 타입의 Move가 BoardOmok에 전달되었습니다!");
            return this;
        }

        int y = move.Y;
        int x = move.X;

        int nextPlayer = (this.player == 1) ? 2 : 1;
        int[,] boardCopy = (int[,])board.Clone();
        boardCopy[y, x] = this.player;

        return new BoardOmok(boardCopy, nextPlayer, this.isRenjuRule);
    }

    // **수정된 부분:** Renju Rule 적용을 위해 BoardOmok 생성자에 isRenjuRule 매개변수 추가
    public BoardOmok(BoardOmok original) : base()
    {
        this.board = (int[,])original.board.Clone();
        this.player = original.player;
        this.isRenjuRule = original.isRenjuRule;
    }

    public int GetCell(int row, int col)
    {
        return board[row, col];
    }

    public override int CheckWinner()
    {
        for (int i = 0; i < BOARD_SIZE; i++)
        {
            for (int j = 0; j < BOARD_SIZE; j++)
            {
                int p = board[i, j];
                if (p == 0) continue;

                if (j <= BOARD_SIZE - 5 && board[i, j + 1] == p && board[i, j + 2] == p && board[i, j + 3] == p && board[i, j + 4] == p)
                {
                    // 흑의 장목 금수 판정 (6개 이상)
                    if (p == 1 && isRenjuRule && IsOverline(i, j, 0, 1)) continue;
                    return p;
                }
                if (i <= BOARD_SIZE - 5 && board[i + 1, j] == p && board[i + 2, j] == p && board[i + 3, j] == p && board[i + 4, j] == p)
                {
                    if (p == 1 && isRenjuRule && IsOverline(i, j, 1, 0)) continue;
                    return p;
                }
                if (i <= BOARD_SIZE - 5 && j <= BOARD_SIZE - 5 && board[i + 1, j + 1] == p && board[i + 2, j + 2] == p && board[i + 3, j + 3] == p && board[i + 4, j + 4] == p)
                {
                    if (p == 1 && isRenjuRule && IsOverline(i, j, 1, 1)) continue;
                    return p;
                }
                if (i <= BOARD_SIZE - 5 && j >= 4 && board[i + 1, j - 1] == p && board[i + 2, j - 2] == p && board[i + 3, j - 3] == p && board[i + 4, j - 4] == p)
                {
                    if (p == 1 && isRenjuRule && IsOverline(i, j, 1, -1)) continue;
                    return p;
                }
            }
        }

        if (GetMoves().Length == 0) return 3;
        return 0;
    }

    public override float Evaluate(int forPlayer)
    {
        int winner = CheckWinner();
        if (winner == forPlayer) return 100000;
        if (winner != 0 && winner != 3) return -100000;
        if (winner == 3) return 0;

        return EvaluateBoardState(forPlayer);
    }

    private float EvaluateBoardState(int forPlayer)
    {
        float totalScore = 0;
        int opponent = (forPlayer == 1) ? 2 : 1;
        int[] dx = { 1, 0, 1, 1 };
        int[] dy = { 0, 1, 1, -1 };

        // 각 방향으로 보드 전체를 순회하며 패턴 분석
        for (int i = 0; i < BOARD_SIZE; i++)
        {
            for (int j = 0; j < BOARD_SIZE; j++)
            {
                for (int k = 0; k < 4; k++)
                {
                    totalScore += EvaluateLine(j, i, dx[k], dy[k], forPlayer, opponent);
                }
            }
        }
        return totalScore;
    }

    private float EvaluateLine(int y, int x, int dy, int dx, int forPlayer, int opponent)
    {
        int myStones = 0;
        int opponentStones = 0;
        int emptySpaces = 0;

        for (int k = 0; k < 5; k++)
        {
            int ny = y + dy * k;
            int nx = x + dx * k;

            if (ny < 0 || ny >= BOARD_SIZE || nx < 0 || nx >= BOARD_SIZE)
                return 0;

            if (board[ny, nx] == forPlayer) myStones++;
            else if (board[ny, nx] == opponent) opponentStones++;
            else emptySpaces++;
        }

        if (myStones > 0 && opponentStones > 0) return 0;
        if (myStones + opponentStones < 5) return 0;

        // 공격 점수
        if (myStones == 4 && emptySpaces == 1) return 1000;
        if (myStones == 3 && emptySpaces == 2) return 100;
        if (myStones == 2 && emptySpaces == 3) return 10;
        if (myStones == 1 && emptySpaces == 4) return 1;

        // 수비 점수
        if (opponentStones == 4 && emptySpaces == 1) return -5000;
        if (opponentStones == 3 && emptySpaces == 2) return -500;
        if (opponentStones == 2 && emptySpaces == 3) return -15;

        return 0;
    }

    // **새로 추가된 부분: 렌주룰 금수 판정**
    public bool IsForbiddenMove(Move_Omok m)
    {
        if (player != 1 || !isRenjuRule) return false;

        int x = m.X;
        int y = m.Y;

        // 빈칸이 아니면 금수가 아님
        if (board[y, x] != 0) return false;

        // 가상으로 돌을 놓아 금수 여부를 판정
        int originalValue = board[y, x];
        board[y, x] = 1;

        // 3x3, 4x4, 장목 금수 판정
        bool isForbidden = CheckDoubleThree(x, y) || CheckDoubleFour(x, y) || IsOverline(y, x);

        // 원상 복구
        board[y, x] = originalValue;
        return isForbidden;
    }

    private bool CheckDoubleThree(int x, int y)
    {
        int count = 0;
        int[] dx = { 1, 0, 1, 1 };
        int[] dy = { 0, 1, 1, -1 };

        for (int i = 0; i < 4; i++)
        {
            if (IsThreat(x, y, dx[i], dy[i], 3, true)) count++;
        }
        return count >= 2;
    }

    private bool CheckDoubleFour(int x, int y)
    {
        int count = 0;
        int[] dx = { 1, 0, 1, 1 };
        int[] dy = { 0, 1, 1, -1 };

        for (int i = 0; i < 4; i++)
        {
            if (IsThreat(x, y, dx[i], dy[i], 4, true)) count++;
        }
        return count >= 2;
    }

    private bool IsOverline(int y, int x)
    {
        int[] dx = { 1, 0, 1, 1 };
        int[] dy = { 0, 1, 1, -1 };
        for (int i = 0; i < 4; i++)
        {
            if (IsOverline(y, x, dy[i], dx[i])) return true;
        }
        return false;
    }

    private bool IsOverline(int y, int x, int dy, int dx)
    {
        int count = 1;
        // 한 방향
        for (int i = 1; i < BOARD_SIZE; i++)
        {
            int ny = y + dy * i;
            int nx = x + dx * i;
            if (ny < 0 || ny >= BOARD_SIZE || nx < 0 || nx >= BOARD_SIZE || board[ny, nx] != 1) break;
            count++;
        }
        // 반대 방향
        for (int i = 1; i < BOARD_SIZE; i++)
        {
            int ny = y - dy * i;
            int nx = x - dx * i;
            if (ny < 0 || ny >= BOARD_SIZE || nx < 0 || nx >= BOARD_SIZE || board[ny, nx] != 1) break;
            count++;
        }
        return count > 5;
    }

    private bool IsThreat(int x, int y, int dx, int dy, int length, bool isOpen)
    {
        int count = 1;
        int ny, nx;
        int blockedSides = 0;
        int playerStone = 1;

        // 한 방향으로 돌 개수 세기
        for (int i = 1; i < length; i++)
        {
            ny = y + dy * i;
            nx = x + dx * i;
            if (ny < 0 || ny >= BOARD_SIZE || nx < 0 || nx >= BOARD_SIZE || board[ny, nx] != playerStone) break;
            count++;
        }
        // 한 방향 끝이 막혔는지 확인
        ny = y + dy * count;
        nx = x + dx * count;
        if (ny < 0 || ny >= BOARD_SIZE || nx < 0 || nx >= BOARD_SIZE || board[ny, nx] == 3 - playerStone) blockedSides++;

        // 반대 방향으로 돌 개수 세기
        for (int i = 1; i < length; i++)
        {
            ny = y - dy * i;
            nx = x - dx * i;
            if (ny < 0 || ny >= BOARD_SIZE || nx < 0 || nx >= BOARD_SIZE || board[ny, nx] != playerStone) break;
            count++;
        }
        // 반대 방향 끝이 막혔는지 확인
        ny = y - dy * (count - 1);
        nx = x - dx * (count - 1);
        if (ny < 0 || ny >= BOARD_SIZE || nx < 0 || nx >= BOARD_SIZE || board[ny, nx] == 3 - playerStone) blockedSides++;

        if (isOpen)
        {
            return count >= length && blockedSides == 0;
        }
        else
        {
            return count >= length;
        }
    }
}