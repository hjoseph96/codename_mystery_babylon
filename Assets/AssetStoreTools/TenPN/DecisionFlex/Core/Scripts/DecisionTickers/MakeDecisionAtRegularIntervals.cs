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
       Sends message at a regular interval
    */
    [AddComponentMenu("TenPN/DecisionFlex/Decision Tickers/Regular DecisionTicker")]
    public class MakeDecisionAtRegularIntervals : DecisionTicker
    {
        //////////////////////////////////////////////////

        /** how many seconds between messages */
        [SerializeField] private float m_tickEvery;

        private float m_timeSinceLastTick = float.MaxValue; // always update on first tick

        //////////////////////////////////////////////////

        private void Update()
        {
            m_timeSinceLastTick += Time.deltaTime;
            if (m_timeSinceLastTick >= m_tickEvery)
            {
                MakeDecision();
                m_timeSinceLastTick = 0f;
            }
        }
    }
}
