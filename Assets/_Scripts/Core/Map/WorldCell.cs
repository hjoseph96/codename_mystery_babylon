using System;
using System.Collections.Generic;
using UnityEngine;


//[Serializable]
public class WorldCellTile
{
    public string TerrainName;
    public SurfaceType SurfaceType;
    public bool HasLineOfSight { get; set; }
    public StairsOrientation StairsOrientation { get; set; }
    public bool IsStairs => StairsOrientation != StairsOrientation.None;

    private Dictionary<UnitType, int> _travelCost = new Dictionary<UnitType, int>();
    private Dictionary<Direction, UnitType> _blockExit = new Dictionary<Direction, UnitType>();
    private Dictionary<Direction, UnitType> _blockEntrance = new Dictionary<Direction, UnitType>();

    public WorldCellTile(TileConfiguration tileConfig, Vector3 scale)
    {
        SetTileConfig(tileConfig, scale);
    }

    private void SetTileConfig(TileConfiguration config, Vector3? scale = null)
    {
        TerrainName = config.TerrainName;
        SurfaceType = config.SurfaceType;
        HasLineOfSight = config.HasLineOfSight;
        StairsOrientation = config.IsStairs ? config.StairsOrientation : StairsOrientation.None;
        _travelCost = config.TravelCost;

        // Scale matters for diagonal stairs
        if (scale == null)
            scale = Vector3.one;

        var isFlippedX = scale.Value.x < 0;
        var isFlippedY = scale.Value.y < 0;

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
                    finalKey = key.Inverse();

                _blockExit[finalKey] = config.BlockExit[key];
            }

            foreach (var key in config.BlockEntrance.Keys)
            {
                var finalKey = key;
                if (key.IsHorizontal() && isFlippedX ||
                    key.IsVertical() && isFlippedY)
                    finalKey = key.Inverse();

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
}


[Serializable]
public class WorldCell
{
    public Vector2Int Position { get; }
    public int Height { get; set; }

    public event Action<Unit, Vector2Int> OnEnterCell;
    public event Action<Unit, Vector2Int> OnExitCell;

    private Unit unit;
    public Unit Unit
    {
        get { return unit; }
        set
        {
            if (unit != null)
                OnExitCell?.Invoke(unit, Position);

            unit = value;
            if (unit != null)
                OnEnterCell?.Invoke(unit, Position);
        }
    }

    private readonly Dictionary<int, WorldCellTile> _tilesByLayer = new Dictionary<int, WorldCellTile>();
    private readonly Dictionary<int, WorldCellTile> _overrideTilesByLayer = new Dictionary<int, WorldCellTile>();

    public WorldCell(Vector2Int position, int height)
    {
        Position = position;
        Height = height;
    }

    public static implicit operator WorldCell(Vector2Int position) => WorldGrid.Instance[position];

    public void AddTile(int sortingLayerId, TileConfiguration config, Vector3 scale)
    {
        Debug.Assert(!_tilesByLayer.ContainsKey(sortingLayerId));
        _tilesByLayer.Add(sortingLayerId, new WorldCellTile(config, scale));
    }

    public void OverrideTile(int sortingLayerId, WorldCellTile tile)
    {
        _overrideTilesByLayer[sortingLayerId] = tile;
    }

    public void ClearOverride(int sortingLayerId)
    {
        _overrideTilesByLayer.Remove(sortingLayerId);
    }

    public WorldCellTile TileAtSortingLayer(int sortingLayerId)
    {
        if (_overrideTilesByLayer.ContainsKey(sortingLayerId))
            return _overrideTilesByLayer[sortingLayerId];

        if (_tilesByLayer.ContainsKey(sortingLayerId))
            return _tilesByLayer[sortingLayerId];

        return WorldGrid.Instance.NullTile;
    }

    public StairsOrientation GetStairsOrientation(int sortingLayerId)
    {
        return TileAtSortingLayer(sortingLayerId).StairsOrientation;
    }

    public StairsOrientation GetStairsOrientation(Unit unit)
    {
        return GetStairsOrientation(unit.SortingLayerId);
    }

    public SurfaceType GetSurfaceType(int sortingLayerId)
    {
        return TileAtSortingLayer(sortingLayerId).SurfaceType;
    }

    public SurfaceType GetSurfaceType(Unit unit)
    {
        return GetSurfaceType(unit.SortingLayerId);
    }

    public string GetTerrainName(int sortingLayerId)
    {
        return TileAtSortingLayer(sortingLayerId).TerrainName;
    }

    public string GetTerrainName(Unit unit)
    {
        return GetTerrainName(unit.SortingLayerId);
    }

    public bool IsStairs(int sortingLayerId)
    {
        return TileAtSortingLayer(sortingLayerId).IsStairs;
    }

    public bool IsStairs(Unit unit)
    {
        return IsStairs(unit.SortingLayerId);
    }

    public bool HasLineOfSight(int sortingLayerId)
    {
        return TileAtSortingLayer(sortingLayerId).HasLineOfSight;
    }

    public bool HasLineOfSight(Unit unit)
    {
        return HasLineOfSight(unit.SortingLayerId);
    }

    public bool IsPassable(int sortingLayerId, UnitType unitType)
    {
        return TileAtSortingLayer(sortingLayerId).GetTravelCost(unitType) >= 0;
    }

    public bool IsPassable(Unit unit)
    {
        return IsPassable(unit.SortingLayerId, unit.UnitType);
    }

    public bool CanExit(Direction direction, int sortingLayerId, UnitType unitType)
    {
        return TileAtSortingLayer(sortingLayerId).CanExit(direction, unitType);
    }

    public bool CanExit(Direction direction, Unit unit)
    {
        return CanExit(direction, unit.SortingLayerId, unit.UnitType);
    }

    public bool CanEnter(Direction direction, int sortingLayerId, UnitType unitType)
    {
        return TileAtSortingLayer(sortingLayerId).CanEnter(direction, unitType);
    }

    public bool CanEnter(Direction direction, Unit unit)
    {
        return CanEnter(direction, unit.SortingLayerId, unit.UnitType);
    }

    public int GetTravelCost(int sortingLayerId, UnitType unitType)
    {
        return TileAtSortingLayer(sortingLayerId).GetTravelCost(unitType);
    }

    public int GetTravelCost(Unit unit)
    {
        return GetTravelCost(unit.SortingLayerId, unit.UnitType);
    }

    public int GetTravelCost(WorldCell destination, int sortingLayerId, UnitType unitType)
    {
        var offset = destination.Position - Position;
        if (offset.sqrMagnitude == 1)
            return destination.GetTravelCost(sortingLayerId, unitType);

        var bestCost = int.MaxValue;

        var middleCell = WorldGrid.Instance[Position + new Vector2Int(offset.x, 0)];
        if (CanMove(middleCell, sortingLayerId, unitType) &&
            middleCell.CanMove(destination, sortingLayerId, unitType))
        {
            bestCost = middleCell.GetTravelCost(sortingLayerId, unitType);
        }

        middleCell = WorldGrid.Instance[Position + new Vector2Int(0, offset.y)];
        if (CanMove(middleCell, sortingLayerId, unitType) &&
            middleCell.CanMove(destination, sortingLayerId, unitType))
        {
            bestCost = Mathf.Min(bestCost, middleCell.GetTravelCost(sortingLayerId, unitType));
        }

        if (bestCost == int.MaxValue)
            return -1;

        return bestCost + destination.GetTravelCost(sortingLayerId, unitType);
    }

    public int GetTravelCost(WorldCell destination, Unit unit)
    {
        return GetTravelCost(destination, unit.SortingLayerId, unit.UnitType);
    }

    public bool CanMove(WorldCell destination, int sortingLayerId, UnitType unitType)
    {
        var direction = GridUtility.GetDirection(Position, destination.Position, false, true);
        if (direction == Direction.None)
            return false;

        return direction.IsCardinal() ?
            CanMoveCardinalDirection(direction, destination, sortingLayerId, unitType) : CanMoveDiagonalDirection(destination, sortingLayerId, unitType);
    }

    public bool CanMove(WorldCell destination, Unit unit)
    {
        return CanMove(destination, unit.SortingLayerId, unit.UnitType);
    }

    private bool CanMoveCardinalDirection(Direction direction, WorldCell destination, int sortingLayerId, UnitType unitType)
    {
        return destination.IsPassable(sortingLayerId, unitType) && CanExit(direction, sortingLayerId, unitType) && destination.CanEnter(direction.Inverse(), sortingLayerId, unitType);
    }

    private bool CanMoveDiagonalDirection(WorldCell destination, int sortingLayerId, UnitType unitType)
    {
        return GetTravelCost(destination, sortingLayerId, unitType) >= 0;
    }
}