using System.Collections.Generic;
using UnityEngine;

public class GridPath
{
    public int Length => _path.Count;
    public Vector2Int this[int i] => _path[i];
    public Vector2Int Goal => this[Length - 1];
    public float TravelCost;

    private readonly List<Vector2Int> _path;

    public GridPath(List<Vector2Int> path, float travelCost)
    {
        _path = path;
        TravelCost = travelCost;
    }

    /*public GridPath(Vector2Int position) : this(new List<Vector2Int>() { position }, 0)
    { }*/

    public Vector2Int Pop()
    {
        var value = _path[0];
        _path.RemoveAt(0);
        return value;
    }

    /*public GridPath Clone()
    {
        var pathClone = new List<Vector2Int>(_path);
        return new GridPath(pathClone, TravelCost);
    }*/
}
