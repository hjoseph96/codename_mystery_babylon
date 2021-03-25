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
    [AddComponentMenu("TenPN/DecisionFlex/Demos/Timesliced/UFOSpawner")]
    public class UFOSpawner : MonoBehaviour
    {
        public IList<UFO> SpawnedUFOs {
            get; private set;
        }
        
        //////////////////////////////////////////////////

        [SerializeField]
        private float m_spawnRate;

        [SerializeField]
        private float m_max;

        [SerializeField]
        private GameObject m_prefab;

        private float m_timeSinceLastSpawn;
        private CircleCollider2D m_spawnZone;

        //////////////////////////////////////////////////

        void Start()
        {
            m_spawnZone = GetComponent<CircleCollider2D>();
            SpawnedUFOs = new List<UFO>();
        }

        void Update()
        {
            if (transform.childCount >= m_max)
            {
                return;
            }

            m_timeSinceLastSpawn += Time.deltaTime;
            float spawnInterval = 1f/m_spawnRate;

            if (m_timeSinceLastSpawn >= spawnInterval)
            {
                SpawnUFO();
                m_timeSinceLastSpawn -= spawnInterval;
            }
        }

        void SpawnUFO()
        {
            var spawnPos = Random.insideUnitCircle.normalized * m_spawnZone.radius;
            var newUFO = (GameObject)Instantiate(m_prefab, spawnPos, Quaternion.identity);
            newUFO.transform.parent = transform;
            newUFO.name = "UFO id:" + newUFO.GetInstanceID();
            SpawnedUFOs.Add(newUFO.GetComponent<UFO>());
        }
    }
}
