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
       Sends set message every frame to target.
    */
    [AddComponentMenu("TenPN/DecisionFlex/Decision Tickers/Every Frame DecisionTicker")]
    public class MakeDecisionEveryFrame : DecisionTicker
    {
        //////////////////////////////////////////////////

        //////////////////////////////////////////////////

        private void Update()
        {
            MakeDecision();
        }
    }
}
