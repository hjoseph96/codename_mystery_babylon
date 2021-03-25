using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RetreatToFarthestAlly : AIBehavior
{
    private bool _setRetreatTarget = false;
    Vector2Int retreatDestination;

    public override void Execute() => executionState = AIBehaviorState.Executing;

    // Update is called once per frame
    void Update()
    {
        if (executionState == AIBehaviorState.Executing)
        {
            if (!_setRetreatTarget)
                retreatDestination = RetreatTarget();

            var movePath = MovePath(retreatDestination);

            AIAgent.OnFinishedMoving = null;

            // Post Move logic: check if NeedToHeal is high, if so use an Healing Item if you have one
            AIAgent.OnFinishedMoving += delegate ()
            {
                // TODO: Add UnitInventory checks for healing items, if has one, use it
                executionState = AIBehaviorState.Complete;
                AIAgent.TookAction();
                _setRetreatTarget = false;
            };

            if (!AIAgent.MovedThisTurn)
            {
                AIAgent.SetMovedThisTurn();
                StartCoroutine(AIAgent.MovementCoroutine(movePath));
            }
        }
    }


    // Generally, you dont wanna use LINQ due to performance
    // But, this method should only need to be called ONCE, so...
    // meh...

    public Vector2Int RetreatTarget()
    {
        _setRetreatTarget = true; // prevent multiple calls per action

        // Find The Closest Enemy
        var nearestEnemy = AIAgent.EnemiesWithinSight()
                                  .OrderBy((enemy) => GridUtility.GetBoxDistance(AIAgent.GridPosition, enemy.GridPosition)).First();

        // Find the farthest cell distance in the grid within the AI's vision range
        var maxDistance = AIAgent.VisionRange()
                                 .Max((gridPosition) => GridUtility.GetBoxDistance(gridPosition, nearestEnemy.GridPosition));

        // Any cell in the vision range that is >= (maxDistance - 3) is far enough away to be considered
        int maxDistanceBuffer = 3;
        var potentialTargets = AIAgent.VisionRange().Where((gridPosition) => GridUtility.GetBoxDistance(gridPosition, nearestEnemy.GridPosition) >= (maxDistance - maxDistanceBuffer));

        // Look for the farthest away Ally within sight
        var farthestAlly = AIAgent.AlliesWithinSight()
                                  .OrderByDescending((ally) => GridUtility.GetBoxDistance(AIAgent.GridPosition, ally.GridPosition)).First();

        // Get the closest cell distance of the potential targets to the farthest ally
        var closestToAllyDistance = potentialTargets.Min((gridPosition) => GridUtility.GetBoxDistance(gridPosition, farthestAlly.GridPosition));

        // The place to retreat to is far away from the nearest enemy, yet close to farthest ally
        var farFromEnemyButCloseToAlly = potentialTargets.First((gridPosition) => GridUtility.GetBoxDistance(gridPosition, farthestAlly.GridPosition) == closestToAllyDistance);

        return farFromEnemyButCloseToAlly;
    }
}
