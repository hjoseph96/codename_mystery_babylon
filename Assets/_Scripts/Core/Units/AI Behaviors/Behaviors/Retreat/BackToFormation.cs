using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BackToFormation : RetreatToFarthestAlly
{
    protected override Vector2Int RetreatTarget()
    {
        _setRetreatTarget = true; // prevent multiple calls per action

        var farthestEnemy = AIAgent.EnemiesWithinSight()
                          .OrderByDescending((enemy) => GridUtility.GetBoxDistance(AIAgent.GridPosition, enemy.GridPosition)).First();
        // Find the farthest cell distance in the grid within the AI's vision range
        var safestPosition = AIAgent.group.FormationPositions()
                                 .First((gridPosition) => GridUtility.GetBoxDistance(gridPosition, farthestEnemy.GridPosition) >= 3);

        return AIAgent.FindClosestCellTo(safestPosition);
    }
}
