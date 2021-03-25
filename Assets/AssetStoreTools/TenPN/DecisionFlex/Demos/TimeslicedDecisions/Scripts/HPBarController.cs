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

namespace TenPN.DecisionFlex.Demos
{
    [AddComponentMenu("TenPN/DecisionFlex/Demos/Timesliced/HPBarController")]
    public class HPBarController : MonoBehaviour
    {
        //////////////////////////////////////////////////

        [SerializeField]
        private Transform m_fillBar;

        private UFO m_ufo;

        //////////////////////////////////////////////////

        void Start()
        {
            m_ufo = transform.parent.GetComponent<UFO>();
            m_ufo.HPChange += OnHPChange;
        }

        void OnDestroy()
        {
            if (m_ufo != null)
            {
                m_ufo.HPChange -= OnHPChange;
            }
        }

        void OnHPChange()
        {
            float fillWidth = m_ufo.NormalizedHP;
            var fillLocalScale = m_fillBar.localScale;
            fillLocalScale.x = fillWidth;
            m_fillBar.localScale = fillLocalScale;

            float fillRightOffset = (1 - m_ufo.NormalizedHP) * -0.5f;
            var fillLocalPos = m_fillBar.localPosition;
            fillLocalPos.x = fillRightOffset;
            m_fillBar.localPosition = fillLocalPos;
        }
    }
}