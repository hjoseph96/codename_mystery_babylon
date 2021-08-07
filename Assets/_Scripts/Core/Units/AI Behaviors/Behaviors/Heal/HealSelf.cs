using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Sirenix.OdinInspector;

public class HealSelf : HealBehavior
{
    [ReadOnly] public new readonly AIActionType ActionType = AIActionType.Heal;

    private bool _usedItem = false;

    public override void Execute() => executionState = AIBehaviorState.Executing;

    // Update is called once per frame
    void Update()
    {
        if (executionState == AIBehaviorState.Executing)
        {
            _usedItem = true;

            var healingItems = AIAgent.HealingItems();

            if (healingItems.Count > 0 && !_usedItem)
            {
                AIAgent.UponHealComplete += delegate ()
                {
                    AIAgent.TookAction();
                    AIAgent.UponHealComplete = null;
                };
     
                healingItems[0].UseItem();
            }
            else
                AIAgent.TookAction();

            executionState = AIBehaviorState.Complete;
        }
    }
}
