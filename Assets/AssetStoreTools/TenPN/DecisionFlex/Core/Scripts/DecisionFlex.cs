// ******************************************************************************************
//
// 							DecisionFlex, (c) Andrew Fray 2014
//
// ******************************************************************************************
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TenPN.DecisionFlex
{
    /**
       \brief
       Represents one decision to be made with the DecisionFlex system.

       \details
       Create a dedicated gameobject for your decision, and add this to it. Also add a derived class of ActionSelector. 

       See \ref structure for more info.
    */
    [AddComponentMenu("TenPN/DecisionFlex/Decision Flex")]
    public class DecisionFlex : MonoBehaviour
    {
        //! subscribe to this event to be told when an action is chosen, just after the action is performed
        public event Action<ActionSelection> OnNewAction;

        /** 
            \brief
            call this, or send "PerformAction" event to this gameobject, to score, select and perform an action. 
            \details
            actually, it can be more than one action if the action gameobject contains more than one IAction.

            Returns the winning action, score and context. Or ActionSelection.Invalid if no winner.
        */
        public ActionSelection PerformAction()
        {
            if (enabled == false)
            {
                return ActionSelection.Invalid;
            }

            var winningAction = ChooseBestAction();
            if (winningAction.IsValid == false)
            {
                return ActionSelection.Invalid;
            }

            Log("taking action ", winningAction.ActionObject);
            for(int actionIndex = 0; 
                actionIndex < winningAction.Actions.Count; 
                ++actionIndex)
            {
                winningAction.Actions[actionIndex].Perform(winningAction.Context); 
            }

            if (OnNewAction != null)
            {
                OnNewAction(winningAction);
            }

            return winningAction;
        }

#if UNITY_EDITOR
        // debugging info

        public struct ContextScore
        {
            public IContext Context;
            public float Score;
        }

        // get all scores this consideration gave during last decision
        public IEnumerable<ContextScore> GetContextScoreForConsideration( 
            Consideration consideration)
        {
            if (m_considerationScores != null 
                && m_considerationScores.ContainsKey(consideration))
            {
                return m_considerationScores[consideration];
            }
            else
            {
                return Enumerable.Empty<ContextScore>();
            }
        }

        // get all scores this action object gave in the last round of decisions
        public IEnumerable<ActionSelection> GetSelectionsForAction(GameObject actionObject)
        {
            return m_allSelections == null
                ? Enumerable.Empty<ActionSelection>()
                : m_allSelections.Where(sel => sel.ActionObject == actionObject);
        }
#endif

        public void ClearContextScores()
        {
#if UNITY_EDITOR
            m_considerationScores.Clear();
#endif
        }

        public IEnumerable<ActionSelection> AllLastSelections
        {
            get { return m_allSelections; }
        }

        //////////////////////////////////////////////////

        private ContextFactory m_contextFactory = null;

        private class ActionConsiderations
        {
            public ActionConsiderations(IAction action) 
            {
                Actions = new List<IAction>(new IAction[] { action });
                Considerations = (action as MonoBehaviour)
                    .GetComponentsInChildren<Consideration>();
                ActionsObject = (action as MonoBehaviour).gameObject;
            }

            public void AddAction(IAction newAction) 
            {
                Actions.Add(newAction);
            }

            public GameObject ActionsObject;
            public IList<IAction> Actions;
            public IList<Consideration> Considerations;
        }
        private IList<ActionConsiderations> m_actions = null;

        // this could be local to ChooseBestAction, but we make it 
        // a member var so it cuts down on allocs, and can be used for debugging
        private IList<ActionSelection> m_allSelections = new List<ActionSelection>();

        /** \cond EDITOR_FIELDS */
        /** if true, editor user wants debugging information spat out */
        [SerializeField] private bool m_isLoggingEnabled = false;
        /** \endcond */

#if UNITY_EDITOR
        // so we can later analyse what scores each consideration gave
        private Dictionary<Consideration, IList<ContextScore>> m_considerationScores = 
            new Dictionary<Consideration, IList<ContextScore>>();
#endif

        private void RecordContextScore(Consideration consideration, 
                                        IContext context, 
                                        float score)
        {
#if UNITY_EDITOR
            if (m_considerationScores.ContainsKey(consideration) == false)
            {
                m_considerationScores[consideration] = new List<ContextScore>();
            }

            m_considerationScores[consideration].Add(new ContextScore {
                    Context = context,
                    Score = score,
                });
#endif
        }
    
        private ActionSelector m_selector = null;

        //////////////////////////////////////////////////

        private void Awake()
        {
            m_contextFactory = GetComponent<ContextFactory>();

            if (m_contextFactory == null)
            {
                throw new UnityException("DecisionFlex " + gameObject.name 
                                         + " has no associated context factory");
            }

            m_selector = GetComponent<ActionSelector>();
            if (m_selector == null)
            {
                throw new UnityException("DecisionFlex " + gameObject.name
                                         + " has no ActionSelector");
            }

            m_actions = new List<ActionConsiderations>();
            var allChildBehaviours = GetComponentsInChildren<MonoBehaviour>();
            foreach(var childBehaviour in allChildBehaviours)
            {
                var action = childBehaviour as IAction;
                if (action == null)
                {
                    continue;
                }

                var owningObject = childBehaviour.gameObject;
                var existantAction = m_actions.FirstOrDefault(
                    currentAction => currentAction.ActionsObject == owningObject);
            
                if (existantAction != null)
                {
                    existantAction.AddAction(action);
                }
                else
                {
                    var cachedActionConsiderations = new ActionConsiderations(action);
                    m_actions.Add(cachedActionConsiderations); 
                }
            }
        }

        private ActionSelection ChooseBestAction()
        {
#if UNITY_EDITOR
            ClearContextScores();
#endif
            m_allSelections.Clear();
        
            Log(" .. ");

            var contextLogging = m_isLoggingEnabled 
                ? ContextFactory.Logging.Enabled
                : ContextFactory.Logging.Disabled;

            var allContexts = m_contextFactory.AllContexts(contextLogging);
            for(int contextIndex = 0; contextIndex < allContexts.Count; ++contextIndex)
            {
                var context = allContexts[contextIndex];
                for(int actionIndex = 0; actionIndex < m_actions.Count; ++actionIndex)
                {
                    var action = m_actions[actionIndex];
                    float actionScore = ScoreAction(action, context);
                    var selection 
                        = new ActionSelection(actionScore, 
                                              action.ActionsObject, 
                                              action.Actions, 
                                              context);
                    m_allSelections.Add(selection);
                }
            }

            if(m_allSelections.Count == 0)
            {
                // nothing to do
                return ActionSelection.Invalid;
            }
        
            // ask selector to choose
            var loggingState = m_isLoggingEnabled ? ActionSelector.Logging.IsEnabled
                : ActionSelector.Logging.IsDisabled;
            var selectedAction = m_selector.Select(m_allSelections, loggingState);

            Log("Selected action: ", selectedAction.ActionObject, 
                " via selector ", m_selector.GetType());

            return selectedAction;
        }

        private float ScoreAction(ActionConsiderations action, IContext context)
        {
            Log("scoring action ", action.ActionsObject);

            float runningScore = 1.0f;

            for(int considerationIndex = 0; 
                considerationIndex < action.Considerations.Count; 
                ++considerationIndex) 
            {
                var consideration = action.Considerations[considerationIndex];
                Log(" -- scoring consideration ", consideration);

                float considerationScore = consideration.Consider(context);
                Log(" -- -- got score ", considerationScore);

                RecordContextScore(consideration, context, considerationScore);

                if (considerationScore == 0.0f) // can never be > 0
                {
                    return 0.0f;
                }

                runningScore *= considerationScore;
            }

            Log(" -- returning score ", runningScore);
            return runningScore;
        }

        private void Log(string msg)
        {
            if (m_isLoggingEnabled)
            {
                Debug.Log(msg);
            }
        }

        private void Log(string msg, Component param)
        {
            if (m_isLoggingEnabled)
            {
                Debug.Log(msg + param.gameObject.name);
            }
        }

        private void Log<T>(string msg, T param)
        {
            if (m_isLoggingEnabled)
            {
                Debug.Log(msg + param);
            }
        }

        private void Log(string msg, object param1, object param2)
        {
            if (m_isLoggingEnabled)
            {
                Debug.Log(msg + param1 + param2);
            }
        }

        private void Log(string msg, object param1, object param2, object param3)
        {
            if (m_isLoggingEnabled)
            {
                Debug.Log(msg + param1 + param2 + param3);
            }
        }

    }
}
