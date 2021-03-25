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
       Kicks off another decisionflex tree when selected. 

       \remarks
       See DecisionFlex_iPerson_NestedDecisions for an example. 
       Useful for (eg) a decision that chooses to eat then a nested decision that chooses where to eat.
       You want your nested decision to have NeverAutomaticallyMakeDecision as a ticker, so it never 
       triggers itself, and link it from here. 
    */
    [AddComponentMenu("TenPN/DecisionFlex/NestedDecisionAction")]
    public class NestedDecisionAction : Action
    {
        public override void Perform(IContext context) {
            m_nestedDecisionToTrigger.PerformAction();
        }

        //////////////////////////////////////////////////

        [SerializeField] private DecisionFlex m_nestedDecisionToTrigger;
    }
}
