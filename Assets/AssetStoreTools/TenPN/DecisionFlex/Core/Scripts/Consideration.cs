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
       A single influence on the scoring of one action.

       \details
       Is called once for every IContext passed by a ContextFactory to a DecisionFlex script. 

       You shouldn't need to routinely derive from this. A number of helper behaviours exist to take care of many situations. See the \ref contact_us page for how to contact us if you think there is an obvious helper missing.
    */
    public abstract class Consideration : MonoBehaviour
    {
        public float Consider(IContext context)
        {
            float rawConsideration = MakeConsideration(context);
            return rawConsideration;
        }

        /** 
            Score this IContext. 
            Called once per IContext produced by the ContextFactory during a decision.
            \return Score should not be less than 0, and is normally less than 1. In some cases, returning more than 1 might be desired to drown out other activities.
        */
        protected abstract float MakeConsideration(IContext context);

        //////////////////////////////////////////////////

#if UNITY_EDITOR
        /** 
            a field for documenting what this consideration does in more detail.
            it is stripped out of the final build.
        */
        [MultilineAttribute]
        [SerializeField] string m_comment;
#endif

        //////////////////////////////////////////////////
    }
}
