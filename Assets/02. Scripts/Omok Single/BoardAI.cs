using UnityEngine;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;

public class BoardAI
{
    private static Move bestMoveFound;
    private static bool timeUp;

    public static async Task<Move> FindBestMoveAsync(Board board, float timeLimitInSeconds)
    {
        System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
        timeUp = false;

        BoardOmok omokBoard = board as BoardOmok;
        if (omokBoard == null) return null;

        var initialMoves = omokBoard.GetRelevantMoves();
        if (initialMoves.Count == 0) return null;
        if (initialMoves.Count == 1) return initialMoves[0];

        bestMoveFound = initialMoves[0];

        Debug.LogWarning("=============== AI 턴 시작 ===============");

        await Task.Run(() =>
        {
            Search(omokBoard, 2, -999999, 999999, stopwatch, timeLimitInSeconds, true);
        });

        stopwatch.Stop();

        Move_Omok finalMove = bestMoveFound as Move_Omok;
        Debug.LogWarning($"=============== 최종 결정: ({finalMove.x}, {finalMove.y}) ===============");

        return bestMoveFound;
    }

    private static float Search(BoardOmok board, int depth, float alpha, float beta, System.Diagnostics.Stopwatch stopwatch, float timeLimit, bool isRoot = false)
    {
        if (stopwatch.Elapsed.TotalSeconds >= timeLimit) timeUp = true;
        if (timeUp) return 0;

        // [수정] CheckWinner()를 통해 게임 종료 상태를 먼저 확인
        int winner = board.CheckWinner();
        if (winner != 0)
        {
            if (winner == board.GetCurrentPlayer()) return 999999;
            if (winner != 3) return -999999;
            return 0; // 무승부
        }

        if (depth == 0)
        {
            // 깊이가 0일 때는 평가 함수를 사용 (이 부분은 더 정교한 평가 함수로 대체 가능)
            return 0;
        }

        var moves = board.GetRelevantMoves();
        var moveEvals = new List<(Move move, int score)>();

        foreach (var move in moves)
        {
            int score = ScoreMoveHeuristically(board, move);
            moveEvals.Add((move, score));
        }

        var orderedMoves = moveEvals.OrderByDescending(eval => eval.score).ToList();

        if (isRoot)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("--- AI가 모든 후보 수를 평가한 결과 (점수 높은 순) ---");
            foreach (var eval in orderedMoves)
            {
                Move_Omok m = eval.move as Move_Omok;
                sb.AppendLine($"좌표: ({m.x}, {m.y}) -> 최종 점수: {eval.score}");
            }
            Debug.Log(sb.ToString());
        }

        float maxScore = -999999;
        Move currentBestMoveInNode = orderedMoves.Count > 0 ? orderedMoves[0].move : null;

        foreach (var eval in orderedMoves)
        {
            Board newBoard = board.MakeMove(eval.move);
            float score = -Search(newBoard as BoardOmok, depth - 1, -beta, -alpha, stopwatch, timeLimit);

            if (score > maxScore)
            {
                maxScore = score;
                currentBestMoveInNode = eval.move;
            }

            alpha = Mathf.Max(alpha, score);
            if (alpha >= beta) break;
        }

        if (isRoot && currentBestMoveInNode != null)
        {
            bestMoveFound = currentBestMoveInNode;
        }

        return maxScore;
    }

    private static int ScoreMoveHeuristically(BoardOmok board, Move move)
    {
        Move_Omok m = move as Move_Omok;
        int aiPlayer = board.GetCurrentPlayer();
        int opponentPlayer = (aiPlayer == 1) ? 2 : 1;

        if (board.CheckIfMoveWins(m.x, m.y, aiPlayer)) return 1000000;
        if (board.CheckIfMoveWins(m.x, m.y, opponentPlayer)) return 900000;

        var myPattern = board.GetPatternAfterMove(m.x, m.y, aiPlayer);
        var opponentPatternToBlock = board.GetPatternAfterMove(m.x, m.y, opponentPlayer);

        if (myPattern == LinePattern.OpenFour) return 800000;
        if (opponentPatternToBlock == LinePattern.OpenFour) return 700000;
        if (myPattern == LinePattern.OpenThree && opponentPatternToBlock == LinePattern.OpenThree) return 600000;
        if (opponentPatternToBlock == LinePattern.HalfOpenFour) return 500000;
        if (myPattern == LinePattern.OpenThree) return 400000;

        int myAttackScore = CalculateMoveScore(board, m.x, m.y, aiPlayer);
        int opponentDefenseScore = CalculateMoveScore(board, m.x, m.y, opponentPlayer);
        return myAttackScore + opponentDefenseScore;
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
        int countForward = 0, countBackward = 0;
        for (int k = 1; k < 5; k++) { if (board.GetCell(y + dy * k, x + dx * k) == player) countForward++; else break; }
        for (int k = 1; k < 5; k++) { if (board.GetCell(y - dy * k, x - dx * k) == player) countBackward++; else break; }
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
            case 4: return openEnds == 2 ? 400000 : 50000;
            case 3: return openEnds == 2 ? 10000 : 1000;
            case 2: return openEnds == 2 ? 500 : 50;
            case 1: return openEnds == 2 ? 10 : 1;
        }
        return 0;
    }
}