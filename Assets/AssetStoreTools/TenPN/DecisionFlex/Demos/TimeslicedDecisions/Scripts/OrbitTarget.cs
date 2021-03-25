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
    //! orbits around enemy, attacking it
    [AddComponentMenu("TenPN/DecisionFlex/Demos/Timesliced/OrbitTarget")]
    public class OrbitTarget : MonoBehaviour
    {
        //////////////////////////////////////////////////

        private Transform m_us;
        private Transform m_target;

        [SerializeField]
        private float m_orbitSpeed = 1f;

        [SerializeField]
        private float m_orbitHeightForce = 1f;

        [SerializeField]
        private float m_orbitSpeedForce = 1f;

        [SerializeField]
        private string m_targetTag = "Enemy";

        //////////////////////////////////////////////////

        private void Awake()
        {
            m_target = GameObject.FindWithTag(m_targetTag).transform;
            
            m_us = transform.parent.parent;
        }

        private void Update()
        {
            var ufoPos = m_us.position;
            var toTarget = (m_target.position - ufoPos).normalized;
            float sign = Vector3.Dot(toTarget, Vector3.right) > 0 ? -1f : 1f;
            float currentAngle = Mathf.Deg2Rad * Vector3.Angle(Vector3.up, toTarget) * sign;

            float nextAngle = currentAngle + m_orbitSpeed * Time.deltaTime;
            var desiredRadial = new Vector3(Mathf.Sin(nextAngle), -Mathf.Cos(nextAngle));
            
            var desiredPos = m_target.position + desiredRadial * m_orbitHeightForce;

            float maxMove = m_orbitSpeedForce * Time.deltaTime;
            m_us.position = Vector3.MoveTowards(ufoPos, desiredPos, maxMove);
        }
    }
}