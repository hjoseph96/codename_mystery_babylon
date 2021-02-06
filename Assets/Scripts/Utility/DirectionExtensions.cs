using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class DirectionExtensions
{
    private static readonly Dictionary<Direction, Direction> InverseDirections = new Dictionary<Direction, Direction>()
    {
        [Direction.None] = Direction.None,
        [Direction.Left] = Direction.Right,
        [Direction.Right] = Direction.Left,
        [Direction.Up] = Direction.Down,
        [Direction.Down] = Direction.Up
    };

    private static readonly Dictionary<Direction, Vector2Int> Vectors = new Dictionary<Direction, Vector2Int>()
    {
        [Direction.None] = Vector2Int.zero,
        [Direction.Left] = Vector2Int.left,
        [Direction.Right] = Vector2Int.right,
        [Direction.Up] = Vector2Int.up,
        [Direction.Down] = Vector2Int.down
    };

    public static Direction[] AllDirections;


    static DirectionExtensions()
    {
        AllDirections = Enum.GetValues(typeof(Direction))
            .Cast<Direction>()
            .Where(dir => dir != Direction.None)
            .ToArray();
    }

    public static Direction Inverse(this Direction direction) => InverseDirections[direction];

    public static Vector2Int ToVector(this Direction direction) => Vectors[direction];
}