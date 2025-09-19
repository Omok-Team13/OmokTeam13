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
            for (int depth = 2; depth <= 6; depth += 2)
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

    private static int ScoreMoveHeuristically(BoardOmok board, Move move)
    {
        Move_Omok m = move as Move_Omok;
        int aiPlayer = board.GetCurrentPlayer();
        int opponentPlayer = (aiPlayer == 1) ? 2 : 1;

        if (board.CheckIfMoveWins(m.x, m.y, aiPlayer)) return 1000000;
        if (board.CheckIfMoveWins(m.x, m.y, opponentPlayer)) return 900000;

        // `HasOpenFour`와 같은 복잡한 헬퍼 함수 대신, `CalculateMoveScore`의 점수 체계를 신뢰하여 로직을 단순화합니다.
        // 더 높은 점수 패턴이 자연스럽게 우선순위를 갖게 됩니다.
        int myAttackScore = CalculateMoveScore(board, m.x, m.y, aiPlayer);
        int opponentDefenseScore = CalculateMoveScore(board, m.x, m.y, opponentPlayer);

        // 상대방의 위협이 더 클 경우 방어에 더 큰 가중치를 둡니다.
        if (opponentDefenseScore > myAttackScore)
        {
            return opponentDefenseScore * 2 + myAttackScore;
        }

        return myAttackScore * 2 + opponentDefenseScore;
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
        for (int k = 1; k < 5; k++)
        {
            if (board.GetCell(y + dy * k, x + dx * k) == player) countForward++;
            else break;
        }
        int countBackward = 0;
        for (int k = 1; k < 5; k++)
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
        if (count >= 5) return 500000;
        if (openEnds == 0) return 0;

        switch (count)
        {
            case 4:
                return openEnds == 2 ? 400000 : 50000;
            case 3:
                return openEnds == 2 ? 10000 : 1000;
            case 2:
                return openEnds == 2 ? 500 : 50;
            case 1:
                return openEnds == 2 ? 10 : 1;
        }
        return 0;
    }
}