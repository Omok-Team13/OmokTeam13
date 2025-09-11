using UnityEngine;

public class BoardAI
{
    public static float Negamax(Board board, int maxDepth, int currentDepth, ref Move bestMove)
    {
        if (board.IsGameOver() || currentDepth == maxDepth)
        {
            return board.Evaluate(board.GetCurrentPlayer());
        }

        float bestScore = Mathf.NegativeInfinity;

        foreach (Move m in board.GetMoves())
        {
            Board b = board.MakeMove(m);
            Move currentMove = null;

            float recursedScore = Negamax(b, maxDepth, currentDepth + 1, ref currentMove);
            float currentScore = -recursedScore;

            if (currentScore > bestScore)
            {
                bestScore = currentScore;
                bestMove = m;
            }
        }
        return bestScore;
    }
}