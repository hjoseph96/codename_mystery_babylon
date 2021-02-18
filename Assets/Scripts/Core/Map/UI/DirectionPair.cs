using System;

[Serializable]
public struct DirectionPair
{
    public Direction In;
    public Direction Out;

    public DirectionPair(Direction first, Direction second)
    {
        In = first;
        Out = second;
    }
}