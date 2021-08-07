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

    public List<OriginalTileTravelCost> originalTravelCosts = new List<OriginalTileTravelCost>();

    private Dictionary<UnitType, int> _travelCost = new Dictionary<UnitType, int>();
    private Dictionary<Direction, UnitType> _blockExit = new Dictionary<Direction, UnitType>();
    private Dictionary<Direction, UnitType> _blockEntrance = new Dictionary<Direction, UnitType>();

    public WorldCellTile(TileConfiguration tileConfig, Vector3 scale)
    {
        SetTileConfig(tileConfig, scale);

        foreach (var entry in _travelCost)
        {
            var originalTravelCost = new OriginalTileTravelCost(entry.Key, entry.Value);
            originalTravelCosts.Add(originalTravelCost);
        }
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

    public void SetTravelCost(UnitType unitType, int travelCost) => _travelCost[unitType] = travelCost;

    public void RevertTravelCosts()
    {
        _travelCost = new Dictionary<UnitType, int>();

        foreach (var originalTravelCost in originalTravelCosts)
            _travelCost.Add(originalTravelCost.UnitType, originalTravelCost.TravelCost);
    }

    public bool IsPassable(UnitType unitType) => GetTravelCost(unitType) >= 0;

    public bool CanExit(Direction direction, UnitType unitType) => 
        direction != Direction.None && !(_blockExit.TryGetValue(direction, out var blockedType) && (blockedType & unitType) == unitType);

    public bool CanEnter(Direction direction, UnitType unitType) => 
        direction != Direction.None && !(_blockEntrance.TryGetValue(direction, out var blockedType) && (blockedType & unitType) == unitType);
}


[Serializable]
public class WorldCell
{
    public Vector2Int Position { get; }
    public int Height { get; set; }

    public event Action<Unit, Vector2Int> OnEnterCell;
    public event Action<Unit, Vector2Int> OnExitCell;

    private List<Unit> _deadUnits = new List<Unit>();

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

    #region Combat-Related Gameplay

    // Seize Objective related
    private bool _canBeSeized = false;
    public bool CanBeSeized { get => _canBeSeized; }

    private Unit _seizedBy;
    public Unit SeizedBy { get => _seizedBy;  }

    private List<int> _unitsAllowedToSeize = new List<int>();


    // Escape Objective related
    private bool _isEscapePoint = false;
    public bool IsEscapePoint { get => _isEscapePoint; }
    private List<int> _unitsAllowedToEscape = new List<int>();

    private List<Unit> _escapees = new List<Unit>();
    public List<Unit> Escapees { get => _escapees;  }
    #endregion


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
        {
            return _overrideTilesByLayer[sortingLayerId];
        }

        if (_tilesByLayer.ContainsKey(sortingLayerId))
        {
            return _tilesByLayer[sortingLayerId];
        }

        return WorldGrid.Instance.NullTile;
    }


    public void MarkSeizeable(List<int> unitTeamsWhoCanSeize)
    {
        _canBeSeized = true;

        _unitsAllowedToSeize = unitTeamsWhoCanSeize;
    }

    public void Seize(Unit seizingUnit)
    {
        if (!CanBeSeized)
            throw new Exception($"[WorldCell] Cell Position: [{Position.x}, {Position.y}] is not seizeable!");

        if (!_unitsAllowedToSeize.Contains(seizingUnit.TeamId))
            throw new Exception($"[WorldCell] Unit: {seizingUnit.Name} is not the allowed teams to seize!");

        _seizedBy = seizingUnit;
    }

    public void MarkEscapable(List<int> unitTeamsAllowedToEscape)
    {
        _isEscapePoint = true;

        _unitsAllowedToEscape = unitTeamsAllowedToEscape;
    }

    public void Escape(Unit escapingUnit)
    {
        if (!IsEscapePoint)
            throw new Exception($"[WorldCell] Cell Position: [{Position.x}, {Position.y}] is not an escape point!");

        if (!_unitsAllowedToEscape.Contains(escapingUnit.TeamId))
            throw new Exception($"[WorldCell] Unit: {escapingUnit.Name} is not the allowed teams to seize!");

        if (_escapees.Contains(escapingUnit))
            throw new Exception($"[WorldCell] Unit: {escapingUnit.Name} has already escaped!");

        _escapees.Add(Unit);
    }

    public StairsOrientation GetStairsOrientation(int sortingLayerId) => TileAtSortingLayer(sortingLayerId).StairsOrientation;

    public SurfaceType GetSurfaceType(int sortingLayerId) => TileAtSortingLayer(sortingLayerId).SurfaceType;

    public string GetTerrainName(int sortingLayerId) => TileAtSortingLayer(sortingLayerId).TerrainName;

    public bool IsStairs(int sortingLayerId) => TileAtSortingLayer(sortingLayerId).IsStairs;

    public bool HasLineOfSight(int sortingLayerId) => TileAtSortingLayer(sortingLayerId).HasLineOfSight;

    /// <summary>
    /// Returns true if the unit type can walk on this tile, and dead units on this tile is less than 2
    /// </summary>
    public bool IsPassable(int sortingLayerId, UnitType unitType) => TileAtSortingLayer(sortingLayerId).GetTravelCost(unitType) >= 0 && _deadUnits.Count < 2;

    public bool CanExit(Direction direction, int sortingLayerId, UnitType unitType) => TileAtSortingLayer(sortingLayerId).CanExit(direction, unitType);

    public bool CanEnter(Direction direction, int sortingLayerId, UnitType unitType) => TileAtSortingLayer(sortingLayerId).CanEnter(direction, unitType);

    public int GetTravelCost(int sortingLayerId, UnitType unitType) => TileAtSortingLayer(sortingLayerId).GetTravelCost(unitType);

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

    public bool CanMove(WorldCell destination, int sortingLayerId, UnitType unitType)
    {
        var direction = GridUtility.GetDirection(Position, destination.Position, false, true);
        if (direction == Direction.None)
            return false;

        return direction.IsCardinal() ?
            CanMoveCardinalDirection(direction, destination, sortingLayerId, unitType) : CanMoveDiagonalDirection(destination, sortingLayerId, unitType);
    }

    private bool CanMoveCardinalDirection(Direction direction, WorldCell destination, int sortingLayerId, UnitType unitType) => 
        destination.IsPassable(sortingLayerId, unitType) && CanExit(direction, sortingLayerId, unitType) && destination.CanEnter(direction.Inverse(), sortingLayerId, unitType);

    private bool CanMoveDiagonalDirection(WorldCell destination, int sortingLayerId, UnitType unitType) => 
        GetTravelCost(destination, sortingLayerId, unitType) >= 0;

    public void AddUnitToLootPile(Unit unit) => _deadUnits.Add(unit);

    public void RemoveUnitFromLootPile(Unit unit)
    {
        for(int i = 0; i < _deadUnits.Count; i++)
        {
            if (_deadUnits[i] == unit)
                _deadUnits.RemoveAt(i);
        }
    }

    public void DestroyCorpsesOnLootPile()
    {
        for(int i = 0; i < _deadUnits.Count; i++)
        {
            //TODO: Do whatever we want to dead units, be it destroying them, or keeping them there but removing all interactive elements 
        }
    }

    public int LootableBodiesCount() => _deadUnits.Count;

    public List<Unit> LootableBodies => _deadUnits;
}