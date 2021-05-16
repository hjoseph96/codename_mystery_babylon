using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldFormation : GroupBehavior
{
    private GridPath _path;

    public override void Execute() => executionState = AIBehaviorState.Executing;
   
    // Update is called once per frame
    void Update()
    {
        if (executionState == AIBehaviorState.Executing)
        {

            var destination = AIAgent.FindClosestCellTo(AIAgent.group.PreferredGroupPosition.Position + AIAgent.MyCellInFormation());
            _path = AIAgent.MovePath(destination);

            if (_path.Length == 0)   // Already In Position
            {
                executionState = AIBehaviorState.Complete;
                AIAgent.TookAction();
            }

            AIAgent.OnFinishedMoving = null;

            AIAgent.OnFinishedMoving += delegate ()
            {
                executionState = AIBehaviorState.Complete;
                AIAgent.TookAction();
            };

            if (!AIAgent.MovedThisTurn)
            {
                AIAgent.SetMovedThisTurn();
                StartCoroutine(AIAgent.MovementCoroutine(_path));
            }
        }
    }

    protected virtual void OnDrawGizmos()
    {
        var worldGrid = WorldGrid.Instance;
        if (Application.isPlaying && worldGrid != null && _path != null)
        {
            foreach(var cell in _path.Path())
                Gizmos.DrawCube(worldGrid.Grid.GetCellCenterWorld((Vector3Int)cell), Vector3.one);

        }
    }

}
