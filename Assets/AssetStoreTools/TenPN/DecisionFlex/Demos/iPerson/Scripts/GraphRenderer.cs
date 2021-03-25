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
using System;
using System.Collections.Generic;
using UnityEngine;

namespace TenPN.DecisionFlex.Demos
{
    public class GraphParameters
    {
        public Rect ScreenBounds;
        public Vector2 YValuesMinMax = Vector2.zero;
        public IList<IList<float>> YValuesList;
        public IList<float> XValues;
        public IEnumerator<Color> Cols;

        // x is min, y is max
        public Vector2 GetYValuesMinMax()
        {
            Vector2 minMax = new Vector2(float.MaxValue, -float.MaxValue);
            for(int listIndex = 0; listIndex < YValuesList.Count; ++listIndex)
            {
                var values = YValuesList[listIndex];
                for(int yIndex = 0; yIndex < values.Count; ++yIndex)
                {
                    float yValue = values[yIndex];
                    minMax.x = Mathf.Min(minMax.x, yValue);
                    minMax.y = Mathf.Max(minMax.y, yValue);
                }
            }
            return minMax;
        }
    }

    [AddComponentMenu("TenPN/DecisionFlex/Demos/iPerson/GraphRenderer")]
    public class GraphRenderer : MonoBehaviour
    {
        public static GraphRenderer Instance;

        public void RenderGraph(GraphParameters graph)
        {
            m_graphs.Enqueue(graph);
        }

        //////////////////////////////////////////////////

        Queue<GraphParameters> m_graphs = new Queue<GraphParameters>();

        [SerializeField]
        Material graphMat;

        //////////////////////////////////////////////////

        void Awake()
        {
            Instance = this;
        }

        void OnPostRender()
        {
            while(m_graphs.Count > 0)
            {
                var graph = m_graphs.Dequeue();
                PostRenderGraph(graph);
            }
        }

        void PostRenderGraph(GraphParameters graph)
        {
            if (graph.XValues.Count <= 1)
            {
                // not enough to render
                return;
            }

            GL.PushMatrix();
            graphMat.SetPass(0);
            GL.LoadOrtho();

            float normScreenGraphXMin = graph.ScreenBounds.x/Screen.width;
            float normScreenGraphYMin = (Screen.height - graph.ScreenBounds.y)/Screen.height;
            float normScreenGraphWidth = graph.ScreenBounds.width/Screen.width;
            float normScreenGraphHeight = graph.ScreenBounds.height/Screen.height;

            Rect normalizedGraphScreenRect = 
                new Rect(normScreenGraphXMin, normScreenGraphYMin,
                         normScreenGraphWidth, normScreenGraphHeight);

            bool isYRangeManuallySet = graph.YValuesMinMax != Vector2.zero;
            var graphMinMax = graph.GetYValuesMinMax();
            float minYValue = isYRangeManuallySet ? graph.YValuesMinMax.x : graphMinMax.x;
            float maxYValue = isYRangeManuallySet ? graph.YValuesMinMax.y : graphMinMax.y;
            float yScale = normalizedGraphScreenRect.height/(maxYValue - minYValue);

            float lastX = graph.XValues[graph.XValues.Count-1];
            float xScale = normalizedGraphScreenRect.width
                /(lastX- graph.XValues[0]);

            Func<float,float,Vector2> graphToGLCoord = (xValue, yValue) => 
                new Vector2(normalizedGraphScreenRect.x + xValue * xScale, 
                            normalizedGraphScreenRect.y - normalizedGraphScreenRect.height 
                            + yValue * yScale);

            // horizontal bars
            GL.Begin(GL.LINES);
            GL.Color(new Color(0.1f, 0.1f, 0.1f, 0.2f));

            GL.Vertex(graphToGLCoord(graph.XValues[0], minYValue));
            GL.Vertex(graphToGLCoord(lastX, minYValue));
            GL.Vertex(graphToGLCoord(graph.XValues[0], maxYValue));
            GL.Vertex(graphToGLCoord(lastX, maxYValue));
            GL.End();

            // vertical bars
            GL.Begin(GL.LINES);
            GL.Color(new Color(0.1f, 0.1f, 0.1f, 0.2f));
            foreach(var xValue in graph.XValues)
            {
                GL.Vertex(graphToGLCoord(xValue, minYValue));
                GL.Vertex(graphToGLCoord(xValue, maxYValue));
            }
            GL.End();

            foreach(var yValues in graph.YValuesList)
            {
                if (yValues.Count <= 1)
                {
                    continue;
                }

                GL.Begin(GL.LINES);

                int xIndexOffset = 
                    graph.XValues.Count - Mathf.Min(graph.XValues.Count, yValues.Count);
                
                var prevPoint = graphToGLCoord(graph.XValues[xIndexOffset], yValues[0]);

                graph.Cols.MoveNext();
                var graphColor = graph.Cols.Current;

                for(int valueIndex = 1; valueIndex < yValues.Count; ++valueIndex)
                {
                    GL.Color(graphColor);

                    float xValue = graph.XValues[valueIndex + xIndexOffset];
                    float yValue = yValues[valueIndex];

                    GL.Vertex(prevPoint);
                    var currentPoint = graphToGLCoord(xValue, yValue);
                    GL.Vertex(currentPoint);
                    
                    prevPoint = currentPoint;
                }

                GL.End();
            }

            GL.PopMatrix();
        }


    }
}
