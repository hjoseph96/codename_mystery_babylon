using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TenPN.DecisionFlex;

public class AttackAction : BaseAction
{
    private Dictionary<string, AIBehavior> _attackStrategies = new Dictionary<string, AIBehavior>();

    void Start()
    {
        foreach (AIBehavior behavior in BehaviorList.AttackBehaviors)
            _attackStrategies.Add(behavior.GetType().ToString(), behavior);
    }

    public override void Perform(IContext context)
    {
        //if (AIAgent.CanAttackLeader())
        //{
        //    Debug.Log("IMPLEMENT AttackLeader AIBehavior...");
        //    return;
        //}

        _attackStrategies["AttackWeakestUnit"].Execute();
    }
}
