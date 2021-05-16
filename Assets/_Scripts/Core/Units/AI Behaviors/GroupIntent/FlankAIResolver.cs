using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlankAIResolver : IAIIntentResolver
{
    protected override RelativePosition ResolveDefensive(AIGroup group, List<Unit> MainEnemies)
    {
        group.PreferredGroupPosition.Position = ApplyMovementMode(group, group.CollaboratorGroup.GetFlankPoint(group));

        return group.PreferredGroupPosition;
    }

    protected override RelativePosition ResolveOffensive(AIGroup group, List<Unit> MainEnemies)
    {
        group.PreferredGroupPosition.Position = ApplyMovementMode(group, group.CollaboratorGroup.GetFlankPoint(group));

        return group.PreferredGroupPosition;
    }
}
