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
       Makes decision every time the gameobject enables
    */
    [AddComponentMenu("TenPN/DecisionFlex/Decision Tickers/Make Decision On Enable Ticker")]
    class MakeDecisionOnEnable : DecisionTicker
    {
        //////////////////////////////////////////////////

        private void OnEnable()
        {
            MakeDecision();
        }
    }
}
