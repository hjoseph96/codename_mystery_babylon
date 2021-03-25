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

namespace TenPN.DecisionFlex.Demos
{
    [AddComponentMenu("TenPN/DecisionFlex/Demos/Timesliced/UFO")]
    public class UFO : MonoBehaviour
    {
        public float HP
        {
            get { return m_hp; }
            set
            {
                var newHP = Mathf.Clamp(value, 0f, m_maxHP);
                if (newHP == m_hp)
                {
                    return;
                }

                if (m_hp < newHP)
                {
                    m_healingAura.SetActive(true);
                }

                m_hp = newHP;
                if (HPChange != null)
                {
                    HPChange();
                }
            }
        }

        public float NormalizedHP
        {
            get { return HP / m_maxHP; }
        }

        public delegate void HPChangeCallback();
        public event HPChangeCallback HPChange;

        public Vector3 Velocity 
        {
            get; set;
        }

        //////////////////////////////////////////////////

        [SerializeField] 
        private float m_maxHP;
        private float m_hp;

        [SerializeField]
        private GameObject m_healingAura;

        //////////////////////////////////////////////////

        void Awake()
        {
            m_hp = m_maxHP;
            Velocity = Vector3.zero;
            m_healingAura.SetActive(false);
        }

        void Update()
        {
            m_healingAura.SetActive(false);
        }
    }
    
}