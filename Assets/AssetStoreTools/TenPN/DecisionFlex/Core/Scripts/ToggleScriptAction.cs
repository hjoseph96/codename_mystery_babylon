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
       helper action that toggles state of target behaviour
       
       \details
       If you have a script that you want enabled when the action is chosen, and disabled when not, this helper action is useful.

       Actions don't normally get callbacks for _not_ being chosen, so this finds the main DecisionFlex object and connects to its OnNewAction event. 
    */
    public class ToggleScriptAction : Action
    {

        public override void Perform(IContext context)
        {
            // we do all our work in OnActionSelection
        }

        //////////////////////////////////////////////////

        [SerializeField]
        private Behaviour m_target;

        private DecisionFlex m_flex;

        //////////////////////////////////////////////////

        private void Awake()
        {
            // everything off by default
            m_target.enabled = false;
        }

        private void Start()
        {
            m_flex = FindDecisionFlex();
            m_flex.OnNewAction += OnActionSelection;
        }

        private void OnDestroy()
        {
            if(m_flex != null)
            {
                m_flex.OnNewAction -= OnActionSelection;
            }
        }

        private void OnActionSelection(ActionSelection winningAction) 
        {
            bool isWinner = winningAction.ActionObject == gameObject;
            m_target.enabled = isWinner;
        }

        private DecisionFlex FindDecisionFlex()
        {
            var currentSearchObject = transform.parent;

            while(currentSearchObject != null)
            {
                var maker = currentSearchObject.GetComponent<DecisionFlex>();
                if (maker != null)
                {
                    return maker;
                }
                currentSearchObject = currentSearchObject.parent;
            }
            
            // can't find it
            throw new UnityException("ToggleActionScript on " + gameObject.name 
                                     + " cannot find DecisionFlex in parent gameobjects");
        }
    }
}
