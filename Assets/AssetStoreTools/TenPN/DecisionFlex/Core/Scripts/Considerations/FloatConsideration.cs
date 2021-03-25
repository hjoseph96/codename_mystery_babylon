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
       Evalutes a float value from the IContext through a response curve
    */
    [AddComponentMenu("TenPN/DecisionFlex/Considerations/Float Consideration")]
    public class FloatConsideration : ContextValueConsideration
    {
        protected override float GetValue(IContext context)
        {
            return context.GetContext<float>(ContextName);
        }
    }
}
