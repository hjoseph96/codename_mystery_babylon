using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TenPN.DecisionFlex;

public class HealAllyAction : BaseAction
{
    private Dictionary<string, AIBehavior> _healingStrategies = new Dictionary<string, AIBehavior>();

    void Start()
    {
        foreach (AIBehavior behavior in BehaviorList.HealAlliesBehaviors)
            _healingStrategies.Add(behavior.GetType().ToString(), behavior);
    }

    public override void Perform(IContext context)
    {
        Debug.Log("I AM CHOOSING TO HEAL");
        _healingStrategies["HealAlly"].Execute();
    }
}
