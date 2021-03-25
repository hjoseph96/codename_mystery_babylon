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
using System.Collections.Generic;

namespace TenPN.DecisionFlex.Demos
{
    [AddComponentMenu("TenPN/DecisionFlex/Demos/Weapon Selection/EnemiesControl")]
    public class EnemiesControl : MonoBehaviour
    {
        public bool Paused
        {
            get { return m_isExternalPause; }
            set
            {
                m_isExternalPause = value;
                RefreshPause();
            }
        }
        
        //////////////////////////////////////////////////
        
        bool m_isPaused;
        IEnumerable<FollowWaypoints> m_enemies;
        bool m_isExternalPause = false;
        bool m_isUIPause = false;

        //////////////////////////////////////////////////

        void Awake()
        {
            m_enemies = GetComponentsInChildren<FollowWaypoints>();
        }
        
        void OnGUI()
        {
            var togglePauseRect = new Rect(20f, 20f, 400f, 40f);
            m_isUIPause = GUI.Toggle(togglePauseRect, m_isUIPause, "Pause enemies");
            RefreshPause();
        }

        void RefreshPause()
        {
            bool isNewPause = m_isUIPause || m_isExternalPause;
            if (isNewPause != m_isPaused)
            {
                foreach(var enemy in m_enemies)
                {
                    enemy.IsPaused = isNewPause;
                }
                m_isPaused = isNewPause;
            }
        }
    }
}