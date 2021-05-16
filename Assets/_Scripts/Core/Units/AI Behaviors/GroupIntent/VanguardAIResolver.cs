using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VanguardAIResolver : IAIIntentResolver
{
    protected override RelativePosition ResolveDefensive(AIGroup group, List<Unit> MainEnemies)
    {
        var enemies = MainEnemies;
        var paths = enemies.Select(enemy => new RelativePosition(enemy, enemy.PreferredDestination));
        RelativePosition closestEnemy = paths
            .OrderBy(path => path.Path.Length).First();

        group.PreferredGroupPosition.Position = ApplyMovementMode(group, closestEnemy.GetPointInPathOrDefault(.5f));
        group.PreferredGroupPosition.Target = closestEnemy.Position;
        return group.PreferredGroupPosition;
    }

    protected override RelativePosition ResolveOffensive(AIGroup group, List<Unit> MainEnemies)
    {
        var enemies = MainEnemies;
        var paths = enemies.Select(enemy => new RelativePosition(enemy, enemy.PreferredDestination));
        RelativePosition closestEnemy = paths
            .OrderBy(path => path.Path.Length).First();

        group.PreferredGroupPosition.Position = ApplyMovementMode(group, closestEnemy.GetPointInPathOrDefault(.05f));
        group.PreferredGroupPosition.Target = closestEnemy.Position;
        return group.PreferredGroupPosition;
    }
}
