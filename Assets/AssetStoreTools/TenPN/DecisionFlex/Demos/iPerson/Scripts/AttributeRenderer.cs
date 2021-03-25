/*
The MIT License (MIT)

Copyright (c) 2014 Andrew Fray

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
 */
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;

namespace TenPN.DecisionFlex.Demos
{
    /** graph all attributes over time, on screen */
    [AddComponentMenu("TenPN/DecisionFlex/Demos/iPerson/Attribute Renderer")]
    public class AttributeRenderer : MonoBehaviour
    {
        //////////////////////////////////////////////////

        [Range(0f,2f)]
        [SerializeField] private float m_sampleInterval;

        [SerializeField] private int m_historySize = 50;

        [Range(0f,1f)]
        [SerializeField] private float m_screenWidthProp = 1f;

        [Range(0f,1f)]
        [SerializeField] private float m_screenHeightProp = 1f;

        [Range(0f,1f)]
        [SerializeField] private float m_screenYProp = 1f;

        [SerializeField] private Vector2 m_barsOriginProp;
        [SerializeField] private Vector2 m_barSizeProp;
        [Range(0f,1f)]
        [SerializeField] private float m_sliderProp;

        private iPerson person;
        private Dictionary<Attribute, Queue<float>> m_attributeHistories;
        private Dictionary<GameObject, Queue<float>> m_actionScoreHistories = 
            new Dictionary<GameObject, Queue<float>>();
        private Queue<string> m_actionsHistory = new Queue<string>();
        private int m_currentHistorySize;

        enum Source
        {
            Attributes,
            Scores,
        }
        private Source m_currentSource = Source.Attributes;

        private string m_pendingAction;
        private DecisionFlex m_flex;
        private bool m_isPaused = true;

        //////////////////////////////////////////////////

        private void Awake()
        {
            IsPaused = m_isPaused;

            person = GetComponentInParent<iPerson>();

            m_attributeHistories = new Dictionary<Attribute, Queue<float>>();
            foreach(var attribute in iPerson.Attributes) 
            {
                m_attributeHistories[attribute] = new Queue<float>();
            }
        }

        private void Start()
        {
            m_flex = transform.parent.GetComponentInChildren<DecisionFlex>();
            if (m_flex != null)
            {
                m_flex.OnNewAction += OnAction;
            }

            var allActions = transform.parent.GetComponentsInChildren<Action>();
            foreach(var action in allActions)
            {
                m_actionScoreHistories[action.gameObject] = new Queue<float>();
            }

            StartCoroutine(Sampler());
        }

        private bool IsPaused
        {
            get
            {
                return m_isPaused;
            }
            set
            {
                Time.timeScale = value ? 0f : 1f;
                m_isPaused = value;
            }
        }

        private IEnumerator Sampler()
        {
            // let's run some early decisions to fill out data
            m_currentHistorySize = 3;
            for(int warmupIndex = 0; warmupIndex < m_currentHistorySize; ++warmupIndex) {
                if (m_flex != null)
                {
                    m_flex.PerformAction();
                    RecordActionScores();
                }
                RecordAttributes();
            }

            while(true)
            {
                // could use InvokeRepeating here, but then you couldn't
                // fiddle with the sample interval in the editor during play.
                m_currentHistorySize = Mathf.Min(m_currentHistorySize + 1, m_historySize);
                if (m_flex != null)
                {
                    RecordActionScores();
                }
                RecordAttributes();
                yield return new WaitForSeconds(m_sampleInterval);
                yield return new WaitForEndOfFrame();
            }
        }

        private void RecordAttributes()
        {
            foreach(var attributeHistory in m_attributeHistories)
            {
                var attribute = attributeHistory.Key;
                var history = attributeHistory.Value;

                PushWithRestrictedSize(person.GetAttribute(attribute), history);
            }

            PushWithRestrictedSize(m_pendingAction, m_actionsHistory);
            m_pendingAction = null;
        }

        private void RecordActionScores()
        {
            var allActionScores = m_flex.AllLastSelections;
            bool isActionTurn = string.IsNullOrEmpty(m_pendingAction) == false;
            
            foreach(var actionScore in allActionScores)
            {
                var action = actionScore.ActionObject;

                Queue<float> recentActionScores = m_actionScoreHistories[action];

                float scoreValue = isActionTurn ? actionScore.Score : 0f;
                PushWithRestrictedSize(scoreValue, recentActionScores);
            }
        }

        private void PushWithRestrictedSize<T>(T data, Queue<T> store)
        {
            store.Enqueue(data);
            while (store.Count > m_historySize)
            {
                store.Dequeue();
            }
        }

        private void OnGUI()
        {
            RenderAttributeBars();
            
            int lastOffset = m_currentHistorySize - 1;

            if (lastOffset < 0)
            {
                return;
            }

            const float padding = 0.1f;
            float graphScreenWidth = Screen.width * m_screenWidthProp;
            float graphScrenHeight = Screen.height * m_screenHeightProp;
            float graphY = Screen.height * m_screenYProp;

            var graphRect = new Rect(padding, graphY,
                                     graphScreenWidth, graphScrenHeight);

            RenderControlAroundGraph(graphRect);

            var xValues = Enumerable.Range(0, m_currentHistorySize)
                .Select(i => (float)i).ToArray();
            var baseGraph = new GraphParameters {
                ScreenBounds = graphRect,
                YValuesMinMax = new Vector2(0f, 1f),
                XValues = xValues,
                Cols = ColorGenerator(),
            };

            if (m_currentSource == Source.Attributes)
            {
                RenderHistories(m_attributeHistories, baseGraph, att => att.ToString());
            }
            else
            {
                RenderHistories(m_actionScoreHistories, baseGraph, act => act.name);
            }

            RenderActionsOnGraph(graphRect);
        }

        private void RenderAttributeBars()
        {
            float barWidth = m_barSizeProp.x * Screen.width;
            float barHeight = Screen.height * m_barSizeProp.y;

            float originX = Screen.width * m_barsOriginProp.x;
            float originY = Screen.height * m_barsOriginProp.y;

            var barRect = new Rect(originX, originY, barWidth, barHeight);

            float sliderHeight = barHeight * m_sliderProp;
            float sliderYOffset = (barHeight - sliderHeight) * 0.5f;

            foreach(var attr in m_attributeHistories.Keys)
            {
                int pct = (int)(100*person.GetAttribute(attr));

                var sliderRect = new Rect(barRect.xMin, barRect.yMin + sliderYOffset,
                                          barRect.width, sliderHeight);
                GUI.VerticalScrollbar(sliderRect, 0f, pct, 100f, 0f);

                GUILayout.BeginArea(barRect);
                GUILayout.BeginVertical();

                GUILayout.Label(attr.ToString());
                GUILayout.FlexibleSpace();
                GUILayout.Label(pct + "%");

                GUILayout.EndVertical();
                GUILayout.EndArea();
                
                barRect.x += barWidth;
            }
        }

        private void RenderActionsOnGraph(Rect graphRect)
        {
            // render decisions label vertically
            var pos = new Vector2(graphRect.x - 20f, Screen.height - 5f);
            var quat = Quaternion.identity; 
            quat.eulerAngles = new Vector3(0f, 0f, -90f);
            var oldMatrix = GUI.matrix;
            GUI.matrix = Matrix4x4.TRS(pos, quat, Vector3.one); 

            var actionsLabelRect = new Rect(0f, 0f,
                                            150f, 50f);
            GUI.Label(actionsLabelRect, "Decisions");

            GUI.matrix = oldMatrix;

            int historyIndex = 0;
            foreach(var action in m_actionsHistory)
            {
                if (action != null)
                {
                    var actionScreenPos = 
                        CalculateScreenCoord(historyIndex, 0f, graphRect);
                    var actionLabelRect = new Rect(actionScreenPos.x - 20f,
                                                   actionScreenPos.y + 10f,
                                                   150f, 50f);
                    GUI.Label(actionLabelRect, action);
                }

                ++historyIndex;
            }
        }

        private void RenderHistories<T>(Dictionary<T,Queue<float>> histories, 
                                        GraphParameters baseGraph,
                                        Func<T,string> toString)
        {
            var yValuesList = new List<IList<float>>();
            foreach(var value in histories.Values)
            {
                yValuesList.Add(value.ToArray());
            }
            baseGraph.YValuesList = yValuesList;
            GraphRenderer.Instance.RenderGraph(baseGraph);

            foreach(var history in histories)
            {
                var key = history.Key;
                var scores = history.Value;
            
                var lastInterestingIndexValue = LastNotableValueIn(scores.ToArray());
                var screenCoord = CalculateScreenCoord(lastInterestingIndexValue.Key,
                                                       lastInterestingIndexValue.Value,
                                                       baseGraph.ScreenBounds);

                var labelRect = new Rect(screenCoord.x - 20.0f, 
                                         screenCoord.y, 
                                         150.0f, 50.0f);
                GUI.Label(labelRect, toString(key));
            }
        }

        private KeyValuePair<int,float> LastNotableValueIn(IList<float> values)
        {
            for(int index = values.Count-1; index >= 0; --index)
            {
                if (values[index] > 0f)
                {
                    return new KeyValuePair<int,float>(index, values[index]);
                }
            }

            // just go with last value
            return new KeyValuePair<int,float>(values.Count-1, values[values.Count-1]);
        }

        private void RenderControlAroundGraph(Rect graphRect)
        {
            const float gap = 5f;
            
            var controlsRect = new Rect(graphRect.xMin + gap, gap, 
                                        graphRect.width, graphRect.yMin - gap);

            GUILayout.BeginArea(controlsRect);
            GUILayout.BeginHorizontal();

            IsPaused = GUILayout.Toggle(IsPaused, "Pause");

            GUILayout.FlexibleSpace();

            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        private void OnAction(ActionSelection winningAction)
        {
            m_pendingAction = winningAction.ToString();
        }

        private Vector2 CalculateScreenCoord(int historyIndex,
                                             float value,
                                             Rect graphScreenRect)
        {
            return new Vector2(
                graphScreenRect.x 
                + (historyIndex + 1)/((float)m_currentHistorySize) * graphScreenRect.width,
                graphScreenRect.yMax - graphScreenRect.height * value
                );
        }

        private int HistoryOffsetToIndex(int offset)
        {
            return offset;
        }

        private IEnumerator<Color> ColorGenerator()
        {
            var colorRand = new System.Random(5);

            while(true)
            {
                var colour = new Color((float)colorRand.NextDouble(),
                                       (float)colorRand.NextDouble(),
                                       (float)colorRand.NextDouble(),
                                       0.85f);
                yield return colour;
            }
        }
    }
}
