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
       Evaulate a context value in relation to a gate, and return one of two scores depending on the result

       \details
       For example, fetch the ammoCount string from the IContext, and if it is greater than zero return a score of one (saying this action can go ahead), otherwise return a score of zero (saying this action can't possibly go ahead).
    */
    [AddComponentMenu("TenPN/DecisionFlex/Considerations/Binary Context Consideration")]
    public class BinaryContextConsideration : Consideration
    {
        protected override float MakeConsideration(IContext context)
        {
            if (context.HasContext<float>(m_contextName))
            {
                return Evaluate(context.GetContext<float>(m_contextName));
            }
            else if (context.HasContext<int>(m_contextName))
            {
                return Evaluate((float)context.GetContext<int>(m_contextName));
            }
            else
            {
                throw new UnityException(
                    "cannot find float or int context of name " + m_contextName);
            }
        }


        //////////////////////////////////////////////////

        private enum BinaryEvaluation
        {
            TrueIfLessThan,
            TrueIfMoreThan,
        }

        [SerializeField] private string m_contextName;
        [SerializeField] private BinaryEvaluation m_evaluationType;
        [SerializeField] private float m_gateValue;
        [RangeAttribute(0.0f, 1.0f)]
        [SerializeField] private float m_ifTrue = 1.0f;
        [RangeAttribute(0.0f, 1.0f)]
        [SerializeField] private float m_ifFalse = 0.0f;

        //////////////////////////////////////////////////

        private float Evaluate(float value)
        {
            bool isTrue = (m_evaluationType == BinaryEvaluation.TrueIfLessThan && value < m_gateValue)
                || (m_evaluationType == BinaryEvaluation.TrueIfMoreThan && value > m_gateValue);
            return isTrue ? m_ifTrue : m_ifFalse;
        }
    }
}
