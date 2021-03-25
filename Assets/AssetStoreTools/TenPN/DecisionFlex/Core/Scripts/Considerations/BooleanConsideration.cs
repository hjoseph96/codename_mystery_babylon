// ******************************************************************************************
//
// 							DecisionFlex, (c) Andrew Fray 2015
//
// ******************************************************************************************
using UnityEngine;

namespace TenPN.DecisionFlex
{
    /**
       \brief
       Evaulate a context value in relation to a bool, and return one of two scores depending on the result

       \details
       For example, fetch the isAlive string from the IContext, and if it is true return a score of one (saying this action can go ahead), otherwise return a score of zero (saying this action can't possibly go ahead).
    */
    [AddComponentMenu("TenPN/DecisionFlex/Considerations/Bool Consideration")]
    public class BooleanConsideration : Consideration
    {
        protected override float MakeConsideration(IContext context)
        {
            if (context.HasContext<bool>(m_contextName))
            {
                bool isFlag = context.GetContext<bool>(m_contextName);
                return isFlag ? m_scoreIfTrue : m_scoreIfFalse;
            }
            else
            {
                throw new UnityException("cannot find bool context of name " + m_contextName);
            }
        }


        //////////////////////////////////////////////////

        [SerializeField] private string m_contextName;
        [RangeAttribute(0.0f, 1.0f)]
        [SerializeField] private float m_scoreIfTrue = 1.0f;
        [RangeAttribute(0.0f, 1.0f)]
        [SerializeField] private float m_scoreIfFalse = 0.0f;

        //////////////////////////////////////////////////
    }
}
