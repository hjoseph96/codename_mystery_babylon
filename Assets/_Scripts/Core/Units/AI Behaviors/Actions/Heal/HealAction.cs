using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TenPN.DecisionFlex;

public class HealAction : BaseAction
{
    private Dictionary<string, AIBehavior> _healingStrategies = new Dictionary<string, AIBehavior>();

    protected override void Awake()
    {
        base.Awake();

        foreach (AIBehavior behavior in BehaviorList.HealBehaviors)
            _healingStrategies.Add(behavior.GetType().ToString(), behavior);
    }

    public override void Perform(IContext context)
    {
        Debug.Log($"{AIAgent.gameObject.name} Decided to Heal");

        if (!_healingStrategies.ContainsKey("HealSelf"))
            Debug.Log("Cannot heal!");

        var behaviorToExecute = _healingStrategies["HealSelf"];

        AIAgent.SetCurrentBehavior(behaviorToExecute);

        behaviorToExecute.Execute();
    }
}
