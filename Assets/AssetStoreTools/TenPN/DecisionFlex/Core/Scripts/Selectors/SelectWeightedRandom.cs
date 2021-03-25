// ******************************************************************************************
//
// 							DecisionFlex, (c) Andrew Fray 2014
//
// ******************************************************************************************
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace TenPN.DecisionFlex
{
    /**
       \brief
       Choose a good action but not always the best, using weights.

       \details
       This is a good choice for your selector when you have to make the decision many times, 
       and want a little bit of noise. You can use a low-pass filter to ignore completely 
       unreasonable things.
       By default it uses unity's RNG, but you can derive from this class to override the RNG.
    */
    [AddComponentMenu("TenPN/DecisionFlex/Selectors/Select Weighted Random ActionSelector")]
    public class SelectWeightedRandom : ActionSelector
    {
        public override ActionSelection Select(IList<ActionSelection> choices,
                                               Logging loggingState)
        {
            float totalScore = 0f;
            for(int choiceIndex = 0; choiceIndex < choices.Count; ++choiceIndex)
            {
                var choice = choices[choiceIndex];
                if (IsScoreAcceptable(choice.Score))
                {
                    totalScore += choice.Score;
                }
            }

            float selectScore = GetNextNormalisedRandom() * totalScore;
        
            for(int choiceIndex = 0; choiceIndex < choices.Count; ++choiceIndex)
            {
                var choice = choices[choiceIndex];
                if (IsScoreAcceptable(choice.Score) == false)
                {
                    continue;
                }

                selectScore -= choice.Score;
                if (selectScore <= 0.0f)
                {
                    return choice;
                }
            }

            return choices.Last();
        }

        /**
           \brief override this to change the RNG used to select the action
        */
        protected virtual float GetNextNormalisedRandom()
        {
            return UnityEngine.Random.value;
        }

        //////////////////////////////////////////////////

        [SerializeField] float m_minScoreCutoff = 0f;

        //////////////////////////////////////////////////

        bool IsScoreAcceptable(float score)
        {
            return score >= m_minScoreCutoff;
        }
    }
}
