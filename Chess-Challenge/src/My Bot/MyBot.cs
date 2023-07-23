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
    private int pieceCount;
    
    //v1.1
    public Move Think(Board board, Timer timer)
    {
        Move[] actions = board.GetLegalMoves();

        if (this.iteration == 0)
        {
            this.color = board.IsWhiteToMove ? "white" : "black";
            this.opponentColor = !board.IsWhiteToMove ? "white" : "black";
            this.pieceCount = countPieces(board, this.color);
        }

        Dictionary<double, Move> qMap = Q(board, actions, timer, 3);
        Move decision = policy(qMap);

        this.pieceCount = countPieces(board, this.color);

        iteration += 1;

        return decision;
    }

    private Dictionary<double, Move> Q(Board board, Move[] moves, Timer timer, int steps)
    {
        //learning rate, discount
        double a = 0.1f;
        double y = 1.0f;

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

        //reward captures
        //TODO add individual piece value
        if (move.IsCapture)
        {
            switch(move.CapturePieceType) 
            {
                case ChessChallenge.API.PieceType.Pawn:
                    r += 0.1f;
                    break;
                case ChessChallenge.API.PieceType.Knight:
                    r += 0.3f;
                    break;
                case ChessChallenge.API.PieceType.Bishop:
                    r += 0.4f;
                    break;
                case ChessChallenge.API.PieceType.Rook:
                    r += 0.4f;
                    break;
                case ChessChallenge.API.PieceType.Queen:
                    r += 0.6f;
                    break;
                case ChessChallenge.API.PieceType.King:
                    r += 1.0f;
                    break;
            }
        }

        //penalize if in check
        if(board.IsInCheck()) 
        {
            r -= 0.5f;
        }
        
        //penalize losing pieces
        //TODO: penalize based off of loss
        if(this.pieceCount < countPieces(board, this.color)) 
        {
            r -= 0.1f;
        }

        return r + rand.NextDouble();
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

    int countPieces(Board board, string color) 
    {
        string pieces = board.WhitePiecesBitboard.ToString();

        if(color == "white") 
        {
            return pieces.Length - pieces.Replace("1","").Length;
        } 

        return pieces.Length - pieces.Replace("0","").Length;
    }
}
