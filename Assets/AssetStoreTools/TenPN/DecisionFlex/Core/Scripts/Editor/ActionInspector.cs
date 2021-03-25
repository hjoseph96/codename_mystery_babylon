// ******************************************************************************************
//
// 							DecisionFlex, (c) Andrew Fray 2014
//
// ******************************************************************************************
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace TenPN.DecisionFlex
{
    [CustomEditor (typeof(Action), true)]
    public class ActionInspector : ContextInspector<ActionSelection>
    {
        protected override IEnumerable<ActionSelection> GetContainersFor(
            MonoBehaviour targetBehaviour)
        {
            return Flex.GetSelectionsForAction(targetBehaviour.gameObject);
        }

        protected override float GetScoreFromContainer(ActionSelection container)
        {
            return container.Score;
        }

        protected override IContext GetContextFromContainer(ActionSelection container)
        {
            return container.Context;
        }

    }
}
