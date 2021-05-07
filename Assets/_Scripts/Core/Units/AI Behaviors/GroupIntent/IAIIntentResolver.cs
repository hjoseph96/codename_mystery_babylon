using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum AIGroupTraits
{
    Offensive,
    Defensive
}

public enum AIGroupIntention
{
    AgainstPlayer,
    AgainstAll,
    AgainstOtherEnemies
}

public abstract class IAIIntentResolver
{
    public virtual RelativePosition Resolve(AIGroup group)
    {
        switch (group.GroupTrait)
        {
            case AIGroupTraits.Offensive:
                return ResolveOffensive(group, GetTargetUnitsByIntention(group));
            case AIGroupTraits.Defensive:
                return ResolveDefensive(group, GetTargetUnitsByIntention(group));
            default:
                return group.PreferredGroupPosition;
        }
    }

    protected abstract RelativePosition ResolveOffensive(AIGroup group, List<Unit> MainEnemies);

    protected abstract RelativePosition ResolveDefensive(AIGroup group, List<Unit> MainEnemies);

    protected virtual List<Unit> GetTargetUnitsByIntention(AIGroup group)
    {
        switch (group.GroupIntention)
        {
            case AIGroupIntention.AgainstPlayer:
                return group.Members[0].Enemies().Select(e => e).Where(e => e.IsLocalPlayerUnit).ToList();

            case AIGroupIntention.AgainstOtherEnemies:
                return group.Members[0].Enemies().Select(e => e).Where(e => !e.IsLocalPlayerUnit).ToList();
            default:
            case AIGroupIntention.AgainstAll:
                return group.Members[0].Enemies();
        }
    }


    protected Vector2Int ApplyMovementMode(AIGroup group, Vector2Int position)
    {
        Vector2Int _preferredPositionInTIghtMode;
        switch (group.MovementMode)
        {
            case AIGroupMovementMode.TightFormation:
                _preferredPositionInTIghtMode = group.Members.Select(m => m.FindClosestCellTo(position))
                    .OrderByDescending(c => GridUtility.GetBoxDistance(c, position)).First();

                foreach (var item in group.Members)
                {
                    var myCell = _preferredPositionInTIghtMode + item.MyCellInFormation();
                    var closestCell = item.FindClosestCellTo(myCell);
                    closestCell += closestCell - myCell;
                    Vector2 normalizedDir = (closestCell - myCell);
                    normalizedDir.Normalize();

                    _preferredPositionInTIghtMode += new Vector2Int(Mathf.Abs(_preferredPositionInTIghtMode.x - closestCell.x) * Mathf.CeilToInt(normalizedDir.x)
                        , Mathf.Abs(_preferredPositionInTIghtMode.y - closestCell.y) * Mathf.CeilToInt(normalizedDir.y));
                }

                return _preferredPositionInTIghtMode;
            case AIGroupMovementMode.LooseFormation:
            default:
                return position;
        }
    }
}
