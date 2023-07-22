using ChessChallenge.API;
using System;
using System.Linq;
using System.Collections.Generic;

public class MyBot : IChessBot
{
    private int iteration = 0;
    private string color;
    private string opponentColor;
    private double QValue = 0.0f;

    public Move Think(Board board, Timer timer)
    {
        Move[] actions = board.GetLegalMoves();

        if (this.iteration == 0)
        {
            this.color = board.IsWhiteToMove ? "white" : "black";
            this.opponentColor = !board.IsWhiteToMove ? "white" : "black";
        }

        Dictionary<double, Move> qMap = Q(board, actions, timer, 1);
        Move decision = policy(qMap);

        iteration += 1;

        return decision;
    }

    private Dictionary<double, Move> Q(Board board, Move[] moves, Timer timer, int steps)
    {
        //learning rate, discount
        double a = 0.1f;
        double y = 0.1f;

        Dictionary<double, Move> qMap = new Dictionary<double, Move>();
        double q = 0.0f;

        if (steps > 0)
        {
            foreach (Move move in moves)
            {
                board.MakeMove(move);
                //Q-value equation
                q =
                    (1 - a) * this.QValue
                    + a
                        * (
                            reward(board, move, timer)
                            + y
                            + maxDouble(Q(board, board.GetLegalMoves(), timer, steps - 1))
                        );

                qMap.Add(q, move);

                board.UndoMove(move);
            }
        }

        return qMap;
    }

    private double reward(Board board, Move move, Timer timer)
    {
        double r = 0.0f;
        //random component is temp to facilitate unique keys
        Random rand = new Random();
        rand.NextDouble();

        if (move.IsCapture)
        {
            r += 0.1f;
        }

        return r + timer.MillisecondsRemaining + rand.NextDouble();
    }

    private Move policy(Dictionary<double, Move> qMap)
    {
        double key = maxDouble(qMap);
        return qMap[key];
    }

    //did not expect to have to write my own max function
    private double maxDouble(Dictionary<double, Move> array)
    {
        double max = 0.0f;
        foreach (KeyValuePair<double, Move> item in array)
        {
            if (item.Key > max)
            {
                max = item.Key;
            }
        }
        return max;
    }
}
