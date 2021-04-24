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
                return Vanguard_Destination(group);
            case AIGroupRole.Flank:
                return Flank_Destination(group);
            default:
                return group.PreferredGroupPosition;
        }
    }

    private static RelativePosition Vanguard_Destination(AIGroup group)
    {
        var enemies = group.Members[0].Enemies();
        var paths = enemies.Select(enemy => new RelativePosition(enemy, CampaignManager.Instance.PlayerDestination));
        RelativePosition closestPlayer = paths
            .OrderBy(path => path.Path.Length).First();

        group.PreferredGroupPosition.Position = ApplyMovementMode(group, closestPlayer.Path.GetPointInPath(.05f));
        group.PreferredGroupPosition.Target = closestPlayer.Position;
        return group.PreferredGroupPosition;
    }

    private static RelativePosition Flank_Destination(AIGroup group)
    {
        group.PreferredGroupPosition.Position = ApplyMovementMode(group, group.CollaboratorGroup.GetFlankPoint(group));
        return group.PreferredGroupPosition;
    }

    private static Vector2Int ApplyMovementMode(AIGroup group, Vector2Int position)
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
