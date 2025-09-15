using UnityEngine;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Generic;

public class BoardAI
{
    private static TranspositionTable transpositionTable = new TranspositionTable();
    private static Move bestMoveFound;
    private static bool timeUp;

    public static async Task<Move> FindBestMoveAsync(Board board, float timeLimitInSeconds)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        timeUp = false;

        BoardOmok omokBoard = board as BoardOmok;
        if (omokBoard == null) return null;

        var initialMoves = omokBoard.GetRelevantMoves();
        if (initialMoves.Count == 0) return null;
        if (initialMoves.Count == 1) return initialMoves[0];

        bestMoveFound = initialMoves[0];

        await Task.Run(() =>
        {
            for (int depth = 2; depth <= 20; depth += 2)
            {
                Search(omokBoard, depth, -999999, 999999, stopwatch, timeLimitInSeconds, true);
                if (timeUp) break;
            }
        });

        stopwatch.Stop();
        return bestMoveFound;
    }

    private static float Search(BoardOmok board, int depth, float alpha, float beta, Stopwatch stopwatch, float timeLimit, bool isRoot = false)
    {
        if (stopwatch.Elapsed.TotalSeconds >= timeLimit) timeUp = true;
        if (timeUp) return 0;

        long hash = transpositionTable.ComputeHash(board.GetBoardState());
        if (!isRoot && transpositionTable.Probe(hash, depth, out float storedScore))
        {
            return storedScore;
        }

        if (depth == 0 || board.IsGameOver())
        {
            return board.Evaluate(board.GetCurrentPlayer());
        }

        float maxScore = -999999;
        var moves = board.GetRelevantMoves();
        var orderedMoves = moves.OrderByDescending(m => ScoreMoveHeuristically(board, m)).ToArray();

        Move currentBestMoveInNode = null;

        foreach (var move in orderedMoves)
        {
            Board newBoard = board.MakeMove(move);
            float score = -Search(newBoard as BoardOmok, depth - 1, -beta, -alpha, stopwatch, timeLimit);

            if (score > maxScore)
            {
                maxScore = score;
                currentBestMoveInNode = move;
            }

            alpha = Mathf.Max(alpha, score);
            if (alpha >= beta) break;
        }

        if (isRoot && currentBestMoveInNode != null)
        {
            bestMoveFound = currentBestMoveInNode;
        }

        transpositionTable.Store(hash, depth, maxScore);
        return maxScore;
    }

    /// <summary>
    /// (핵심 수정) AI가 공격의 '주도권'을 잡도록 가치관을 수정합니다.
    /// </summary>
    private static int ScoreMoveHeuristically(BoardOmok board, Move move)
    {
        Move_Omok m = move as Move_Omok;
        int player = board.GetCurrentPlayer();
        int opponent = (player == 1) ? 2 : 1;

        int myAttackScore = CalculateMoveScore(board, m.x, m.y, player);
        int opponentDefenseScore = CalculateMoveScore(board, m.x, m.y, opponent);

        // (수정) 위협 수준이 비슷할 때 공격에 약간의 보너스를 주어 주도권을 잡도록 유도합니다.
        // 상대의 위협이 훨씬 더 클 경우에는 여전히 수비를 우선합니다.
        return (int)(myAttackScore * 1.1f) + opponentDefenseScore;
    }

    private static int CalculateMoveScore(BoardOmok board, int x, int y, int player)
    {
        int totalScore = 0;
        int[] dx = { 1, 0, 1, 1 };
        int[] dy = { 0, 1, 1, -1 };

        for (int i = 0; i < 4; i++)
        {
            var analysis = AnalyzeLine(board, x, y, player, dy[i], dx[i]);
            totalScore += GetScoreForPattern(analysis.count, analysis.openEnds);
        }

        totalScore += (7 - Mathf.Abs(x - 7)) + (7 - Mathf.Abs(y - 7));
        return totalScore;
    }

    private static (int count, int openEnds) AnalyzeLine(BoardOmok board, int x, int y, int player, int dy, int dx)
    {
        int countForward = 0;
        for (int k = 1; k < 6; k++)
        {
            if (board.GetCell(y + dy * k, x + dx * k) == player) countForward++;
            else break;
        }
        int countBackward = 0;
        for (int k = 1; k < 6; k++)
        {
            if (board.GetCell(y - dy * k, x - dx * k) == player) countBackward++;
            else break;
        }
        int totalCount = 1 + countForward + countBackward;

        int openEnds = 0;
        if (board.GetCell(y + dy * (countForward + 1), x + dx * (countForward + 1)) == 0) openEnds++;
        if (board.GetCell(y - dy * (countBackward + 1), x - dx * (countBackward + 1)) == 0) openEnds++;

        return (totalCount, openEnds);
    }

    private static int GetScoreForPattern(int count, int openEnds)
    {
        if (count >= 5) return 100000;
        if (openEnds == 0) return 0;
        switch (count)
        {
            case 4:
                if (openEnds == 2) return 10000;
                return 500;
            case 3:
                if (openEnds == 2) return 200;
                return 50;
            case 2:
                if (openEnds == 2) return 7;
                break;
        }
        return 0;
    }
}

