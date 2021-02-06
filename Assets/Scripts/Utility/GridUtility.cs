using System;
using System.Collections.Generic;
using System.Linq;
using Tazdraperm.Utility;
using UnityEngine;

public static class GridUtility
{
    public static Direction GetDirection(Vector2Int from, Vector2Int to)
    {
        var offset = to - @from;

        if (Math.Abs(offset.magnitude - 1) > 0.001f)
        {
            //Debug.LogError("Incorrect direction. Cells must be adjacent.");
            return Direction.None;
        }

        if (offset.x != 0)
        {
            return offset.x > 0 ? Direction.Right : Direction.Left;
        }

        return offset.y > 0 ? Direction.Up : Direction.Down;
    }

    public static Vector2Int SnapToGrid(MonoBehaviour monoBehaviour)
    {
        var cell = WorldGrid.Instance.Grid.WorldToCell(monoBehaviour.transform.position);
        monoBehaviour.transform.position = WorldGrid.Instance.Grid.GetCellCenterWorld(cell);
        return (Vector2Int) cell;
    }

    public static List<Vector2Int> GetReachableCells(Unit unit, out List<Vector2Int> attackableCells, int movePoints = -1, int minAttackRange = 0, int maxAttackRange = 0)
    {
        var t = Time.realtimeSinceStartup;
        if (movePoints == -1)
        {
            movePoints = unit.MovePoints;
        }

        if (minAttackRange == 0 || maxAttackRange == 0)
        {
            minAttackRange = 1; // Unit.Weapon.MinAttackRange
            maxAttackRange = 1; // // Unit.Weapon.MaxAttackRange
        }

        var start = unit.GridPosition;
        var unitType = unit.UnitType;

        var queue = new Queue<Vector2Int>();
        var movementCost = new Dictionary<Vector2Int, int>();

        queue.Enqueue(start);
        movementCost[start] = 0;

        while (queue.Count > 0)
        {
            var currentPosition = queue.Dequeue();

            foreach (var directions in DirectionExtensions.AllDirections)
            {
                var offset = directions.ToVector();
                var neighbourPosition = currentPosition + offset;

                // Skip out of bounds cells
                if (!WorldGrid.Instance.PointInGrid(neighbourPosition))
                {
                    continue;
                }

                // Check if can move
                if (!WorldGrid.Instance[currentPosition].CanMove(neighbourPosition, unitType))
                {
                    continue;
                }

                // We can not pass through enemy units
                if (WorldGrid.Instance[neighbourPosition].Unit != null)
                {
                    // TODO: Do this ONLY for enemy units
                    continue;
                }

                // Calculate new travel cost for the cell
                var neighbourCellCost = WorldGrid.Instance[neighbourPosition].TravelCost(unitType);
                var newTravelCost = movementCost[currentPosition] + neighbourCellCost;

                // If new cost if more than we can move or we cost is more than old cost
                if (newTravelCost > movePoints ||
                    movementCost.TryGetValue(neighbourPosition, out var neighbourCost) && newTravelCost >= neighbourCost)
                {
                    continue;
                }

                movementCost[neighbourPosition] = newTravelCost;

                // Add cell to queue
                if (!queue.Contains(neighbourPosition))
                {
                    queue.Enqueue(neighbourPosition);
                }
            }
        }

        Debug.Log(Time.realtimeSinceStartup - t);

        var reachableCells =  movementCost.Keys
            .Where(key => WorldGrid.Instance[key].Unit == unit || WorldGrid.Instance[key].Unit == null)
            .ToList();

        var borderCells = reachableCells.Where((cell) => 
                                                         !reachableCells.Contains(cell + Vector2Int.left) ||
                                                         !reachableCells.Contains(cell + Vector2Int.right) ||
                                                         !reachableCells.Contains(cell + Vector2Int.up) ||
                                                         !reachableCells.Contains(cell + Vector2Int.down));

        movementCost.Clear();
        foreach (var cell in borderCells)
        {
            queue.Enqueue(cell);
            movementCost[cell] = 0;
        }

        while (queue.Count > 0)
        {
            var currentPosition = queue.Dequeue();

            foreach (var directions in DirectionExtensions.AllDirections)
            {
                var offset = directions.ToVector();
                var neighbourPosition = currentPosition + offset;

                // Skip out of bounds cells
                if (!WorldGrid.Instance.PointInGrid(neighbourPosition))
                {
                    continue;
                }

                // Calculate new travel cost for the cell
                var newTravelCost = movementCost[currentPosition] + 1;

                // If new cost if more than we can move or we cost is more than old cost
                if (newTravelCost > maxAttackRange ||
                    movementCost.TryGetValue(neighbourPosition, out var neighbourCost) && newTravelCost >= neighbourCost)
                {
                    continue;
                }

                movementCost[neighbourPosition] = newTravelCost;

                // Add cell to queue
                if (!queue.Contains(neighbourPosition))
                {
                    queue.Enqueue(neighbourPosition);
                }
            }
        }

        attackableCells = movementCost.Keys
            .Where(cell => !reachableCells.Contains(cell))
            .ToList();
        return reachableCells;
    }

    public static List<Vector2Int> GetAttackableCells(Unit unit, List<Vector2Int> reachableCells, int minAttackRange = 0, int maxAttackRange = 0)
    {
        var t = Time.realtimeSinceStartup;

        if (minAttackRange == 0 || maxAttackRange == 0)
        {
            minAttackRange = 1; // Unit.Weapon.MinAttackRange
            maxAttackRange = 1; // // Unit.Weapon.MaxAttackRange
        }

        var queue = new Stack<(Vector2Int, int)>();
        // var cost = new Dictionary<Vector2Int, int>();
        var canAttack = new HashSet<Vector2Int>();

        foreach (var cell in reachableCells)
        {
            queue.Push((cell, 0));
            //cost[cell] = 0;
        }

        while (queue.Count > 0)
        {
            var (currentPosition, currentCost) = queue.Pop();
            foreach (var direction in DirectionExtensions.AllDirections)
            {
                var offset = direction.ToVector();
                var neighbourPosition = currentPosition + offset;

                if (!WorldGrid.Instance.PointInGrid(neighbourPosition))
                {
                    continue;
                }

                if (!WorldGrid.Instance[neighbourPosition].IsPassable())
                {
                    continue;
                }

                // TODO: Check LOS!

                // Calculate new cost for the cell
                var newCost = currentCost + 1;
                // If new cost if more than we can attack or new cost is worse than old one
                if (newCost > maxAttackRange)// || 
                    //cost.TryGetValue(neighbourPosition, out var oldCost) && newCost >= oldCost)
                {
                    continue;
                }

                if (!reachableCells.Contains(neighbourPosition) &&
                    newCost >= minAttackRange && newCost <= maxAttackRange)
                {
                    canAttack.Add(neighbourPosition);
                }

                //if (!cost.TryGetValue(neighbourPosition, out var oldCost) || newCost < oldCost)
                {
                //    cost[neighbourPosition] = newCost;
                }

                // Add cell to queue
                if (!queue.Contains((neighbourPosition, newCost)))
                {
                    queue.Push((neighbourPosition, newCost));
                }
            }
        }

        Debug.Log(Time.realtimeSinceStartup - t);
        return canAttack.ToList();
    }

    public static GridPath FindPath(Unit unit, Vector2Int start, Vector2Int goal)
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
                var path = RestorePath(start, goal, cameFrom);
                var travelCost = runningCost[goal];
                return new GridPath(path, travelCost);
            }

            var currentCost = runningCost[currentPosition];
            foreach (var direction in DirectionExtensions.AllDirections)
            {
                var neighbourPosition = currentPosition + direction.ToVector();

                // Check if not out of bounds
                if (!WorldGrid.Instance.PointInGrid(neighbourPosition))
                {
                    continue;
                }

                // Check if cell free
                var otherUnit = WorldGrid.Instance[neighbourPosition].Unit;
                if (otherUnit != null)
                {
                    continue;
                }

                // Check if can move
                if (!WorldGrid.Instance[currentPosition].CanMove(neighbourPosition, unitType))
                {
                    continue;
                }

                var neighbourCost = WorldGrid.Instance[neighbourPosition].TravelCost(unitType);
                var newCost = currentCost + neighbourCost;
                if (runningCost.TryGetValue(neighbourPosition, out var oldCost) && newCost >= oldCost)
                {
                    continue;
                }

                runningCost[neighbourPosition] = newCost;
                cameFrom[neighbourPosition] = currentPosition;

                var priority = newCost + GridUtility.ManhattanHeuristicWithTieBreaks(neighbourPosition, goal);

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

    public static GridPath FindPath(Unit unit, Vector2Int goal)
    {
        return FindPath(unit, unit.GridPosition, goal);
    }

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

    public static float ManhattanHeuristic(Vector2Int from, Vector2Int to)
    {
        return Mathf.Abs(@from.x - to.x) + Mathf.Abs(@from.y - to.y);
    }

    public static float ManhattanHeuristicWithTieBreaks(Vector2Int from, Vector2Int to)
    {
        return Mathf.Abs(@from.x - to.x) + Mathf.Abs(@from.y - to.y) + EuclideanHeuristic(@from, to) * 0.0001f;
    }

    public static float EuclideanHeuristic(Vector2Int from, Vector2Int to)
    {
        return (@from - to).magnitude;
    }
}