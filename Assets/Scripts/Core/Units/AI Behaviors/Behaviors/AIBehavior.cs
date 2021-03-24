using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Sirenix.OdinInspector;

public enum AIBehaviorState
{
    NotStarted,
    Executing,
    Failed,
    Complete
}

public enum AIActionType
{
    Attack,
    Defend,
    Retreat,
    Heal
}

public class AIBehavior : MonoBehaviour
{
    [ReadOnly] public readonly AIActionType ActionType = AIActionType.Attack;

    protected AIBehaviorState executionState = AIBehaviorState.NotStarted;
    [ShowInInspector] public AIBehaviorState ExecutionState { get => executionState; }
    protected AIUnit executingAgent;

    public virtual void Execute() => throw new System.Exception("You didn't implement Execute() for this AIBehavior!");

    public void SetTargetAgent(AIUnit agent) => executingAgent = agent;

    protected Vector2Int FindClosestCellToTarget(Vector2Int goal)
    {
        var moveRange = GridUtility.GetReachableCells(executingAgent);

        if (moveRange.Contains(goal))
            return goal;

        Vector2Int targetCell;

        // Get cells next to the target that are passable and unoccupied
        var targetNeighbors = GridUtility.GetNeighbours(goal);
        targetNeighbors.Select((position) => WorldGrid.Instance[position].IsPassable(executingAgent.UnitType));

        // Find the nearest neighbor
        var shortestDistance = targetNeighbors.Min((position) => GridUtility.GetBoxDistance(executingAgent.GridPosition, position));
        targetCell = targetNeighbors.First((position) => GridUtility.GetBoxDistance(executingAgent.GridPosition, position) == shortestDistance);

        // Take the nearest neighbor and go to the closest REACHABLE cell
        shortestDistance = moveRange.Min((position) => GridUtility.GetBoxDistance(position, targetCell));
        targetCell = moveRange.First((position) => GridUtility.GetBoxDistance(position, targetCell) == shortestDistance);

        return targetCell;
    }

    public GridPath MovePath(Vector2Int destination)
    {
        return GridUtility.FindPath(executingAgent, executingAgent.GridPosition, destination, executingAgent.CurrentMovementPoints);
    }
}
