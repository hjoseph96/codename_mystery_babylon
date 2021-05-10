using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectionUtility
{
    public static Dictionary<Vector2, Direction> FacingToDirection = new Dictionary<Vector2, Direction>
    {
        {new Vector2(1, 0), Direction.Left },
        {new Vector2(-1, 0), Direction.Right },
        {new Vector2(0, -1), Direction.Up },
        {new Vector2(0, 1), Direction.Down },
    };

    public static Dictionary<Direction, Vector2> DirectionToFacing = new Dictionary<Direction, Vector2>
    {
        { Direction.Left, new Vector2(1, 0) },
        { Direction.Right, new Vector2(-1, 0) },
        { Direction.Up, new Vector2(0, 1) },
        { Direction.Down, new Vector2(0, -1) },
    };

    public static Direction GetDirection(Vector3 currentPosition, Vector3 targetDirection)
    {
        var facing = GetFacing(currentPosition, targetDirection);

        Debug.Log($"Facing: {facing}");

        return FacingToDirection[facing];
    }

    public static Vector2 GetFacing(Vector3 currentPosition, Vector3 targetDirection)
    {
        var difference = currentPosition - targetDirection;
        var facing = new Vector2(Mathf.Clamp(difference.x, -1, 1), Mathf.Clamp(difference.y, -1, 1));

        if (facing.x < 0 && facing.x != -1)
            facing.x = -1;

        if (facing.y < 0 && facing.y != -1)
            facing.y = -1;

        if (facing.x > 0 && facing.x != 1)
            facing.x = 1;

        if (facing.y > 0 && facing.y != 1)
            facing.y = 1;

        return facing;
    }
}
