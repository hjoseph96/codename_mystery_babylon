using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HealAlly : HealBehavior
{
    [ReadOnly] public new readonly AIActionType ActionType = AIActionType.Heal;



    public override void Execute() => executionState = AIBehaviorState.Executing;

    // Update is called once per frame
    void Update()
    {


        if (executionState == AIBehaviorState.Executing)
        {

            var allyToHeal = AIAgent.ReachableAllies<AIUnit>().Select(ally => ally).Where(ally => ally)
             .OrderByDescending(ally => ally.NeedToHeal()).FirstOrDefault();

            var movePath = AIAgent.MovePath(AIAgent.FindClosestCellTo(allyToHeal.GridPosition));

            AIAgent.OnFinishedMoving = null;

            AIAgent.OnFinishedMoving += delegate ()
            {
                executionState = AIBehaviorState.Complete;
                Heal(allyToHeal);
            };

            if (!AIAgent.MovedThisTurn)
            {
                AIAgent.SetMovedThisTurn();
                StartCoroutine(AIAgent.MovementCoroutine(movePath));
            }
            



        }
    }

    private void Heal(Unit ally)
    {
        var healingItems = AIAgent.HealingItems();

        if (healingItems.Count > 0)
            AIAgent.Trade(ally, healingItems[0], null);

        AIAgent.TookAction();
    }

}
