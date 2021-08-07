using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Survive : MapObjective
{
    [SerializeField] private int _turnsToSurvive;

    protected override void Start()
    {
        base.Start();

        objectiveType = ObjectiveType.Win;
    }

    public override bool CheckConditions() => campaignManager.Turn > _turnsToSurvive;
}
