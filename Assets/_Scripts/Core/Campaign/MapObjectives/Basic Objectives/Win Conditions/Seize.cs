using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Sirenix.Serialization;

public class Seize : MapObjective
{
    [OdinSerialize] private Dictionary<int, List<Vector2Int>> _PositionsToSeize;
    public Dictionary<int, List<Vector2Int>> PositionsToSeize { get => _PositionsToSeize; }


    protected override void Start()
    {
        base.Start();

        objectiveType = ObjectiveType.Win;
    }

    /// <summary>
    /// Check if all groups of Seize Points have at least on WorldCell that has been seized by a PlayerUnit who is a Leader.
    /// </summary>
    /// <returns></returns>
    public override bool CheckConditions()
    {
        var passedObjective = PositionsToSeize.All(delegate (KeyValuePair<int, List<Vector2Int>> entry)
        {
            var anySeizedByPlayer = entry.Value.Any(delegate (Vector2Int seizePoint)
            {
                var cellToCheck = worldGrid[seizePoint];
                var seizedByPlayer = cellToCheck.SeizedBy.TeamId == Player.LocalPlayer.TeamId && cellToCheck.SeizedBy.IsLeader;

                return seizedByPlayer;
            });

            return anySeizedByPlayer;
        });

        return passedObjective;
    }
}
