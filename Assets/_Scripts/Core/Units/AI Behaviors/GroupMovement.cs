using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class GroupMovement
{
    public static RelativePosition UpdateDestination(AIGroup group)
    {
        switch (group.GroupRole)
        {
            case AIGroupRole.Vanguard:
                return new VanguardAIResolver().Resolve(group); //Vanguard_Destination(group);
            case AIGroupRole.Flank:
                return new FlankAIResolver().Resolve(group);
            default:
                return group.PreferredGroupPosition;
        }
    }
}
