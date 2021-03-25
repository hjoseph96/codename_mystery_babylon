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
       Choose the highest-scoring action 

       \details
       This is a good choice for your selector when you always want the definitive choice
    */
    [AddComponentMenu("TenPN/DecisionFlex/Selectors/Select Highest Score ActionSelector")]
    public class SelectHighestScore : ActionSelector
    {
        public override ActionSelection Select(IList<ActionSelection> choices,
                                               Logging loggingState)
        {
            float highestScore = float.NegativeInfinity;
            int chosenActionIndex = -1;
            for(int actionIndex = 0; actionIndex < choices.Count; ++actionIndex)
            {
                float choiceScore = choices[actionIndex].Score;
                if ((choiceScore > 0f || m_isZeroScoreIgnored == false)
                    && choiceScore > highestScore)
                {
                    highestScore = choiceScore;
                    chosenActionIndex = actionIndex;
                }
            }

            return chosenActionIndex >= 0 ? choices[chosenActionIndex]
                : ActionSelection.Invalid;
        }

        //////////////////////////////////////////////////

        [SerializeField] bool m_isZeroScoreIgnored;

        //////////////////////////////////////////////////

    }
}
