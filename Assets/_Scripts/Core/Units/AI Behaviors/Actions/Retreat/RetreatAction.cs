using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TenPN.DecisionFlex;

public class RetreatAction : BaseAction
{
    private Dictionary<string, AIBehavior> _retreatStrategies = new Dictionary<string, AIBehavior>();

    void Start()
    {
        foreach (AIBehavior behavior in BehaviorList.RetreatBehaviors)
            _retreatStrategies.Add(behavior.GetType().ToString(), behavior);
    }

    public override void Perform(IContext context)
    {
        var canSeeAllies = AIAgent.AlliesWithinSight().Count > 0;
        if (canSeeAllies)
            _retreatStrategies["BackToFormation"].Execute();
        // _retreatStrategies["RetreatToFarthestAlly"].Execute();
    }
}
