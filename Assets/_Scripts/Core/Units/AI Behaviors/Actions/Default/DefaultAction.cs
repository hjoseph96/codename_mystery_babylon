using System.Collections;
using System.Collections.Generic;
using TenPN.DecisionFlex;
using UnityEngine;

public class DefaultAction : BaseAction
{
    private Dictionary<string, AIBehavior> _defaultStrategies = new Dictionary<string, AIBehavior>();

    void Start()
    {
        foreach (AIBehavior behavior in BehaviorList.DefaultBehaviors)
            _defaultStrategies.Add(behavior.GetType().ToString(), behavior);
    }

    public override void Perform(IContext context)
    {
        Debug.Log(AIAgent.Name + " Doing Default Action");
        _defaultStrategies["HoldFormation"].Execute();
    }
}
