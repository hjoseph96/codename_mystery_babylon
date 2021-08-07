using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[Serializable]
public class GridPath
{
    public int Length => _path != null ? _path.Count : 0;
    public Vector2Int this[int i] => _path[i];
    public Vector2Int Goal => this[Length - 1];
    public float TravelCost;

    private readonly List<Vector2Int> _path;

    public GridPath(List<Vector2Int> path, float travelCost)
    {
        _path = path;
        TravelCost = travelCost;
    }

    public Vector2Int Pop()
    {
        var value = _path[0];
        _path.RemoveAt(0);
        return value;
    }

    public Vector2Int GetPointInPath(float normalizedDistance)
    {
        normalizedDistance = Mathf.Clamp01(normalizedDistance);
        return _path[Mathf.FloorToInt(_path.Count / (1 / normalizedDistance))];
    }

    public List<Vector2Int> Intersect(List<Vector2Int> cells)
    {
        return _path.Intersect(cells).ToList();
    }

    public List<Vector2Int> Path()
    {
        return _path;
    }


}
