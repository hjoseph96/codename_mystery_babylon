using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealSelf : HealBehavior
{
    [ReadOnly] public new readonly AIActionType ActionType = AIActionType.Heal;



    public override void Execute() => executionState = AIBehaviorState.Executing;

    // Update is called once per frame
    void Update()
    {
        if (executionState == AIBehaviorState.Executing)
        {
            var healingItems = AIAgent.HealingItems();

            if (healingItems.Count > 0)
                StartCoroutine(UseHealingItem(healingItems[0]));
            else
                AIAgent.TookAction();
        }
    }


    private IEnumerator UseHealingItem(Consumable item)
    {
        item.UseItem();

        yield return new WaitForSecondsRealtime(7f);

        AIAgent.TookAction();
    }
}
