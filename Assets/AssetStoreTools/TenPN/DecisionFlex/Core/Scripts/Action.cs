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
       A helper class, combining MonoBehaviour and IUtilityAction for new actions.

       \details
       If you are integrating DecisionFlex with existing code, you might find it easier to use IUtilityAction directly.
    */
    public abstract class Action : MonoBehaviour, IAction
    {
        public abstract void Perform(IContext context);
    }
}
