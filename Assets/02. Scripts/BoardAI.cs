using System.Collections.Generic;
using UnityEngine;

public class BoardAI
{
    public static float Negamax(Board board, int depth, float alpha, float beta, ref Move bestMove)
    {
        if (depth == 0 || board.IsGameOver())
        {
            return board.Evaluate(board.GetCurrentPlayer());
        }

        float maxScore = Mathf.NegativeInfinity;
        Move[] moves = board.GetMoves();

        if (bestMove == null && moves.Length > 0)
        {
            bestMove = moves[0];
        }

        foreach (var move in moves)
        {
            Board newBoard = board.MakeMove(move);
            float score = -Negamax(newBoard, depth - 1, -beta, -alpha, ref bestMove);

            if (score > maxScore)
            {
                maxScore = score;
                if (depth == 3)
                {
                    bestMove = move;
                }
            }

            alpha = Mathf.Max(alpha, score);
            if (alpha >= beta)
            {
                break;
            }
        }
        return maxScore;
    }
}