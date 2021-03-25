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
    [CustomEditor (typeof(Consideration), true)]
    public class ConsiderationInspector 
        : ContextInspector<DecisionFlex.ContextScore>
    {
        protected override IEnumerable<DecisionFlex.ContextScore> GetContainersFor(
            MonoBehaviour targetBehaviour)
        {
            return Flex.GetContextScoreForConsideration(targetBehaviour as Consideration);
        }

        protected override float GetScoreFromContainer(DecisionFlex.ContextScore container)
        {
            return container.Score;
        }

        protected override IContext GetContextFromContainer(
            DecisionFlex.ContextScore container)
        {
            return container.Context;
        }
    }
}
