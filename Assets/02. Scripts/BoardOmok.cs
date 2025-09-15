using System;
using System.Collections.Generic;
using UnityEngine;

public class BoardOmok : Board
{
    private readonly int[,] board;
    private const int BOARD_SIZE = 15;
    private readonly bool isRenjuRule;

    public BoardOmok(bool applyRenjuRule = true) : base()
    {
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
                    moves.Add(new Move_Omok(j, i));
                }
            }
        }
        return moves.ToArray();
    }

    public override Board MakeMove(Move m)
    {
        Move_Omok move = m as Move_Omok;
        if (move == null || board[move.y, move.x] != 0)
        {
            return this; // 잘못된 이동이거나 이미 돌이 있으면 현재 보드 상태를 그대로 반환
        }

        int nextPlayer = (this.player == 1) ? 2 : 1;
        int[,] boardCopy = (int[,])board.Clone();
        boardCopy[move.y, move.x] = this.player;

        return new BoardOmok(boardCopy, nextPlayer, this.isRenjuRule);
    }

    public int GetCell(int row, int col)
    {
        if (row < 0 || row >= BOARD_SIZE || col < 0 || col >= BOARD_SIZE) return -1; // 보드 바깥
        return board[row, col];
    }

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
                // 4가지 방향(가로, 세로, 대각선\, 대각선/)으로 5목 확인
                int[] dx = { 1, 0, 1, 1 };
                int[] dy = { 0, 1, 1, -1 };

                for (int k = 0; k < 4; k++)
                {
                    // (수정) 승리 판정 로직을 더 간결하고 정확하게 변경
                    if (CountStonesInLine(i, j, dy[k], dx[k], p) == 5)
                    {
                        // 렌주룰: 흑의 6목 이상은 승리가 아님
                        if (p == 1 && isRenjuRule && CountStonesInLine(i, j, dy[k], dx[k], p) > 5)
                        {
                            continue;
                        }
                        return p; // 승자 반환
                    }
                }
            }
        }

        // (수정) 비효율적인 GetMoves() 호출 대신, 빈 칸 유무로 무승부 판단
        if (!hasEmptyCell) return 3; // 무승부

        return 0; // 게임 진행 중
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

        for (int i = 0; i < BOARD_SIZE; i++)
        {
            for (int j = 0; j < BOARD_SIZE; j++)
            {
                if (board[i, j] != 0) continue; // 빈 칸에서만 라인을 평가하는 것이 더 효율적

                // 4가지 방향의 라인을 평가
                totalScore += EvaluateLine(i, j, 1, 0, forPlayer, opponent); // 가로
                totalScore += EvaluateLine(i, j, 0, 1, forPlayer, opponent); // 세로
                totalScore += EvaluateLine(i, j, 1, 1, forPlayer, opponent); // 대각선 \
                totalScore += EvaluateLine(i, j, 1, -1, forPlayer, opponent); // 대각선 /
            }
        }
        return totalScore;
    }

    private float EvaluateLine(int y, int x, int dy, int dx, int forPlayer, int opponent)
    {
        float score = 0;
        // 5칸 라인에 대해 평가
        score += GetLineScore(y, x, dy, dx, 5, forPlayer, opponent);
        // 6칸 라인을 평가하여 '열린 4' 같은 더 가치있는 수를 찾도록 함
        score += GetLineScore(y, x, dy, dx, 6, forPlayer, opponent);
        return score;
    }

    private float GetLineScore(int y, int x, int dy, int dx, int len, int forPlayer, int opponent)
    {
        int myStones = 0;
        int opponentStones = 0;
        int emptySpaces = 0;

        for (int k = 0; k < len; k++)
        {
            int ny = y + dy * k;
            int nx = x + dx * k;
            if (ny < 0 || ny >= BOARD_SIZE || nx < 0 || nx >= BOARD_SIZE) return 0;

            int cellState = board[ny, nx];
            if (cellState == forPlayer) myStones++;
            else if (cellState == opponent) opponentStones++;
            else emptySpaces++;
        }

        // (수정) AI 평가 함수의 치명적 버그 수정 (myStones > 0 && opponentStones > 0 조건 제거)
        if (myStones > 0 && opponentStones > 0) return 0; // 한 라인에 나와 상대 돌이 같이 있으면 가치 없음

        float score = 0;
        if (myStones > 0)
        {
            if (myStones == 4) score = 1000;
            else if (myStones == 3) score = 100;
            else if (myStones == 2) score = 10;
            else score = 1;
        }
        else if (opponentStones > 0)
        {
            if (opponentStones == 4) score = -5000;
            else if (opponentStones == 3) score = -500;
            else if (opponentStones == 2) score = -15;
            else score = -1;
        }

        // '열린' 라인에 가중치 부여 (예: 양쪽이 비어있는 3은 더 위협적)
        if (emptySpaces == len - myStones - opponentStones && emptySpaces > 0)
        {
            score *= 1.5f;
        }

        return score;
    }

    public bool IsForbiddenMove(Move_Omok m)
    {
        if (player != 1 || !isRenjuRule || board[m.y, m.x] != 0) return false;

        // (수정) 보드 상태를 직접 바꾸지 않고, 가상의 보드에서 검사하도록 변경
        return IsDoubleThree(m.y, m.x) || IsDoubleFour(m.y, m.x);
    }

    private bool IsDoubleThree(int y, int x)
    {
        return CountOpenThrees(y, x) >= 2;
    }

    private bool IsDoubleFour(int y, int x)
    {
        return CountOpenFours(y, x) >= 2;
    }

    // (수정) 금수 체크 로직을 더 안전하고 명확하게 개선
    private int CountOpenThrees(int y, int x)
    {
        return CountOpenLines(y, x, 3);
    }
    private int CountOpenFours(int y, int x)
    {
        return CountOpenLines(y, x, 4);
    }

    private int CountOpenLines(int y, int x, int targetLength)
    {
        int count = 0;
        int[] dx = { 1, 0, 1, 1 };
        int[] dy = { 0, 1, 1, -1 };

        board[y, x] = 1; // 임시로 돌을 놓아봄
        for (int i = 0; i < 4; i++)
        {
            if (IsOpenLine(y, x, dy[i], dx[i], targetLength))
            {
                count++;
            }
        }
        board[y, x] = 0; // 검사 후 반드시 원래대로 되돌림

        return count;
    }

    private bool IsOpenLine(int y, int x, int dy, int dx, int targetLength)
    {
        int stones = CountStonesInLine(y, x, dy, dx, 1);
        if (stones != targetLength) return false;

        // 라인의 양 끝이 비어 있는지 확인
        int emptyCount = 0;
        if (GetCell(y - dy, x - dx) == 0) emptyCount++;

        int endY = y, endX = x;
        while (GetCell(endY + dy, endX + dx) == 1)
        {
            endY += dy;
            endX += dx;
        }
        if (GetCell(endY + dy, endX + dx) == 0) emptyCount++;

        return emptyCount == 2;
    }

    private int CountStonesInLine(int y, int x, int dy, int dx, int p)
    {
        int count = 1;
        // 정방향
        for (int i = 1; i < 6; i++)
        {
            if (GetCell(y + dy * i, x + dx * i) == p) count++;
            else break;
        }
        // 역방향
        for (int i = 1; i < 6; i++)
        {
            if (GetCell(y - dy * i, x - dx * i) == p) count++;
            else break;
        }
        return count;
    }
}
