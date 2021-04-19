using UnityEngine;
using System.Collections;
using SS.Cache;

namespace SS.TwoD
{
    public class ParticleAutoDestroy : UpdateRegister
    {
        ParticleSystem m_Ps;
        float m_LifeTime;
        float m_Time;

        void Awake()
        {
            m_Ps = GetComponent<ParticleSystem>();
            #if UNITY_5_5_OR_NEWER
            m_LifeTime = m_Ps.main.startLifetimeMultiplier;
            #else
            m_LifeTime = m_Ps.startLifetime;
            #endif
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            m_Time = 0;
        }

        public override void UpdateMe()
        {
            if (m_Time >= m_LifeTime)
            {
                CacheManager.Destroy(gameObject);
            }
            else
            {
                m_Time += Time.deltaTime;
            }
        }
    }

}