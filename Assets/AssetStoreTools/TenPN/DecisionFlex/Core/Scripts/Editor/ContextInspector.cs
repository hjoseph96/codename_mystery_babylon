// ******************************************************************************************
//
// 							DecisionFlex, (c) Andrew Fray 2014
//
// ******************************************************************************************
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace TenPN.DecisionFlex
{
    internal static class ContextIndex
    {
        // make this static so we can flip between multiple considerations and actions
        // without having to rescore
        public static int s_current = 0;
    }
        

    // looks directly at the contexts producted by a decision
    public abstract class ContextInspector<ContextContainer> : DecisionInspector
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Separator();

            if (EditorApplication.isPlaying)
            {
                DrawRuntimeDebugInformation();
            }    
        }

        protected override DecisionFlex Flex 
        { 
            get { return m_flex; }
        }

        protected override void OnEnable()
        {
            m_flex = FindDecisionFlex();

            base.OnEnable();
        }

        protected abstract IEnumerable<ContextContainer> GetContainersFor(
            MonoBehaviour targetBehaviour);

        protected abstract float GetScoreFromContainer(ContextContainer container);
        protected abstract IContext GetContextFromContainer(ContextContainer container);

        //////////////////////////////////////////////////

        DecisionFlex m_flex;

        //////////////////////////////////////////////////

        // fetches decision maker local to this context container.
        // returns null and prints warning if none found.
        DecisionFlex FindDecisionFlex()
        {
            // context objects should be children of a game object containing a decision 
            // maker.

            var inspectingComponent = (MonoBehaviour)target;
            
            if (inspectingComponent.transform.parent != null) 
            {
                var decisionMakerTransform = inspectingComponent.transform.parent;
                while(decisionMakerTransform.GetComponent<DecisionFlex>() == null)
                {
                    if (decisionMakerTransform.parent == null)
                    {
                        Debug.LogWarning("cannot find DecisionFlex as parent of " 
                                         + inspectingComponent.GetType() + " on object " 
                                         + inspectingComponent.gameObject.name,
                                         inspectingComponent.gameObject);
                        return null;
                    }
                    decisionMakerTransform = decisionMakerTransform.parent;
                }
                return decisionMakerTransform.GetComponent<DecisionFlex>();
            } 
            else
            {
                Debug.LogWarning("Inspector of " + inspectingComponent.GetType() + " on " 
                                 + inspectingComponent.gameObject.name
                                 + " expects to be child of decision maker", 
                                 inspectingComponent.gameObject);
                return null;
            }
        }

        void DrawRuntimeDebugInformation()
        {
            EditorGUILayout.LabelField("Last scored contexts", EditorStyles.boldLabel);

            var scoring = GetContainersFor(target as MonoBehaviour);

            int scoreCount = scoring.Count();

            if (scoreCount == 0)
            {
                EditorGUILayout.LabelField("(No scores)");
                return;
            }

            ContextIndex.s_current = EditorGUILayout.IntSlider(
                "Select context", ContextIndex.s_current, 0, scoreCount-1);

            EditorGUILayout.Separator();
            
            var container = scoring.ElementAt(ContextIndex.s_current);
            var score = GetScoreFromContainer(container);
            EditorGUILayout.LabelField("Score", score.ToString());

            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Context");

            var context = GetContextFromContainer(container);
            foreach(var key in context.AllKeys())
            {
                EditorGUILayout.LabelField(key.ToString(), 
                                           context.GetContext<object>(key).ToString());
            }
        }
    }
}
