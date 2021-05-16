using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TenPN.DecisionFlex;

public class HealAction : BaseAction
{
    private Dictionary<string, AIBehavior> _healingStrategies = new Dictionary<string, AIBehavior>();

    void Start()
    {
        foreach (AIBehavior behavior in BehaviorList.HealBehaviors)
            _healingStrategies.Add(behavior.GetType().ToString(), behavior);
    }

    public override void Perform(IContext context)
    {
        Debug.Log("I AM CHOOSING TO HEAL");
        _healingStrategies["HealSelf"].Execute();
    }
}
