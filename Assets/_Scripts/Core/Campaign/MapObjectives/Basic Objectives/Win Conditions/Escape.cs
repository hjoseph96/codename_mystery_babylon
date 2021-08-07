using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class Escape : MapObjective
{
    protected override void Start()
    {
        base.Start();

        objectiveType = ObjectiveType.Win;
    }

    public override bool CheckConditions()
    {
        var playerUnits = campaignManager.PlayerUnits();

        return playerUnits.All((player) => player.HasEscaped);
    }
}
