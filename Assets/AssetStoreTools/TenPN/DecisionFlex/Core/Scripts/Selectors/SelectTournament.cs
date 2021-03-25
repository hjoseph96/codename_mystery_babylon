// ******************************************************************************************
//
// 							SelectTournament.cs (c) Andrew Fray 2015
//
// ******************************************************************************************
using UnityEngine;
using System.Collections.Generic;

namespace TenPN.DecisionFlex
{
    /**
       \brief
       Choose a good action but not always the best, using relative ranking.

       \details
       This is a good choice for your selector when you have to make the decision many times,
       and want a little bit of noise, but you also want the behavior to be invariant
       to the particular weights, and depend only on their relative weights.
       For example, "highest", "second ranked", "third ranked", and so on. It requires less
       tuning than SelectWeightedRandom, but will produce a "bad" choice more often.

       Thanks to Johnicholas Hines for the code.
    */
    [AddComponentMenu("TenPN/DecisionFlex/Selectors/Select By Tournament ActionSelector")]
    public class SelectTournament : ActionSelector
    {
        public override ActionSelection Select(IList<ActionSelection> choices,
                                               Logging loggingState)
        {
            s_shuffledChoices.Clear();
            s_shuffledChoices.AddRange(choices);
            Shuffle(s_shuffledChoices);

            float highestScore = float.NegativeInfinity;
            int chosenActionIndex = -1;
            int maxSteps = Mathf.Max(choices.Count, tournamentSize);
            for (int actionIndex = 0; actionIndex < maxSteps; ++actionIndex)
            {
                float choiceScore = s_shuffledChoices[actionIndex].Score;
                if ((choiceScore > 0f || m_isZeroScoreIgnored == false)
                    && choiceScore > highestScore)
                {
                    highestScore = choiceScore;
                    chosenActionIndex = actionIndex;
                }
            }

            return chosenActionIndex >= 0 ? s_shuffledChoices[chosenActionIndex]
                : ActionSelection.Invalid;
        }
        
        //////////////////////////////////////////////////

        [SerializeField] int tournamentSize = 2;
        [SerializeField] bool m_isZeroScoreIgnored = false;

        // we can share the shuffle buffer, since unity is single-threaded
        private static List<ActionSelection> s_shuffledChoices = new List<ActionSelection>();
        
        //////////////////////////////////////////////////

        static void Shuffle(IList<ActionSelection> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--; 
                int k = Random.Range(0, n+1);
                var value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

    }
}
