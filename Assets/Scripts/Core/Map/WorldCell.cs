using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WorldCell
{
    public string TerrainName;
    public SurfaceType SurfaceType;
    public Vector2Int Position { get; }
    public bool HasLineOfSight { get; set;  }
    public StairsOrientation StairsOrientation { get; set;  }
    public bool IsStairs => StairsOrientation != StairsOrientation.None;
    public int Height { get; set; }


    public Unit Unit { get; set; }

    private Dictionary<UnitType, int> _travelCost = new Dictionary<UnitType, int>();
    private Dictionary<Direction, UnitType> _blockExit = new Dictionary<Direction, UnitType>();
    private Dictionary<Direction, UnitType> _blockEntrance = new Dictionary<Direction, UnitType>();

    public WorldCell(Vector2Int position, TileConfiguration config, int height, Vector3 scale)
    {
        Position = position;
        if (config != null)
            SetTileConfig(config, height, scale);
    }

    public static implicit operator WorldCell(Vector2Int position)
    {
        return WorldGrid.Instance[position];
    }

    public void SetTileConfig(TileConfiguration config, int height = -999, Vector3 scale = new Vector3())
    {
        TerrainName = config.TerrainName;
        SurfaceType = config.SurfaceType;
        HasLineOfSight      = config.HasLineOfSight;
        StairsOrientation   = config.IsStairs ? config.StairsOrientation : StairsOrientation.None;
        _travelCost         = config.TravelCost;

        if (height != -999)
            Height = height;

        // scale matters for diagonal stairs
        if (scale == new Vector3())
            scale = Vector3.one;

        var isFlippedX = scale.x < 0;
        var isFlippedY = scale.y < 0;

        if (isFlippedX && IsStairs)
        {
            StairsOrientation = StairsOrientation == StairsOrientation.LeftToRight
                ? StairsOrientation.RightToLeft
                : StairsOrientation.LeftToRight;
        }

        if (isFlippedX || isFlippedY)
        {
            _blockExit = new Dictionary<Direction, UnitType>();
            _blockEntrance = new Dictionary<Direction, UnitType>();

            foreach (var key in config.BlockExit.Keys)
            {
                var finalKey = key;
                if (key.IsHorizontal() && isFlippedX ||
                    key.IsVertical() && isFlippedY)
                {
                    finalKey = key.Inverse();
                }

                _blockExit[finalKey] = config.BlockExit[key];
            }

            foreach (var key in config.BlockEntrance.Keys)
            {
                var finalKey = key;
                if (key.IsHorizontal() && isFlippedX ||
                    key.IsVertical() && isFlippedY)
                {
                    finalKey = key.Inverse();
                }

                _blockEntrance[finalKey] = config.BlockEntrance[key];
            }
        }
        else
        {
            _blockExit = config.BlockExit;
            _blockEntrance = config.BlockEntrance;
        }
    }

    public int GetTravelCost(UnitType unitType)
    {
        if (_travelCost.TryGetValueExt(unitType, out var result))
            return result;  

        return -1;
    }

    public int GetTravelCost(WorldCell destination, UnitType unitType)
    {
        var offset = destination.Position - Position;
        if (offset.sqrMagnitude == 1)
            return destination.GetTravelCost(unitType);

        var bestCost = int.MaxValue;

        var middleCell = WorldGrid.Instance[Position + new Vector2Int(offset.x, 0)];
        if (CanMove(middleCell, unitType) &&
            middleCell.CanMove(destination, unitType))
        {
            bestCost = middleCell.GetTravelCost(unitType);
        }

        middleCell = WorldGrid.Instance[Position + new Vector2Int(0, offset.y)];
        if (CanMove(middleCell, unitType) &&
            middleCell.CanMove(destination, unitType))
        {
            bestCost = Mathf.Min(bestCost, middleCell.GetTravelCost(unitType));
        }

        if (bestCost == int.MaxValue)
            return -1;

        return bestCost + destination.GetTravelCost(unitType);
    }

    public bool IsPassable(UnitType unitType) => GetTravelCost(unitType) >= 0;

    public bool IsPassable()
    {
        return IsPassable(UnitType.Unmounted) ||
               IsPassable(UnitType.Mounted) ||
               IsPassable(UnitType.Air);
    }

    public bool CanExit(Direction direction, UnitType unitType)
    {
        return direction != Direction.None && !(_blockExit.TryGetValue(direction, out var blockedType) &&
                                                (blockedType & unitType) == unitType);
    }

    public bool CanEnter(Direction direction, UnitType unitType)
    {
        return direction != Direction.None && !(_blockEntrance.TryGetValue(direction, out var blockedType) &&
                                                (blockedType & unitType) == unitType);
    }

    // TODO:  out int travelCost
    public bool CanMove(WorldCell destination, UnitType unitType)
    {
        var direction = GridUtility.GetDirection(Position, destination.Position, false, true);
        if (direction == Direction.None)
            return false;

        return direction.IsCardinal() ? CanMoveCardinalDirection(direction, destination, unitType) : CanMoveDiagonalDirection(destination, unitType);
    }

    private bool CanMoveCardinalDirection(Direction direction, WorldCell destination, UnitType unitType)
    {
        return destination.IsPassable(unitType) && CanExit(direction, unitType) && destination.CanEnter(direction.Inverse(), unitType);
    }

    private bool CanMoveDiagonalDirection(WorldCell destination, UnitType unitType)
    {
        return GetTravelCost(destination, unitType) >= 0;
    }
}