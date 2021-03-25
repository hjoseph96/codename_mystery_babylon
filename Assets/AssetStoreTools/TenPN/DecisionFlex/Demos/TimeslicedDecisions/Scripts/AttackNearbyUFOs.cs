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

namespace TenPN.DecisionFlex.Demos
{
    [AddComponentMenu("TenPN/DecisionFlex/Demos/Timesliced/AttackNearbyUFOs")]
    public class AttackNearbyUFOs : MonoBehaviour
    {
        //////////////////////////////////////////////////

        private CircleCollider2D m_attackZone;

        [SerializeField]
        private float m_attackStrength = 5f;

        [SerializeField]
        private float m_attackInterval = 1f;

        [SerializeField]
        private Transform m_laser;

        [SerializeField]
        private UFOSpawner m_spawner;

        //////////////////////////////////////////////////

        void Start()
        {
            m_attackZone = GetComponent<CircleCollider2D>();
            m_laser.gameObject.SetActive(false);
            StartCoroutine(Attack());
        }

        IEnumerator Attack()
        {
            m_attackInterval = Mathf.Max(m_attackInterval, 1/60f);

            float timeToNextAttack = m_attackInterval;
            
            while(true)
            {
                timeToNextAttack -= Time.deltaTime;
                if (timeToNextAttack < 0)
                {
                    var target = FindUFOToAttack();
                    if (target != null)
                    {
                        StartCoroutine(AttackUFO(target));
                    }
                    timeToNextAttack += m_attackInterval;
                }
                yield return null;
            }
        }
        
        // returns null if no target
        UFO FindUFOToAttack()
        {
            var allUFOs = m_spawner.SpawnedUFOs;
            if (allUFOs.Count == 0) {
                return null;
            }

            var maxSqrDist = m_attackZone.radius * m_attackZone.radius;
            
            int randomStart = Random.Range(0, allUFOs.Count);
            // try to find a random UFO within range:
            for(int ufoIndex = 0; ufoIndex < allUFOs.Count; ++ufoIndex) {
                int candidateIndex = (ufoIndex + randomStart) % allUFOs.Count;
                var candidate = allUFOs[candidateIndex];

                var sqrDistToCandidate = (transform.position - candidate.transform.position).sqrMagnitude;
                if (sqrDistToCandidate <= maxSqrDist) {
                    return candidate;
                }
            }

            return null;
        }

        IEnumerator AttackUFO(UFO victim)
        {
            m_laser.position = transform.position;
            m_laser.gameObject.SetActive(true);
            
            float attackDuration = m_attackInterval * 0.8f;
            float attackTimer = 0f;
            var victimTransform = victim.transform;

            while(attackTimer <= attackDuration) {
                float t = attackTimer / attackDuration;
                m_laser.position =
                    Vector3.Lerp(transform.position, victimTransform.position, t);

                var toTarget = (victimTransform.position - transform.position).normalized;
                float sign = Vector3.Dot(toTarget, Vector3.right) > 0 ? -1f : 1f;
                float rotation = Vector3.Angle(Vector3.up, toTarget) * sign;
                m_laser.eulerAngles = transform.eulerAngles = Vector3.forward * rotation;

                attackTimer += Time.deltaTime;
                
                yield return null;
            }

            m_laser.gameObject.SetActive(false);
            victim.HP -= m_attackStrength;
        }
    }
}
