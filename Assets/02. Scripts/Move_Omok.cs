using System;
using UnityEngine;

public class Move_Omok : Move, IEquatable<Move_Omok>
{
    private const int BOARD_SIZE = 19;

    public int X => position % BOARD_SIZE;
    public int Y => position / BOARD_SIZE;

    public Move_Omok(int position) : base(position) { }

    public bool Equals(Move_Omok other)
    {
        return other != null && position == other.position;
    }

    public override bool Equals(object obj)
    {
        if (obj is Move_Omok other)
        {
            return Equals(other);
        }
        return false;
    }

    public override int GetHashCode()
    {
        return position.GetHashCode();
    }
}