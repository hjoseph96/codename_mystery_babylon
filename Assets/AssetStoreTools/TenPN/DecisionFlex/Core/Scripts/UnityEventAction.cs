// ******************************************************************************************
//
// 							DecisionFlex, (c) Andrew Fray 2014
//
// ******************************************************************************************
using UnityEngine;
using UnityEngine.Events;

namespace TenPN.DecisionFlex
{
    /** 
        \brief 
        Use this to easily connect an action to another part of the code, eg driving animation.
    */
    [AddComponentMenu("TenPN/DecisionFlex/UnityEventAction")]
    public class UnityEventAction : Action
    {
        /** 
            \brief
            This event fires when the action is selected. Connect to it in an inspector.
        */
        public UnityEvent Event;
        
        public override void Perform(IContext context)
        {
            Event.Invoke();
        }
    }
}
