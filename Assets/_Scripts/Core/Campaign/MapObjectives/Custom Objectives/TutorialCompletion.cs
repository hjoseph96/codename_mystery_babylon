using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialCompletion : MapObjective
{
    [SerializeField] private Unit _Christian;

    protected override void Start()
    {
        base.Start();

        objectiveType = ObjectiveType.Win;
    }

    public override bool CheckConditions()
    {
        if (campaignManager == null)
            campaignManager = CampaignManager.Instance;

        return campaignManager.EnemyUnits().Count == 0 && _Christian.CurrentHealth == 1;
    }
}
