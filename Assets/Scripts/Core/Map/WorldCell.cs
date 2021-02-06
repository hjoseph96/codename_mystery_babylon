using UnityEngine;

public class WorldCell
{
    public Vector2Int Position { get; }
    public TileConfiguration Config { get; }

    public Unit Unit { get; set; }

    public WorldCell(Vector2Int position, TileConfiguration config)
    {
        Position = position;
        Config = config;
    }

    public static implicit operator WorldCell(Vector2Int position)
    {
        return WorldGrid.Instance[position];
    }

    public int TravelCost(UnitType unitType)
    {
        if (Config.TravelCost.TryGetValueExt(unitType, out var result))
            return result;

        return -1;
    }

    public bool IsPassable(UnitType unitType) => TravelCost(unitType) >= 0;

    public bool IsPassable()
    {
        return IsPassable(UnitType.Unmounted) ||
               IsPassable(UnitType.Mounted) ||
               IsPassable(UnitType.Air);
    }

    public bool CanExit(Direction direction, UnitType unitType)
    {
        return direction != Direction.None && !(Config.BlockExit.TryGetValue(direction, out var blockedType) &&
                                                (blockedType & unitType) == unitType);
    }

    public bool CanEnter(Direction direction, UnitType unitType)
    {
        return direction != Direction.None && !(Config.BlockEntrance.TryGetValue(direction, out var blockedType) &&
                                                (blockedType & unitType) == unitType);
    }

    public bool CanMove(WorldCell destination, UnitType unitType)
    {
        var direction = GridUtility.GetDirection(Position, destination.Position);
        if (direction == Direction.None)
            return false;

        return destination.IsPassable(unitType) && CanExit(direction, unitType) && destination.CanEnter(direction.Inverse(), unitType);
    }
        
}