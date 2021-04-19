using System.Collections;
using System.Collections.Generic;
using TenPN.DecisionFlex;
using UnityEngine;

public class GroupAction : BaseAction
{
    private Dictionary<string, AIBehavior> _groupStrategies = new Dictionary<string, AIBehavior>();

    void Start()
    {
        foreach (AIBehavior behavior in BehaviorList.GroupBehaviors)
            _groupStrategies.Add(behavior.GetType().ToString(), behavior);
    }

    public override void Perform(IContext context)
    {
        Debug.Log(AIAgent.Name +  " Going With Group");
        _groupStrategies["HoldFormation"].Execute();
    }
}
