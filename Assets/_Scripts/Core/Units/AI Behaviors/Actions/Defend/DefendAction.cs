using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TenPN.DecisionFlex;

public class DefendAction : BaseAction
{
    private Dictionary<string, AIBehavior> _defensiveStrategies = new Dictionary<string, AIBehavior>();

    void Start()
    {
        foreach (AIBehavior behavior in BehaviorList.DefendBehaviors)
            _defensiveStrategies.Add(behavior.GetType().ToString(), behavior);
    }

    public override void Perform(IContext context)
    {
        Debug.Log("I AM CHOOSING TO DEFND");
        AIAgent.TookAction();
    }
}
