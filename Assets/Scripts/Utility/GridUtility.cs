using System;
using System.Collections.Generic;
using Tazdraperm.Utility;
using UnityEngine;


public static class GridUtility
{
    public const int MaxAttackRange = 4;
    public static readonly Vector2Int[][] AttackPositions;

    #region Neighbours Vectors

    // Legend:
    // [X] - random non-stairs tile
    // [R] - stairs tile with right-to-left orientation
    // [L] - stairs tile with left-to-right orientation
    // [T] - currently processed tile

    // When tile is not stairs tile and no stairs tiles nearby
    //[X][X][X]
    //[X][T][X]
    //[X][X][X]
    public static Vector2Int[] DefaultNeighboursOffsets =
    {
        Vector2Int.left,
        Vector2Int.up,
        Vector2Int.right,
        Vector2Int.down
    };

    // When tile is stairs tile with right-to-left orientation
    //[X][X][X]
    //[X][T=R][X]
    //[X][X][X]
    public static Vector2Int[] RightToLeftStairsNeighboursOffsets =
    {
        new Vector2Int(-1, 1),
        Vector2Int.up,
        new Vector2Int(1, -1),
        Vector2Int.down
    };

    // When tile is not stairs tile, but stairs tile with right-to-left orientation is in top-left position
    //[R][X][X]
    //[X][T][X]
    //[X][X][X]
    public static Vector2Int[] LeftHalfRightToLeftStairsNeighboursOffsets =
    {
        new Vector2Int(-1, 1),
        Vector2Int.up,
        Vector2Int.right,
        Vector2Int.down
    };

    // When tile is not stairs tile, but stairs tile with right-to-left orientation is in down-right position
    //[X][X][X]
    //[X][T][X]
    //[X][X][R]
    public static Vector2Int[] RightHalfRightToLeftStairsNeighboursOffsets =
    {
        new Vector2Int(1, -1),
        Vector2Int.up,
        Vector2Int.left,
        Vector2Int.down
    };

    // When tile is stairs tile with left-to-right orientation
    //[X][X][X]
    //[X][T=L][X]
    //[X][X][X]
    public static Vector2Int[] LeftToRightStairsNeighboursOffsets =
    {
        new Vector2Int(1, 1),
        Vector2Int.up,
        new Vector2Int(-1, -1),
        Vector2Int.down
    };

    // When tile is not stairs tile, but stairs tile with right-to-left orientation is in top-left position
    //[X][X][X]
    //[X][T][X]
    //[L][X][X]
    public static Vector2Int[] LeftHalfLeftToRightStairsNeighboursOffsets =
    {
        new Vector2Int(-1, -1),
        Vector2Int.up,
        Vector2Int.right,
        Vector2Int.down
    };

    // When tile is not stairs tile, but stairs tile with right-to-left orientation is in down-right position
    //[X][X][L]
    //[X][T][X]
    //[X][X][X]
    public static Vector2Int[] RightHalfLeftToRightStairsNeighboursOffsets =
    {
        new Vector2Int(1, 1),
        Vector2Int.up,
        Vector2Int.left,
        Vector2Int.down
    };

    #endregion
    

    static GridUtility()
    {
        AttackPositions = new Vector2Int[MaxAttackRange + 1][];
        AttackPositions[0] = new [] { Vector2Int.zero };

        for (var i = 1; i <= MaxAttackRange; i++)
        {
            AttackPositions[i] = new Vector2Int[i * 4];
            for (var j = 0; j < i; j++)
            {
                AttackPositions[i][j * 4] = new Vector2Int(i - j, j);
                AttackPositions[i][j * 4 + 1] = new Vector2Int(-i + j, -j);
                AttackPositions[i][j * 4 + 2] = new Vector2Int(-j, i - j);
                AttackPositions[i][j * 4 + 3] = new Vector2Int(j, -i + j);
            }
        }
    }

    // TODO: Use flags
    public static Direction GetDirection(Vector2Int from, Vector2Int to, bool roundToCardinal = false, bool allowDiagonalOutput = false)
    {
        var offset = to - from;
        if (roundToCardinal)
        {
            if (Math.Abs(offset.x) >= Math.Abs(offset.y))
            {
                offset.y = 0;
            }
            else
            {
                offset.x = 0;
            }
        }

        return DirectionExtensions.FromVector(offset, allowDiagonalOutput);
    }

    public static Vector2Int[] GetNeighboursOffsets(Vector2Int position)
    {
        var grid = WorldGrid.Instance;
        var cell = grid[position];
        switch (cell.StairsOrientation)
        {
            case StairsOrientation.RightToLeft:
                return RightToLeftStairsNeighboursOffsets;

            case StairsOrientation.LeftToRight:
                return LeftToRightStairsNeighboursOffsets;

            // TODO: Safety checks
            case StairsOrientation.None:
                if (grid[position + new Vector2Int(-1, 1)].StairsOrientation == StairsOrientation.RightToLeft)
                    return LeftHalfRightToLeftStairsNeighboursOffsets;
                if (grid[position + new Vector2Int(1, -1)].StairsOrientation == StairsOrientation.RightToLeft)
                    return RightHalfRightToLeftStairsNeighboursOffsets;
                if (grid[position + new Vector2Int(1, 1)].StairsOrientation == StairsOrientation.LeftToRight)
                    return RightHalfLeftToRightStairsNeighboursOffsets;
                if (grid[position + new Vector2Int(-1, -1)].StairsOrientation == StairsOrientation.LeftToRight)
                    return LeftHalfLeftToRightStairsNeighboursOffsets;
                break;
        }

        return DefaultNeighboursOffsets;
    }

    public static int GetBoxDistance(Vector2Int from, Vector2Int to)
    {
        return Mathf.Max(Mathf.Abs(from.x - to.x), Mathf.Abs(from.y - to.y));
    }

    public static Vector2Int SnapToGrid(MonoBehaviour monoBehaviour)
    {
        var cell = WorldGrid.Instance.Grid.WorldToCell(monoBehaviour.transform.position);
        monoBehaviour.transform.position = WorldGrid.Instance.Grid.GetCellCenterWorld(cell);
        return (Vector2Int) cell;
    }

    public static HashSet<Vector2Int> GetReachableCells(Unit unit, int movePoints = -1)
    {
        if (movePoints == -1)
        {
            movePoints = unit.CurrentMovementPoints;
        }

        var start = unit.GridPosition;
        var unitType = unit.UnitType;

        var queue = new Queue<Vector2Int>();
        var isolatedCells = new HashSet<Vector2Int>();
        var movementCost = new Dictionary<Vector2Int, int>();

        queue.Enqueue(start);
        movementCost[start] = 0;

        while (queue.Count > 0)
        {
            var currentPosition = queue.Dequeue();
            var offsets = GetNeighboursOffsets(currentPosition);

            for (var i = 0; i < offsets.Length; i++)
            {
                var offset = offsets[i];
                var neighbourPosition = currentPosition + offset;

                // Skip out of bounds cells
                //if (!WorldGrid.Instance.PointInGrid(neighbourPosition))
                //{
                //    continue;
                //}

                // Check if can move
                if (!WorldGrid.Instance[currentPosition].CanMove(neighbourPosition, unitType))
                {
                    continue;
                }

                // We can not pass through enemy units
                var otherUnit = WorldGrid.Instance[neighbourPosition].Unit;
                if (otherUnit != null && otherUnit.IsEnemy(unit))
                {
                    continue;
                }

                var neighbourCost = WorldGrid.Instance[currentPosition].GetTravelCost(neighbourPosition, unitType);
                var newCost = movementCost[currentPosition] + neighbourCost;

                // If new cost if more than we can move
                if (newCost > movePoints)
                {
                    continue;
                }

                var alreadyVisited = movementCost.TryGetValue(neighbourPosition, out var oldCost);
                if (!GetDirection(currentPosition, neighbourPosition, false, true).IsCardinal())
                {
                    if (!alreadyVisited)
                        isolatedCells.Add(neighbourPosition);
                }
                else
                {
                    isolatedCells.Remove(neighbourPosition);
                }

                // If new cost is more than old cost
                if (alreadyVisited && newCost >= oldCost)
                {
                    continue;
                }

                movementCost[neighbourPosition] = newCost;

                // Add cell to queue
                if (!queue.Contains(neighbourPosition))
                {
                    queue.Enqueue(neighbourPosition);
                }
            }
        }

        var result = new HashSet<Vector2Int>();
        var en = movementCost.GetEnumerator();
        while (en.MoveNext())
        {
            var key = en.Current.Key;
            var currentUnit = WorldGrid.Instance[key].Unit;
            if (currentUnit == null || currentUnit == unit)
                result.Add(key);
        }
        en.Dispose();

        foreach (var cell in isolatedCells)
        {
            if (!result.Contains(cell + Vector2Int.left) &&
                !result.Contains(cell + Vector2Int.right) &&
                !result.Contains(cell + Vector2Int.up) &&
                !result.Contains(cell + Vector2Int.down))
            {
                result.Remove(cell);
            }
        }

        return result;
    }

    public static List<Vector2Int> GetAttackableCells(Unit unit, HashSet<Vector2Int> movementCost, Weapon weapon = null)
    {
        if (weapon == null)
        {
            if (unit.HasWeapon)
                weapon = unit.EquippedWeapon;
            else
            {
                return new List<Vector2Int>();
            }
        }


        var minAttackRange = weapon.Stats[WeaponStat.MinRange].ValueInt;
        var maxAttackRange = weapon.Stats[WeaponStat.MaxRange].ValueInt;

        var borderCells = new List<Vector2Int>();
        var en = movementCost.GetEnumerator();
        while (en.MoveNext())
        {
            var key = en.Current;
            if (!movementCost.Contains(key + Vector2Int.left) ||
                !movementCost.Contains(key + Vector2Int.right) ||
                !movementCost.Contains(key + Vector2Int.up) ||
                !movementCost.Contains(key + Vector2Int.down))
            {
                borderCells.Add(key);
            }
        }
        en.Dispose();

        var result = new List<Vector2Int>();
        foreach (var cell in borderCells)
        {
            for (var i = 1; i <= maxAttackRange; i++)
            {
                foreach (var offset in AttackPositions[i])
                {
                    var position = cell + offset;
                    if (!result.Contains(position) &&
                        !movementCost.Contains(position) &&
                        WorldGrid.Instance.PointInGrid(position) &&
                        WorldGrid.Instance[position].Unit != null && WorldGrid.Instance[position].Unit.IsEnemy(unit) &&
                        CanAttack(position))

                        result.Add(position);
                }
            }
        }

        bool CanAttack(Vector2Int cell)
        {
            for (var i = minAttackRange; i <= maxAttackRange; i++)
            {
                foreach (var offset in AttackPositions[i])
                {
                    var position = cell + offset;
                    if (movementCost.Contains(position) &&
                        (WorldGrid.Instance[position].Unit == null || WorldGrid.Instance[position].Unit == unit) 
                        && HasLineOfSight(position, cell))
                        return true;
                }
            }

            return false;
        }

        return result;
    }

    public static GridPath FindPath(Unit unit, Vector2Int start, Vector2Int goal, int maxCost = int.MaxValue)
    {
        var unitType = unit.UnitType;

        var capacity = ((Mathf.Abs(goal.x - start.x) + 1) * (Mathf.Abs(goal.y - start.y) + 1)) >> 1;
        var frontier = new PriorityQueue<Vector2Int>(capacity);
        var runningCost = new Dictionary<Vector2Int, float>(capacity);
        var cameFrom = new Dictionary<Vector2Int, Vector2Int>(capacity);

        if (!WorldGrid.Instance.PointInGrid(goal) || !WorldGrid.Instance[goal].IsPassable(unitType))
        {
            return null;
        }

        frontier.Enqueue(start, 0f);
        runningCost[start] = 0f;

        while (frontier.Length > 0)
        {
            var currentPosition = frontier.Dequeue();

            if (currentPosition == goal)
            {
                var path = RestorePath(start, currentPosition, cameFrom);
                var travelCost = runningCost[currentPosition];
                var gridPath =  new GridPath(path, travelCost);
                return gridPath;
            }

            var currentCost = runningCost[currentPosition];
            var offsets = GetNeighboursOffsets(currentPosition);

            for (var i = 0; i < offsets.Length; i++)
            {
                var offset = offsets[i];
                var neighbourPosition = currentPosition + offset;

                // Check if not out of bounds
                //if (!WorldGrid.Instance.PointInGrid(neighbourPosition))
                //{
                //    continue;
                //}

                // Check if cell free
                var otherUnit = WorldGrid.Instance[neighbourPosition].Unit;
                if (otherUnit != null && otherUnit.IsEnemy(unit))
                {
                    continue;
                }

                // Check if can move
                if (!WorldGrid.Instance[currentPosition].CanMove(neighbourPosition, unitType))
                {
                    continue;
                }

                var neighbourCost = WorldGrid.Instance[currentPosition].GetTravelCost(neighbourPosition, unitType);
                var newCost = currentCost + neighbourCost;
                if (newCost > maxCost || runningCost.TryGetValue(neighbourPosition, out var oldCost) && newCost > oldCost)
                {
                    continue;
                }

                runningCost[neighbourPosition] = newCost;
                cameFrom[neighbourPosition] = currentPosition;

                var heuristic = ManhattanHeuristicWithInverseTieBreaks(neighbourPosition, goal);
                var priority = newCost + heuristic;

                if (frontier.Contains(neighbourPosition))
                {
                    frontier[neighbourPosition] = priority;
                }
                else
                {
                    frontier.Enqueue(neighbourPosition, priority);
                }
            }
        }

        return null;
    }

    public static GridPath FindPath(Unit unit, Vector2Int goal, int maxCost = int.MaxValue)
    {
        return FindPath(unit, unit.GridPosition, goal, maxCost);
    }

    public static bool HasLineOfSight(Vector2Int start, Vector2Int end, Func<Vector2Int, bool> validator, out GridLine gridLine)
    {
        gridLine = new GridLine(start, end);

        if (!validator(end))
        {
            return false;
        }

        // Nearest cells always visible
        if ((end - start).sqrMagnitude == 1)
        {
            return true;
        }

        Vector2Int? prevPoint = null;
        foreach (var point in gridLine)
        {
            // Check distance from last point with LOS to current point
            // If it's greater than 1, then LOS is blocked
            if (prevPoint != null && GetBoxDistance(prevPoint.Value, point) > 1)
            {
                return false;
            }

            // Skip point with out LOS
            if (!validator(point))
            {
                continue;
            }

            // Do additional checks for diagonals
            if (prevPoint != null && prevPoint.Value.x != point.x && prevPoint.Value.y != point.y && // If direction from previous is diagonal
                !validator(new Vector2Int(point.x, prevPoint.Value.y)) && !validator(new Vector2Int(prevPoint.Value.x, point.y))) // If diagonal movement is blocked
            {
                return false;
            }

            prevPoint = point;
        }

        return true;
    }

    public static bool HasLineOfSight(Vector2Int start, Vector2Int end)
    {
        return HasLineOfSight(start, end, DefaultCellLOSValidator, out _);
    }

    public static float ManhattanHeuristic(Vector2Int from, Vector2Int to)
    {
        return Mathf.Abs(from.x - to.x) + Mathf.Abs(from.y - to.y);
    }

    public static float ManhattanHeuristicWithTieBreaks(Vector2Int from, Vector2Int to)
    {
        return Mathf.Abs(from.x - to.x) + Mathf.Abs(from.y - to.y) + EuclideanHeuristic(from, to) * 0.0001f;
    }

    public static float ManhattanHeuristicWithInverseTieBreaks(Vector2Int from, Vector2Int to)
    {
        return Mathf.Abs(from.x - to.x) + Mathf.Abs(from.y - to.y) - EuclideanHeuristic(from, to) * 0.0001f;
    }

    public static float EuclideanHeuristic(Vector2Int from, Vector2Int to)
    {
        return (from - to).magnitude;
    }

    public static bool DefaultCellLOSValidator(Vector2Int cell) => WorldGrid.Instance[cell].HasLineOfSight;

    private static List<Vector2Int> RestorePath(Vector2Int start, Vector2Int goal, Dictionary<Vector2Int, Vector2Int> cameFrom)
    {
        var result = new List<Vector2Int>();
        var current = goal;
        while (current != start)
        {
            result.Insert(0, current);
            current = cameFrom[current];
        }

        return result;
    }
}