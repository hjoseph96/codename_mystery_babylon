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
    [AddComponentMenu("TenPN/DecisionFlex/Demos/Timesliced/HealNearbyUFOs")]
    public class HealNearbyUFOs : MonoBehaviour
    {
        //////////////////////////////////////////////////

        private CircleCollider2D m_healZone;

        [SerializeField]
        private float m_healRate = 5f;

        [SerializeField]
        private UFOSpawner m_spawner;

        //////////////////////////////////////////////////

        void Start()
        {
            m_healZone = GetComponent<CircleCollider2D>();
        }

        void LateUpdate()
        {
            float restoredHealth = Time.deltaTime * m_healRate;
            HealNearbyUFOsBy(restoredHealth);
        }
        
        void HealNearbyUFOsBy(float restoredHealth)
        {
            var allUFOs = m_spawner.SpawnedUFOs;
            var maxHealSqrRadius = m_healZone.radius * m_healZone.radius;

            for(int ufoIndex = 0; ufoIndex < allUFOs.Count; ++ufoIndex) {
                var healCandidate = allUFOs[ufoIndex];
                var sqrDistToCandidate = (transform.position - healCandidate.transform.position).sqrMagnitude;
                if (sqrDistToCandidate <= maxHealSqrRadius) {
                    healCandidate.HP += restoredHealth;
                }
            }
        }
    }
}
