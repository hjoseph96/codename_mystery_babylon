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
        return FacingToDirection[facing];
    }

    /// <summary>
    /// A helper method for getting left or right direction for Speech Bubbles
    /// </summary>
    public static Direction GetHorizontalDirection(Vector3 currentPosition, Vector3 targetDirection)
    {
        if (currentPosition.x == targetDirection.x)
            return Direction.Left;

        var facing = GetFacing(new Vector3(targetDirection.x, 0, 0), new Vector3(currentPosition.x, 0, 0));
        return FacingToDirection[facing];
    }

    public static Vector2 GetFacing(Vector3 currentPosition, Vector3 targetDirection)
    {
        var horizontal = false;
        var vertical = false;
        
        var difference = currentPosition - targetDirection;
        Vector2 rawFacing = difference;

        if (Mathf.Abs(rawFacing.x) > Mathf.Abs(rawFacing.y))
            horizontal = true;

        if (Mathf.Abs(rawFacing.y) > Mathf.Abs(rawFacing.x))
            vertical = true;

        var facing = new Vector2(Mathf.Clamp(difference.x, -1, 1), Mathf.Clamp(difference.y, -1, 1));


        if (facing.x < 0 && facing.x != -1)
            facing.x = -1;

        if (facing.y < 0 && facing.y != -1)
            facing.y = -1;

        if (facing.x > 0 && facing.x != 1)
            facing.x = 1;

        if (facing.y > 0 && facing.y != 1)
            facing.y = 1;

        if (horizontal)
            facing.y = 0;

        if (vertical)
            facing.x = 0;

        return facing;
    }
}
