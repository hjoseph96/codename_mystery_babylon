using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RelativePosition
{
    public Unit Unit;
    public  Vector2Int Position;
    private Vector2Int _target;
    public Vector2Int Target
    {
        get
        {
            return _target;
        }

        set
        {
            _target = value;
            Path = GridUtility.FindPath(Unit, Position, _target);
        }
    }

    public GridPath Path;

    public RelativePosition(Unit _unit, Vector2Int _target)
    {
        Unit = _unit;
        Position = Unit.GridPosition;
        Target = _target;

    }

    public Vector2Int GetPointInPathOrDefault(float normalizedDistance)
    {
        if (Position == Target)
            return Position;

        return Path.GetPointInPath(normalizedDistance);
    }

}
