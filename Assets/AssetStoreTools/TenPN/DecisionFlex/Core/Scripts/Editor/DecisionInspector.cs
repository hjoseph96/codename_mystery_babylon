// ******************************************************************************************
//
// 							DecisionFlex, (c) Andrew Fray 2014
//
// ******************************************************************************************
using UnityEditor;
using UnityEngine;

namespace TenPN.DecisionFlex
{
    // an inspector with helpers for dealing with a decision maker
    public abstract class DecisionInspector : Editor
    {

        protected abstract DecisionFlex Flex { get; }

        protected virtual void OnEnable()
        {
            EditorApplication.playmodeStateChanged += OnGameStart;
            OnGameStart();
        }

        protected virtual void OnNewAction(ActionSelection winningAction)
        {
            Repaint();
        }

        //////////////////////////////////////////////////

        //////////////////////////////////////////////////
        
        void OnDisable()
        {
            if (Flex != null)
            {
                Flex.OnNewAction -= OnNewAction;
            }
        }

        void OnGameStart()
        {
            if (Application.isPlaying == false || Flex == null)
            {
                return;
            }

            Flex.OnNewAction -= OnNewAction;
            Flex.OnNewAction += OnNewAction;        
        }

    }
}
