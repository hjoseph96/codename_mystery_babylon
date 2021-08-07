using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Sirenix.OdinInspector;

public class RetreatToFarthestAlly : RetreatBehavior
{
    [ReadOnly] public new readonly AIActionType ActionType = AIActionType.Retreat;


    protected bool _setRetreatTarget = false;
    private Vector2Int _retreatDestination;

    public override void Execute() => executionState = AIBehaviorState.Executing;

    // Update is called once per frame
    void Update()
    {
        if (executionState == AIBehaviorState.Executing)
        {
            if (!_setRetreatTarget)
                _retreatDestination = RetreatTarget();

            var movePath = AIAgent.MovePath(_retreatDestination);

            AIAgent.OnFinishedMoving = null;

            AIAgent.OnFinishedMoving += delegate ()
            {
                executionState = AIBehaviorState.Complete;

                // Post Move logic: check if NeedToHeal is high, if so use an Healing Item if you have one
                if (AIAgent.NeedToHeal() > .45)
                {
                    var healingItems = AIAgent.HealingItems();

                    if (healingItems.Count > 0)
                        StartCoroutine(UseHealingItem(healingItems[0]));
                    else
                        CompleteAction();
                }
                else
                    CompleteAction();
            };

            if (!AIAgent.MovedThisTurn)
            {
                AIAgent.SetMovedThisTurn();
                StartCoroutine(AIAgent.MovementCoroutine(movePath));
            }
        }
    }


    private IEnumerator UseHealingItem(Consumable item)
    {
        item.UseItem();

        yield return new WaitForSeconds(3f);

        CompleteAction();
    }


    private void CompleteAction()
    {
        AIAgent.TookAction();
        _setRetreatTarget = false;
    }



    protected virtual Vector2Int RetreatTarget()
    {
        _setRetreatTarget = true; // prevent multiple calls per action

        // Find The Closest Enemy
        var farthestEnemy = AIAgent.EnemiesWithinSight()
                                  .OrderByDescending((enemy) => GridUtility.GetBoxDistance(AIAgent.GridPosition, enemy.GridPosition)).First();

        // Find the farthest cell distance in the grid within the AI's vision range
        var maxDistance = AIAgent.VisionRange()
                                 .Max((gridPosition) => GridUtility.GetBoxDistance(gridPosition, farthestEnemy.GridPosition));

        // Any cell in the vision range that is >= (maxDistance - 3) is far enough away to be considered
        int maxDistanceBuffer = 3;
        var potentialTargets = AIAgent.VisionRange().Where((gridPosition) => GridUtility.GetBoxDistance(gridPosition, farthestEnemy.GridPosition) >= (maxDistance - maxDistanceBuffer));

        if (AIAgent.AlliesWithinSight().Count == 0)
            return potentialTargets.ToList()[0];

        // Look for the farthest away Ally within sight
        var farthestAlly = AIAgent.AlliesWithinSight()
                                  .OrderByDescending((ally) => GridUtility.GetBoxDistance(AIAgent.GridPosition, ally.GridPosition)).First();

        // Get the closest cell distance of the potential targets to the farthest ally
        var closestToAllyDistance = potentialTargets.Min((gridPosition) => GridUtility.GetBoxDistance(gridPosition, farthestAlly.GridPosition));

        // The place to retreat to is far away from the nearest enemy, yet close to farthest ally
        var farFromEnemyButCloseToAlly = potentialTargets.First((gridPosition) => GridUtility.GetBoxDistance(gridPosition, farthestAlly.GridPosition) == closestToAllyDistance);

        return AIAgent.FindClosestCellTo(farFromEnemyButCloseToAlly);
    }
}
