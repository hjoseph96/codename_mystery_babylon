using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class DirectionExtensions
{
    private static readonly Dictionary<Vector2Int, Direction> InverseCardinalDirections = new Dictionary<Vector2Int, Direction>()
    {
        [Vector2Int.left] = Direction.Left,
        [Vector2Int.up] = Direction.Up,
        [Vector2Int.right] = Direction.Right,
        [Vector2Int.down] = Direction.Down
    };

    private static readonly Dictionary<Vector2Int, Direction> InverseDirections = new Dictionary<Vector2Int, Direction>()
    {
        [Vector2Int.left] = Direction.Left,
        [Vector2Int.up] = Direction.Up,
        [Vector2Int.right] = Direction.Right,
        [Vector2Int.down] = Direction.Down,
        [Vector2Int.one] = Direction.RightUp,
        [-Vector2Int.one] = Direction.LeftDown,
        [new Vector2Int(1, -1)] = Direction.RightDown,
        [new Vector2Int(-1, 1)] = Direction.LeftUp
    };

    private static readonly Dictionary<Direction, Vector2Int> Vectors = new Dictionary<Direction, Vector2Int>()
    {
        [Direction.None] = Vector2Int.zero,
        [Direction.Left] = Vector2Int.left,
        [Direction.Right] = Vector2Int.right,
        [Direction.Up] = Vector2Int.up,
        [Direction.Down] = Vector2Int.down,
        [Direction.LeftUp] = new Vector2Int(-1, 1),
        [Direction.RightDown] = new Vector2Int(1, -1),
        [Direction.LeftDown] = -Vector2Int.one,
        [Direction.RightUp] = Vector2Int.one
    };

    /*public static Direction[] AllDirections;


    static DirectionExtensions()
    {
        AllDirections = Enum.GetValues(typeof(Direction))
            .Cast<Direction>()
            .Where(dir => dir != Direction.None)
            .ToArray();
    }*/
    
    public static bool IsHorizontal(this Direction direction) => direction == Direction.Left || direction == Direction.Right;

    public static bool IsVertical(this Direction direction) => direction == Direction.Up || direction == Direction.Down;

    public static bool IsCardinal(this Direction direction) => direction.IsHorizontal() || direction.IsVertical();

    public static Direction Inverse(this Direction direction) => InverseDirections[-direction.ToVector()];

    public static Direction FromVector(Vector2Int vector, bool allowDiagonal = false)
    {
        var dict = allowDiagonal ? InverseDirections : InverseCardinalDirections;
        return dict.TryGetValue(vector, out var result) ? result : Direction.None;
    }

    public static Vector2Int ToVector(this Direction direction) => Vectors[direction];
}