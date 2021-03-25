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
       Base class for chosing an action from all the scored choices
       
       \details
       \ref TenPN.DecisionFlex.DecisionFlex "DecisionFlex" uses this behaviour to choose which action is best, once all actions are scored. Two selectors ship with the package: \ref TenPN.DecisionFlex.SelectHighestScore "SelectHighestScore" and \ref TenPN.DecisionFlex.SelectWeightedRandom "SelectWeightedRandom".
    */
    public abstract class ActionSelector : MonoBehaviour
    {
        public enum Logging
        {
            IsEnabled,
            IsDisabled,
        }

        /**
           given a list of actions and scores, pick the one that should be executed
           \param choices actions, each with a normalized score
           \param loggingState IsEnabled if the caller wants extra debugging sent to the console
        */
        public abstract ActionSelection Select(IList<ActionSelection> choices,
                                               Logging loggingState);

    }
}
