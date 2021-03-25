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
       Base class for considerations that evaluate a context value through an animation curve
       \details
       Good for normalising a value into a score in a data-driven way.
       The majority of your considerations will probaly be derived from this class.
    */
    public abstract class ContextValueConsideration : Consideration
    {
        protected override float MakeConsideration(IContext context)
        {
            float floatValue = GetValue(context);
            float score = Consider(floatValue);
            return score;
        }

        protected abstract float GetValue(IContext context);

        protected string ContextName { get { return m_contextName; } }

        //////////////////////////////////////////////////

        [SerializeField] private string m_contextName;
        [SerializeField] private AnimationCurve m_responseCurve;

        //////////////////////////////////////////////////

        private float Consider(float floatValue)
        {
            float utility = Mathf.Clamp01(m_responseCurve.Evaluate(floatValue));
            return utility;
        }

    }
}
