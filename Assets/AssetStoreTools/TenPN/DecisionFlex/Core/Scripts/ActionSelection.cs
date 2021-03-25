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
       The snapshot of scoring all the considerations associated with a single action object

       \details
       A list of these, one per action object, is passed to the \ref TenPN.DecisionFlex.ActionSelector "ActionSelector" for this decision. The winning selection is returned from \ref TenPN.DecisionFlex.DecisionFlex.PerformAction "DecisionFlex.PerformAction" and also via the \ref TenPN.DecisionFlex.DecisionFlex OnNewAction "DecisionFlex.OnNewAction" callback.
    */
    public struct ActionSelection
    {
        public readonly static ActionSelection Invalid = new ActionSelection();

        public ActionSelection(float score, GameObject actionObject, 
                               IList<IAction> actions, 
                               IContext context) 
        : this()
        {
            Score = score;
            ActionObject = actionObject;
            Actions = actions;
            Context = context;
        }

        public bool IsValid { get { return Actions != null; } }
        
        public bool IsBetterThan(float otherScore)
        {
            return Score > otherScore;
        }

        public override string ToString()
        {
            return IsValid ? ActionObject.name : "NONE";
        }

        /** the normalized score of the action object. not less than 0, normally less than 1*/
        public float Score;
        public GameObject ActionObject;
        /** since one action object can contain multiple actions, here they all are */
        public IList<IAction> Actions;
        /** the context used by the considerations to come to this score */
        public IContext Context;
    }
}