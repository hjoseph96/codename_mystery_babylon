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
using System.Collections;
using System.Collections.Generic;
using System;

using Action = System.Action;

namespace TenPN.DecisionFlex.Demos
{
    // given text, start T, end T will move label from start to end
    [AddComponentMenu("TenPN/DecisionFlex/Demos/Weapon Selection/LabelLerper")]
    public class LabelLerper : MonoBehaviour
    {
        public static void LerpLabelFromTo(string label, Vector3 fromPos, 
                                           Transform toTransform)
        {
            s_instance.StartCoroutine(
                s_instance.DoLerpLabelFromTo(label, fromPos, toTransform));
        }

        //////////////////////////////////////////////////

        static LabelLerper s_instance;

        [SerializeField] float m_smoothTime = 3f;
        [SerializeField] float m_initialPause = 1f;
        [SerializeField] float m_postPause = 0.5f;

        int m_nextID = 0;
        Dictionary<int, System.Action> m_renderers = new Dictionary<int, System.Action>();
        EnemiesControl m_enemies;

        //////////////////////////////////////////////////

        void Awake()
        {
            s_instance = this;
            m_enemies = FindObjectOfType<EnemiesControl>();
        }

        void OnDestroy()
        {
            s_instance = null;
        }
        
        void OnGUI()
        {
            foreach(var renderer in m_renderers.Values)
            {
                renderer();
            }
        }

        IEnumerator DoLerpLabelFromTo(string label, Vector3 fromPos, Transform toTransform)
        {
            m_enemies.Paused = true;
            int id = m_nextID;
            ++m_nextID;

            Vector3 renderWorldPos = fromPos;
            Vector3 velocity = Vector3.zero;

            System.Action renderer = () => {
                var labelSize = GUI.skin.box.CalcSize(new GUIContent(label));

                var renderScreenPos = Camera.main.WorldToScreenPoint(renderWorldPos);
                renderScreenPos.y = Screen.height - renderScreenPos.y;

                var labelRect = new Rect(renderScreenPos.x - labelSize.y,
                                         renderScreenPos.y,
                                         labelSize.x, labelSize.y);
                GUI.Box(labelRect, label);
            };
            
            m_renderers[id] = renderer;

            yield return new WaitForSeconds(m_initialPause);

            while(true)
            {
                renderWorldPos = Vector3.SmoothDamp(renderWorldPos, toTransform.position,
                                                    ref velocity, m_smoothTime);
                float sqDistToTarget = (toTransform.position - renderWorldPos).sqrMagnitude;
                if (sqDistToTarget < 0.35f)
                {
                    break;
                }
                yield return null;
            }

            m_renderers.Remove(id);

            yield return new WaitForSeconds(m_postPause);
            
            m_enemies.Paused = false;
        }
    }
}