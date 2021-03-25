// ******************************************************************************************
//
// 							DecisionFlex, (c) Andrew Fray 2014
//
// ******************************************************************************************
using UnityEngine;
using System.Collections.Generic;

namespace TenPN.DecisionFlex
{
    /** 
        \brief
        Produces IContext objects, used to evaulate UtilityConsiderations when making decisions.

        \details
        Populate your IContext objects with the data and details that help your UtilityConsiderations produce scores.

        You can return more than one consideration, which is useful for pairing targets with decisions, eg targets and weapons. Each will be scored against all your actions seperately. See WeaponSelectionContextFactory for an example. 
        If you want to return just one context, you may prefer to use SingleContextFactory instead.
    */
    public abstract class ContextFactory : MonoBehaviour
    {
        public enum Logging
        {
            Enabled,
            Disabled,
        }

        /** 
            \returns a list of IContext objects, each of which will be scored against the decision's actions seperately.
            \param loggingSetting if Logging.Enabled, DecisionFlex#m_isLoggingEnabled is true. You should print or display some debugging info.
        */
        public abstract IList<IContext> AllContexts(Logging loggingSetting);
    }
}
