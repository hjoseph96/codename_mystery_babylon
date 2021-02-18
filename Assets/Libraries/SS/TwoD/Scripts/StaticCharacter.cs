using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_5_5_OR_NEWER
using UnityEngine.AI;
#endif

namespace SS.TwoD
{
    public class StaticCharacter : Character
    {
        NavMeshObstacle m_NavObstacle;

        public override float GetRadius()
        {
            if (m_NavObstacle.shape == NavMeshObstacleShape.Box)
            {
                return m_NavObstacle.size.x;
            }

            return m_NavObstacle.radius;
        }

        protected override void Awake()
        {
            base.Awake();

            m_NavObstacle = GetComponent<NavMeshObstacle>();
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            m_SqrRealAttackRange = Sqr(attackRange);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
        }

        public override void FixedUpdateMe()
        {
            base.FixedUpdateMe();

            CheckTargetInRange();
            ProcessStates();
        }

        void CheckTargetInRange()
        {
            if (target != null)
            {
                if (Sqr(target.position.x - transform.position.x) + Sqr(target.position.z - transform.position.z) > m_SqrRealAttackRange)
                {
                    target = null;
                }
            }
        }

        void ProcessStates()
        {
            if (target != null && m_TargetCharacter.state != State.Die)
            {
                if (InAttackRange())
                {
                    FixedUpdateAttack();
                }
                else
                {
                    FixedUpdateIdle();
                }
            }
            else
            {
                FixedUpdateIdle();
            }
        }
    }
}