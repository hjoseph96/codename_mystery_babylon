// ******************************************************************************************
//
// 							DecisionFlex, (c) Andrew Fray 2014
//
// ******************************************************************************************
using UnityEngine;

namespace TenPN.DecisionFlex
{
    /**
       \brief
       Evalutes an int value from the IContext through a response curve
    */
    [AddComponentMenu("TenPN/DecisionFlex/Considerations/Int Consideration")]
    public class IntConsideration : ContextValueConsideration
    {
        protected override float GetValue(IContext context)
        {
            return (float)context.GetContext<int>(ContextName);
        }
    }
}
