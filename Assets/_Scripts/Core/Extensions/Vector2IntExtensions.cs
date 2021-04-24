using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public static class Vector2IntExtensions
{
    private static readonly int INFINITY_INT = 999999;
    public static Vector2Int CenterOfGravity(this Vector2Int self, Vector2Int other)
    {
        return (self + other) / 2;
    }

    public static Vector2Int ToVector2Int(this Vector2 self)
    {
        return new Vector2Int((int)self.x, (int)self.y);
    }

    public static Vector2Int ClosestTo(this IEnumerable<Vector2Int> self, Vector2Int other)
    {
        Vector2Int closest = new Vector2Int(INFINITY_INT, INFINITY_INT);
        var closestDist = INFINITY_INT;
        foreach (var item in self)
        {
            var dist = GridUtility.GetBoxDistance(item, other);
            if(dist < closestDist)
            {
                closestDist = dist;
                closest = other;
            }
        }

        return closest;
    }

}
