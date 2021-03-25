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
    //! logs HP, distance to enemy and distance to health
    [AddComponentMenu("TenPN/DecisionFlex/Demos/Timesliced/UFOContextFactory")]
    public class UFOContextFactory : SingleContextFactory
    {
        // returns considerations of this ufo
        public override IContext SingleContext(Logging loggingSetting)
        {
            var distanceToHealth = 
                Mathf.Max(0f, 
                          (m_healthArea.transform.position - transform.position).magnitude 
                          - m_healthArea.radius);
            m_context.SetContext("DistanceToHealth", distanceToHealth);

            var distanceToEnemy = 
                Mathf.Max(0f, 
                          (m_enemyArea.transform.position - transform.position).magnitude 
                          - m_enemyArea.radius);
            m_context.SetContext("DistanceToEnemy", distanceToEnemy);

            m_context.SetContext("HP", m_ufo.NormalizedHP);

            return m_context;
        }

        //////////////////////////////////////////////////

        // avoid allocations by caching everything we can reuse
        private ContextDictionary m_context = new ContextDictionary();

        private CircleCollider2D m_healthArea;
        private CircleCollider2D m_enemyArea;
        private UFO m_ufo;

        //////////////////////////////////////////////////

        private void Awake()
        {
            m_healthArea = (CircleCollider2D)GameObject.FindWithTag("Health").GetComponent<Collider2D>();
            m_enemyArea = (CircleCollider2D)GameObject.FindWithTag("Enemy").GetComponent<Collider2D>();
            m_ufo = transform.parent.GetComponent<UFO>();
        }
    }
}
