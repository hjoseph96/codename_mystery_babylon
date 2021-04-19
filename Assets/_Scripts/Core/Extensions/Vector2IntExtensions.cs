using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Vector2IntExtensions
{
    public static Vector2Int CenterOfGravity(this Vector2Int self, Vector2Int other)
    {
        return (self + other) / 2;
    }

    public static Vector2Int ToVector2Int(this Vector2 self)
    {
        return new Vector2Int((int)self.x, (int)self.y);
    }

}
