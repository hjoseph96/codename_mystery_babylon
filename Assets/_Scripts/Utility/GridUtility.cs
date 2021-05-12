using System;
using System.Collections.Generic;
using Tazdraperm.Utility;
using UnityEngine;


public static class GridUtility
{
    public const int MaxAttackRange = 6;
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


    public static Vector2Int[] DiagonalNeighborOffsets =
    {
        new Vector2Int(-1, 1),
        new Vector2Int(1, 1),
        new Vector2Int(-1, 1),
        new Vector2Int(1, -1),
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
        AttackPositions[0] = new[] { Vector2Int.zero };

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

    // TODO: Use flags, because using roundToCardinal and allowDiagonalOutput does not make sense
    /// <summary>
    /// Returns direction between two points
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="roundToCardinal">If set, returned direction will be rounded to cardinal</param>
    /// <param name="allowDiagonalOutput">If set, returned direction can be diagonal</param>
    /// <returns>Direction</returns>
    public static Direction GetDirection(Vector2Int from, Vector2Int to, bool roundToCardinal = false, bool allowDiagonalOutput = false)
    {
        /*
         Example: from (0, 0), to (1, 1) => Direction.None
         Example: from (0, 0), to (1, 1), roundToCardinal = true => Direction.Right
         Example: from (0, 0), to (-5, 1), roundToCardinal = true => Direction.Left
         Example: from (0, 0), to (1, 1), allowDiagonalOutput = true => Direction.RightUp
         */

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

    /// <summary>
    /// Used to get an array of neighbour offsets for a specific point in grid with respect of stairs<br/>
    /// For non-stairs tile with non-stairs neighbours result is an array of fours cardinal directions<br/>
    /// Mostly used for pathfinding
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public static Vector2Int[] GetNeighboursOffsets(int sortingLayerId, Vector2Int position)
    {
        var grid = WorldGrid.Instance;
        var cell = grid[position];
        switch (cell.GetStairsOrientation(sortingLayerId))
        {
            case StairsOrientation.RightToLeft:
                return RightToLeftStairsNeighboursOffsets;

            case StairsOrientation.LeftToRight:
                return LeftToRightStairsNeighboursOffsets;

            case StairsOrientation.None:
                var leftHalfRightToLeftOffset = position + new Vector2Int(-1, 1);

                if (grid.PointInGrid(leftHalfRightToLeftOffset) && grid[leftHalfRightToLeftOffset].GetStairsOrientation(sortingLayerId) == StairsOrientation.RightToLeft)
                    return LeftHalfRightToLeftStairsNeighboursOffsets;

                var rightHalfRightToLeftOffset = position + new Vector2Int(1, -1);
                if (grid.PointInGrid(rightHalfRightToLeftOffset) && grid[rightHalfRightToLeftOffset].GetStairsOrientation(sortingLayerId) == StairsOrientation.RightToLeft)
                    return RightHalfRightToLeftStairsNeighboursOffsets;

                var rightHalfLeftToRightOffset = position + new Vector2Int(1, 1);
                if (grid.PointInGrid(rightHalfLeftToRightOffset) && grid[rightHalfLeftToRightOffset].GetStairsOrientation(sortingLayerId) == StairsOrientation.LeftToRight)
                    return RightHalfLeftToRightStairsNeighboursOffsets;

                var leftHalfLeftToRightOffset = position + new Vector2Int(-1, -1);
                if (grid.PointInGrid(leftHalfLeftToRightOffset) && grid[leftHalfLeftToRightOffset].GetStairsOrientation(sortingLayerId) == StairsOrientation.LeftToRight)
                    return LeftHalfLeftToRightStairsNeighboursOffsets;

                break;
        }

        return DefaultNeighboursOffsets;
    }

    public static List<Vector2Int> GetNeighbours(int sortingLayerId, Vector2Int gridPosition)
    {
        var neighbors = new List<Vector2Int>();

        var offsets = GetNeighboursOffsets(sortingLayerId, gridPosition);
        foreach (Vector2Int offset in offsets)
        {
            var neighbor = gridPosition + offset;
            if (WorldGrid.Instance.PointInGrid(neighbor))
                neighbors.Add(gridPosition + offset);
        }

        return neighbors;
    }

    public static List<Vector2Int> GetDiagonalNeigbors(Vector2Int gridPosition)
    {
        var diagonals = new List<Vector2Int>();

        foreach (Vector2Int offset in DiagonalNeighborOffsets)
        {
            var diagonal = gridPosition + offset;
            if (WorldGrid.Instance.PointInGrid(diagonal))
                diagonals.Add(gridPosition + offset);
        }

        return diagonals;
    }

    public static int GetBoxDistance(Vector2Int from, Vector2Int to)
    {
        return Mathf.Max(Mathf.Abs(from.x - to.x), Mathf.Abs(from.y - to.y));
    }

    /// <summary>
    /// Snaps MonoBehavior's GameObject to WorldGrid
    /// </summary>
    /// <param name="monoBehaviour"></param>
    /// <returns>Resulting grid position</returns>
    public static Vector2Int SnapToGrid(MonoBehaviour monoBehaviour)
    {
        var cell = WorldGrid.Instance.Grid.WorldToCell(monoBehaviour.transform.position);
        monoBehaviour.transform.position = WorldGrid.Instance.Grid.GetCellCenterWorld(cell);
        return (Vector2Int)cell;
    }

    /// <summary>
    /// Finds all grid cells than can be reached by unit
    /// </summary>
    /// <param name="unit"></param>
    /// <param name="movePoints">Available movement points. If to -1, unit's current movement points value will be used</param>
    /// <returns></returns>
    public static HashSet<Vector2Int> GetReachableCells(Unit unit, int movePoints = -1, bool fullRadiusMode = false, bool includeEnemy = false)
    {
        if (movePoints == -1)
            movePoints = unit.CurrentMovementPoints;

        var start = unit.GridPosition;
        var unitType = unit.UnitType;

        var queue = new Queue<Vector2Int>();
        var isolatedCells = new HashSet<Vector2Int>();
        var movementCost = new Dictionary<Vector2Int, int>();

        queue.Enqueue(start);
        movementCost[start] = 0;

        var _worldGrid = WorldGrid.Instance;

        // We use breadth-first search to find all reachable cells
        while (queue.Count > 0)
        {
            var currentPosition = queue.Dequeue();
            var offsets = GetNeighboursOffsets(unit.SortingLayerId, currentPosition);

            for (var i = 0; i < offsets.Length; i++)
            {
                var offset = offsets[i];
                var neighbourPosition = currentPosition + offset;

                // Skip out of bounds cells
                var currentPositionIsInGrid = _worldGrid.PointInGrid(currentPosition);
                var neighborIsInGrid = _worldGrid.PointInGrid(neighbourPosition);
                if (!currentPositionIsInGrid || !neighborIsInGrid)
                    continue;


                // Check if can move
                if (!_worldGrid[currentPosition].CanMove(neighbourPosition, unit) && !fullRadiusMode)
                    continue;

                // We can not pass through enemy units
                var otherUnit = _worldGrid[neighbourPosition].Unit;

                if (otherUnit != null)
                    if (otherUnit.IsEnemy(unit) && !fullRadiusMode && !includeEnemy)
                        continue;

                var neighbourCost = _worldGrid[currentPosition].GetTravelCost(neighbourPosition, unit);
                var newCost = movementCost[currentPosition] + neighbourCost;

                //// If new cost if more than we can move
                if (newCost > movePoints)
                    continue;

                // We maintain a list of potentially isolated cells. Isolated cell is a such cell than can only be reached diagonally
                var alreadyVisited = movementCost.TryGetValue(neighbourPosition, out var oldCost);
                if (!GetDirection(currentPosition, neighbourPosition, false, true).IsCardinal())
                    if (!alreadyVisited)
                        isolatedCells.Add(neighbourPosition);
                    else
                        isolatedCells.Remove(neighbourPosition);

                // If new cost is more than old cost
                if (alreadyVisited && newCost >= oldCost)
                    continue;

                movementCost[neighbourPosition] = newCost;

                // Add cell to queue
                if (!queue.Contains(neighbourPosition))
                    queue.Enqueue(neighbourPosition);
            }
        }

        // Access enumerator manually for performance reasons
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

        // Exclude isolated cells
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

    /// <summary>
    /// Finds all grid cells than can be reached by unit
    /// </summary>
    /// <param name="unit"></param>
    /// <param name="movePoints">Available movement points. If to -1, unit's current movement points value will be used</param>
    /// <returns></returns>
    public static HashSet<Vector2Int> GetCellsInRadius(Vector2Int origin, int movePoints)
    {

        var start = origin;


        var queue = new Queue<Vector2Int>();
        var isolatedCells = new HashSet<Vector2Int>();
        var movementCost = new Dictionary<Vector2Int, int>();

        queue.Enqueue(start);
        movementCost[start] = 0;

        var _worldGrid = WorldGrid.Instance;

        // We use breadth-first search to find all reachable cells
        while (queue.Count > 0)
        {
            var currentPosition = queue.Dequeue();
            var offsets = GetNeighboursOffsets(0, currentPosition);

            for (var i = 0; i < offsets.Length; i++)
            {
                var offset = offsets[i];
                var neighbourPosition = currentPosition + offset;

                // Skip out of bounds cells
                var currentPositionIsInGrid = _worldGrid.PointInGrid(currentPosition);
                var neighborIsInGrid = _worldGrid.PointInGrid(neighbourPosition);
                if (!currentPositionIsInGrid || !neighborIsInGrid)
                    continue;


                var neighbourCost = 1;
                var newCost = movementCost[currentPosition] + neighbourCost;

                //// If new cost if more than we can move
                if (newCost > movePoints)
                    continue;

                // We maintain a list of potentially isolated cells. Isolated cell is a such cell than can only be reached diagonally
                var alreadyVisited = movementCost.TryGetValue(neighbourPosition, out var oldCost);
                if (!GetDirection(currentPosition, neighbourPosition, false, true).IsCardinal())
                    if (!alreadyVisited)
                        isolatedCells.Add(neighbourPosition);
                    else
                        isolatedCells.Remove(neighbourPosition);

                // If new cost is more than old cost
                if (alreadyVisited && newCost >= oldCost)
                    continue;

                movementCost[neighbourPosition] = newCost;

                // Add cell to queue
                if (!queue.Contains(neighbourPosition))
                    queue.Enqueue(neighbourPosition);
            }
        }

        // Access enumerator manually for performance reasons
        var result = new HashSet<Vector2Int>();
        var en = movementCost.GetEnumerator();
        while (en.MoveNext())
        {
            var key = en.Current.Key;
            result.Add(key);
        }
        en.Dispose();

        // Exclude isolated cells
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

    /// <summary>
    /// Finds all grid cells than can be reached by unit within a specified range of a target `GridPosition`
    /// </summary>
    /// <param name="targetCell"></param>
    /// <param name="radius">How wide to make the search</param>
    /// <param name="unit">Unit to check reachability for</param>
    /// <returns></returns>
    public static HashSet<Vector2Int> GetReachableCellNeighbours(Vector2Int targetCell, int radius, Unit unit)
    {
        var start = targetCell;
        var unitType = unit.UnitType;

        var queue = new Queue<Vector2Int>();
        var isolatedCells = new HashSet<Vector2Int>();
        var movementCost = new Dictionary<Vector2Int, int>();

        queue.Enqueue(start);
        movementCost[start] = 0;

        var _worldGrid = WorldGrid.Instance;

        // TODO: Warning! No safety (PointInGrid) checks! This is done to increase performance slightly
        // We use breadth-first search to find all reachable cells
        while (queue.Count > 0)
        {
            var currentPosition = queue.Dequeue();
            var offsets = GetNeighboursOffsets(unit.SortingLayerId, currentPosition);

            for (var i = 0; i < offsets.Length; i++)
            {
                var offset = offsets[i];
                var neighbourPosition = currentPosition + offset;

                // Skip out of bounds cells
                var currentPositionIsInGrid = _worldGrid.PointInGrid(currentPosition);
                var neighborIsInGrid = _worldGrid.PointInGrid(neighbourPosition);
                if (!currentPositionIsInGrid || !neighborIsInGrid)
                    continue;


                // Check if can move
                if (!_worldGrid[currentPosition].CanMove(neighbourPosition, unit))
                    continue;

                // We can not attack from a position that has a unit already there
                var otherUnit = _worldGrid[neighbourPosition].Unit;
                if (otherUnit != null)
                    continue;

                var neighbourCost = _worldGrid[currentPosition].GetTravelCost(neighbourPosition, unit);
                var newCost = movementCost[currentPosition] + neighbourCost;

                // If new cost if more than we can move
                if (newCost > radius)
                    continue;

                // We maintain a list of potentially isolated cells. Isolated cell is a such cell than can only be reached diagonally
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
                    continue;

                movementCost[neighbourPosition] = newCost;

                // Add cell to queue
                if (!queue.Contains(neighbourPosition))
                    queue.Enqueue(neighbourPosition);
            }
        }

        // Access enumerator manually for performance reasons
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

        // Exclude isolated cells
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

    /// <summary>
    /// Finds all grid cells than can be attacked by unit
    /// </summary>
    /// <param name="unit"></param>
    /// <param name="reachableCells">Reachable cells, usually just use value returned by GetReachableCells here</param>
    /// <param name="weapon">Weapon, which attack range should be taken into account. If set to null, unit's Equipped weapon will be used</param>
    /// <returns></returns>
    public static List<Vector2Int> GetAttackableCells(Unit unit, HashSet<Vector2Int> reachableCells, Weapon weapon = null)
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

        // First we determine border cells. Border cell is a such cell that is NOT surrounded from all four sides
        var borderCells = new List<Vector2Int>();
        var en = reachableCells.GetEnumerator();
        while (en.MoveNext())
        {
            var key = en.Current;
            if (!reachableCells.Contains(key + Vector2Int.left) ||
                !reachableCells.Contains(key + Vector2Int.right) ||
                !reachableCells.Contains(key + Vector2Int.up) ||
                !reachableCells.Contains(key + Vector2Int.down))
            {
                borderCells.Add(key);
            }
        }
        en.Dispose();

        /*
         Then for each border cell we check all cells in range of maxAttackRange from it
         We use pre-calculated AttackPositions
         Cell is valid if:
         1. Not in result
         2. Not in reachable cells (if we can move to it then there's no enemy)
         3. Is inside grid
         4. Contains enemy
         5. We CanAttack it
        */
        var result = new List<Vector2Int>();
        foreach (var cell in borderCells)
        {
            for (var i = 1; i <= maxAttackRange; i++)
            {
                foreach (var offset in AttackPositions[i])
                {
                    var position = cell + offset;
                    if (!result.Contains(position) &&
                        !reachableCells.Contains(position) &&
                        WorldGrid.Instance.PointInGrid(position) &&
                        WorldGrid.Instance[position].Unit != null && WorldGrid.Instance[position].Unit.IsEnemy(unit) &&
                        CanAttack(position))

                        result.Add(position);
                }
            }
        }

        // We CanAttack cell if we have a position around it that is between minAttackRange and maxAttackRange from it and we have LOS from this position to cell
        bool CanAttack(Vector2Int cell)
        {
            for (var i = minAttackRange; i <= maxAttackRange; i++)
            {
                foreach (var offset in AttackPositions[i])
                {
                    var position = cell + offset;
                    if (reachableCells.Contains(position) &&
                        (WorldGrid.Instance[position].Unit == null || WorldGrid.Instance[position].Unit == unit)
                        && HasLineOfSight(unit.SortingLayerId, position, cell))
                        return true;
                }
            }

            return false;
        }

        return result;
    }


    public static HashSet<Vector2Int> GetCellsWithinMovementRange(Unit unit, int movePoints = -1, bool fullRadiusMode = false, bool includeEnemy = false)
    {
        if (movePoints == -1)
            movePoints = unit.CurrentMovementPoints;

        var start = unit.GridPosition;
        var unitType = unit.UnitType;

        var queue = new Queue<Vector2Int>();
        var isolatedCells = new HashSet<Vector2Int>();
        var movementCost = new Dictionary<Vector2Int, int>();

        queue.Enqueue(start);
        movementCost[start] = 0;

        var _worldGrid = WorldGrid.Instance;

        // We use breadth-first search to find all reachable cells
        while (queue.Count > 0)
        {
            var currentPosition = queue.Dequeue();
            var offsets = GetNeighboursOffsets(unit.SortingLayerId, currentPosition);

            for (var i = 0; i < offsets.Length; i++)
            {
                var offset = offsets[i];
                var neighbourPosition = currentPosition + offset;

                // Skip out of bounds cells
                var currentPositionIsInGrid = _worldGrid.PointInGrid(currentPosition);
                var neighborIsInGrid = _worldGrid.PointInGrid(neighbourPosition);
                if (!currentPositionIsInGrid || !neighborIsInGrid)
                    continue;


                // Check if can move
                if (!_worldGrid[currentPosition].CanMove(neighbourPosition, unit) && !fullRadiusMode)
                    continue;

                // We can not pass through enemy units
                //var otherUnit = _worldGrid[neighbourPosition].Unit;

                //if (otherUnit != null)
                //    if (otherUnit.IsEnemy(unit) && !fullRadiusMode && !includeEnemy)
                //        continue;

                var neighbourCost = _worldGrid[currentPosition].GetTravelCost(neighbourPosition, unit);
                var newCost = movementCost[currentPosition] + neighbourCost;

                // If new cost if more than we can move
                if (newCost > movePoints)
                    continue;

                // We maintain a list of potentially isolated cells. Isolated cell is a such cell than can only be reached diagonally
                var alreadyVisited = movementCost.TryGetValue(neighbourPosition, out var oldCost);
                if (!GetDirection(currentPosition, neighbourPosition, false, true).IsCardinal())
                    if (!alreadyVisited)
                        isolatedCells.Add(neighbourPosition);
                    else
                        isolatedCells.Remove(neighbourPosition);

                // If new cost is more than old cost
                if (alreadyVisited && newCost >= oldCost)
                    continue;

                movementCost[neighbourPosition] = newCost;

                // Add cell to queue
                if (!queue.Contains(neighbourPosition))
                    queue.Enqueue(neighbourPosition);
            }
        }

        // Access enumerator manually for performance reasons
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

        // Exclude isolated cells
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

    /// <summary>
    /// Finds path for unit between 2 points using A*
    /// </summary>
    /// <param name="unit"></param>
    /// <param name="start"></param>
    /// <param name="goal"></param>
    /// <param name="maxCost">Optional max pathfinding depth</param>
    /// <returns></returns>
    public static GridPath FindPath(Unit unit, Vector2Int start, Vector2Int goal, int maxCost = int.MaxValue)
    {
        var unitType = unit.UnitType;

        var capacity = ((Mathf.Abs(goal.x - start.x) + 1) * (Mathf.Abs(goal.y - start.y) + 1)) >> 1;
        var frontier = new PriorityQueue<Vector2Int>(capacity);
        var runningCost = new Dictionary<Vector2Int, float>(capacity);
        var cameFrom = new Dictionary<Vector2Int, Vector2Int>(capacity);

        if (!WorldGrid.Instance.PointInGrid(goal) || !WorldGrid.Instance[goal].IsPassable(unit))
            return null;

        frontier.Enqueue(start, 0f);
        runningCost[start] = 0f;

        // We use classic A*
        while (frontier.Length > 0)
        {
            var currentPosition = frontier.Dequeue();


            var currentCost = runningCost[currentPosition];
            var offsets = GetNeighboursOffsets(unit.SortingLayerId, currentPosition);
            // Reached Goal
            if (currentPosition == goal)
            {
                var path = RestorePath(start, currentPosition, cameFrom);
                var travelCost = runningCost[currentPosition];
                var gridPath = new GridPath(path, travelCost);
                return gridPath;
            }

            for (var i = 0; i < offsets.Length; i++)
            {
                var offset = offsets[i];
                var neighbourPosition = currentPosition + offset;

                // Check if not out of bounds
                if (!WorldGrid.Instance.PointInGrid(neighbourPosition))
                    continue;


                // Check if cell free
                var otherUnit = WorldGrid.Instance[neighbourPosition].Unit;
                if (otherUnit != null && otherUnit.IsEnemy(unit))
                    continue;

                // Check if can move
                if (!WorldGrid.Instance[currentPosition].CanMove(neighbourPosition, unit))
                    continue;

                var neighbourCost = WorldGrid.Instance[currentPosition].GetTravelCost(neighbourPosition, unit);
                var newCost = currentCost + neighbourCost;
                if (newCost > maxCost || runningCost.TryGetValue(neighbourPosition, out var oldCost) && newCost > oldCost)
                    continue;

                runningCost[neighbourPosition] = newCost;
                cameFrom[neighbourPosition] = currentPosition;

                var heuristic = ManhattanHeuristicWithInverseTieBreaks(neighbourPosition, goal);
                var priority = newCost + heuristic;

                if (frontier.Contains(neighbourPosition))
                    frontier[neighbourPosition] = priority;
                else
                    frontier.Enqueue(neighbourPosition, priority);
            }
        }

        return null;
    }

    /// <summary>
    /// Finds path for unit between 2 points using A*
    /// </summary>
    /// <param name="start"></param>
    /// <param name="goal"></param>
    /// <param name="maxCost">Optional max pathfinding depth</param>
    /// <returns></returns>
    public static GridPath FindPath(int sortingLayerID, Vector2Int start, Vector2Int goal, int maxCost = int.MaxValue)
    {
        var capacity = ((Mathf.Abs(goal.x - start.x) + 1) * (Mathf.Abs(goal.y - start.y) + 1)) >> 1;
        var frontier = new PriorityQueue<Vector2Int>(capacity);
        var runningCost = new Dictionary<Vector2Int, float>(capacity);
        var cameFrom = new Dictionary<Vector2Int, Vector2Int>(capacity);

        if (!WorldGrid.Instance.PointInGrid(goal))
            return null;

        frontier.Enqueue(start, 0f);
        runningCost[start] = 0f;

        // We use classic A*
        while (frontier.Length > 0)
        {
            var currentPosition = frontier.Dequeue();


            var currentCost = runningCost[currentPosition];
            var offsets = GetNeighboursOffsets(sortingLayerID, currentPosition);
            // Reached Goal
            if (currentPosition == goal)
            {
                var path = RestorePath(start, currentPosition, cameFrom);
                var travelCost = runningCost[currentPosition];
                var gridPath = new GridPath(path, travelCost);
                return gridPath;
            }

            for (var i = 0; i < offsets.Length; i++)
            {
                var offset = offsets[i];
                var neighbourPosition = currentPosition + offset;

                // Check if not out of bounds
                if (!WorldGrid.Instance.PointInGrid(neighbourPosition))
                    continue;


                // Check if cell free
                var otherUnit = WorldGrid.Instance[neighbourPosition].Unit;
                if (otherUnit != null)
                    continue;

                var neighbourCost = 1;
                var newCost = currentCost + neighbourCost;
                if (newCost > maxCost || runningCost.TryGetValue(neighbourPosition, out var oldCost) && newCost > oldCost)
                    continue;

                runningCost[neighbourPosition] = newCost;
                cameFrom[neighbourPosition] = currentPosition;

                var heuristic = ManhattanHeuristicWithInverseTieBreaks(neighbourPosition, goal);
                var priority = newCost + heuristic;

                if (frontier.Contains(neighbourPosition))
                    frontier[neighbourPosition] = priority;
                else
                    frontier.Enqueue(neighbourPosition, priority);
            }
        }

        return null;
    }

    /// <summary>
    /// Finds path for unit from its current position to goal
    /// </summary>
    /// <param name="unit"></param>
    /// <param name="goal"></param>
    /// <param name="maxCost">Optional max pathfinding depth</param>
    /// <returns></returns>
    public static GridPath FindPath(Unit unit, Vector2Int goal, int maxCost = int.MaxValue)
    {
        return FindPath(unit, unit.GridPosition, goal, maxCost);
    }

    /// <summary>
    /// Checks if there's a line of sight between two points
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="validator">Function used to determine whether specific cell blocks LOS from start position</param>
    /// <param name="gridLine">Equivalent of line between start and end points on grid </param>
    /// <returns></returns>
    public static bool HasLineOfSight(int sortingLayerId, Vector2Int start, Vector2Int end, Func<int, Vector2Int, Vector2Int, bool> validator, out GridLine gridLine)
    {
        // We use modification of the Bresenham's line drawing algorithm
        gridLine = new GridLine(start, end);

        // Nearest cells always visible
        if ((end - start).sqrMagnitude == 1)
            return true;

        // Return false if we have no LOS for end position
        if (!validator(sortingLayerId, start, end))
            return false;

        // Last point with LOS
        Vector2Int? prevPoint = null;

        // We check every point on gridLine
        foreach (var point in gridLine)
        {
            // Check distance from last point with LOS to current point
            // If it's greater than 1, then LOS is blocked
            if (prevPoint != null && GetBoxDistance(prevPoint.Value, point) > 1)
                return false;

            // Skip point with out LOS
            if (!validator(sortingLayerId, start, point))
                continue;

            // Do additional checks for diagonals
            if (prevPoint != null && prevPoint.Value.x != point.x && prevPoint.Value.y != point.y && // If direction from previous is diagonal
                !validator(sortingLayerId, start, new Vector2Int(point.x, prevPoint.Value.y)) &&
                !validator(sortingLayerId, start, new Vector2Int(prevPoint.Value.x, point.y))) // If diagonal movement is blocked
            {
                return false;
            }

            // We have LOS of point, so set prevPoint to be equal to point
            prevPoint = point;
        }

        return true;
    }

    /// <summary>
    /// Checks if there's a line of sight between two points. Takes tiles LOS and Height properties into account
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public static bool HasLineOfSight(int sortingLayerId, Vector2Int start, Vector2Int end)
    {
        return HasLineOfSight(sortingLayerId, start, end, CellLOSValidatorWithHeight, out _);
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

    public static bool DefaultCellLOSValidator(int sortingLayerId, Vector2Int start, Vector2Int cell)
    {
        return WorldGrid.Instance[cell].HasLineOfSight(sortingLayerId);
    }

    public static bool CellLOSValidatorWithHeight(int sortingLayerId, Vector2Int start, Vector2Int cell)
    {
        var diff = WorldGrid.Instance[start].Height - WorldGrid.Instance[cell].Height;
        if (diff > 2)
            return true;

        if (diff < -2)
            return false;

        return WorldGrid.Instance[cell].HasLineOfSight(sortingLayerId);
    }

    // Internal method used to restore A* path
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