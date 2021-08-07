using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class Rout : MapObjective
{
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        objectiveType = ObjectiveType.Win;
    }

    public override bool CheckConditions()
    {
        return campaignManager.EnemyUnits().Count == 0 && campaignManager.OtherEnemyUnits().Count == 0;
    }
}
