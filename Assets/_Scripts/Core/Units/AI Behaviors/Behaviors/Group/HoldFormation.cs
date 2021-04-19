using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldFormation : GroupBehavior
{
    public override void Execute() => executionState = AIBehaviorState.Executing;
   
    // Update is called once per frame
    void Update()
    {
        if (executionState == AIBehaviorState.Executing)
        {


            
            var destination = AIAgent.FindClosestCellTo(AIAgent.group.PreferredGroupPosition.Position + AIAgent.MyCellInFormation());
            var movePath = AIAgent.MovePath(destination);

            AIAgent.OnFinishedMoving = null;

            AIAgent.OnFinishedMoving += delegate ()
            {
                executionState = AIBehaviorState.Complete;
                AIAgent.TookAction();
            };

            if (!AIAgent.MovedThisTurn)
            {
                AIAgent.SetMovedThisTurn();
                StartCoroutine(AIAgent.MovementCoroutine(movePath));
            }
        }
    }

    
}
