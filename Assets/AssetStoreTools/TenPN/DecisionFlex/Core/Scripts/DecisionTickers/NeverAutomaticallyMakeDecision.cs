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
       Dummy ticker that doesn't make a decision. To avoid the inspector warning. 

       \details
       You're responsible for calling decisionFlex.PerformAction()!
    */
    [AddComponentMenu("TenPN/DecisionFlex/Decision Tickers/Never Automatically Make Decisions")]
    class NeverAutomaticallyMakeDecision : DecisionTicker
    {
        // nothing! 
    }
}
