// ******************************************************************************************
//
// 							DecisionFlex, (c) Andrew Fray 2014
//
// ******************************************************************************************
using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace TenPN.DecisionFlex
{

    [CanEditMultipleObjects]
    [CustomEditor (typeof(DecisionFlex))]
    public class DecisionFlexInspector : DecisionInspector
    {

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var loggingFlagLabel =
                new GUIContent("Is logging enabled?",
                               "Dumps detailed info to console if ticked");
            EditorGUILayout.PropertyField(m_isLoggingEnabled, loggingFlagLabel);
                                          
	
            EditorGUILayout.Separator();

            if (EditorApplication.isPlaying)
            {
                OnRuntimeInspectorGUI();
            }
            else
            {
                // editor only:
                RenderBuilderHelper();
            }

            serializedObject.ApplyModifiedProperties();
        }

        protected override DecisionFlex Flex
        { 
            get { return target as DecisionFlex; } 
        }

        protected override void OnEnable()
        {
            m_recentDecisions = new Queue<GameObject>();
            m_isLoggingEnabled = serializedObject.FindProperty("m_isLoggingEnabled");
            base.OnEnable();
        }

        protected override void OnNewAction(ActionSelection winningAction)
        {
            m_recentDecisions.Enqueue(winningAction.ActionObject);

            if(m_recentDecisions.Count > 5)
            {
                m_recentDecisions.Dequeue();
            }

            m_scoreToRender = winningAction.Score;
            m_actionToRender = winningAction.ActionObject;
            m_contextToRender = winningAction.Context;

            base.OnNewAction(winningAction);
        }

        //////////////////////////////////////////////////

        private SerializedProperty m_isLoggingEnabled;

        private float m_scoreToRender;
        private GameObject m_actionToRender = null;
        private IContext m_contextToRender = null;

        private Queue<GameObject> m_recentDecisions;

        //////////////////////////////////////////////////

        private void RenderBuilderHelper()
        {
            // as we build the decision hierarchy, remind the user what's missing.
            var requiredScriptTypes = new Type[] { 
                typeof(DecisionTicker), 
                typeof(ActionSelector), 
                typeof(ContextFactory),
            };
            
            foreach(var requiredScript in requiredScriptTypes)
            {
                var instance = Flex.GetComponent(requiredScript);
                if (instance == null)
                {
                    EditorGUILayout.HelpBox("DecisionFlex on " 
                                            + Flex.gameObject.name 
                                            + " needs " + requiredScript + " script", 
                                            MessageType.Warning,
                                            true);
                }
            }
        }

        private void OnRuntimeInspectorGUI()
        {
            EditorGUILayout.LabelField("Recent Actions", EditorStyles.boldLabel);
            foreach(var decision in m_recentDecisions)
            {
                EditorGUILayout.LabelField(decision.name);
            }

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Winning Action", EditorStyles.boldLabel);
            RenderActionSelection(m_scoreToRender, m_actionToRender, m_contextToRender);
        }

        private void RenderActionSelection(float score,
                                           GameObject actionsObject,
                                           IContext context)
        {
            if (actionsObject == null)
            {
                EditorGUILayout.LabelField("(invalid)");
                return;
            }        

            RenderAction(actionsObject);
            EditorGUILayout.LabelField("Score", score.ToString());
            RenderContext(context);
        }

        private void RenderAction(GameObject actionsObject)
        {
            EditorGUILayout.LabelField("Action", actionsObject.name);
        }

        private void RenderContext(IContext context)
        {
            EditorGUILayout.LabelField("Context", EditorStyles.boldLabel);

            if (context == null)
            {
                EditorGUILayout.LabelField("(no context)");
                return;
            }

            foreach(var key in context.AllKeys())
            {
                EditorGUILayout.LabelField(key.ToString(), 
                                           context.GetContext<object>(key).ToString());
            }
        }

    }
}