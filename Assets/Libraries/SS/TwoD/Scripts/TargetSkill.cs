using UnityEngine;
using System.Collections;
using SS.Cache;

namespace SS.TwoD
{
    public class TargetSkill : UpdateRegister
    {
        [SerializeField] GameObject m_ExplosionPrefab;
        [SerializeField] float m_Speed;

        public enum State
        {
            FLYING,
            IMPACTED,
        }

        public Character target
        {
            get { return m_Target; }
            set
            {
                m_Target = value;

                if (m_Target != null)
                {
                    m_TargetPos = m_Target.center.position;
                }
            }
        }

        public float speed
        {
            get { return m_Speed; }
            set { m_Speed = value; }
        }

        public State state
        {
            get;
            set;
        }

        public int damage
        {
            get;
            set;
        }

        protected Character m_Target;
        protected Vector3 m_TargetPos;

        public override void UpdateMe()
        {
            switch (state)
            {
                case State.FLYING:
                    Flying();
                    break;
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            state = State.FLYING;
        }

        protected virtual void Flying()
        {
            // If Target object still alive, update its position
            if (IsTargetLive())
            {
                m_TargetPos = m_Target.center.position;
            }
            else
            {
                target = null;
            }

            // Distance to target object
            Vector3 ab = (m_TargetPos - transform.position);
            float sqrDistance = ab.sqrMagnitude;

            // Process
            if (sqrDistance <= 0.0025f)
            {
                Impacted();
            }
            else
            {
                Vector3 ac = ab.normalized * Time.deltaTime * speed;

                if (ac.sqrMagnitude < sqrDistance)
                {
                    transform.position += ac;
                }
                else
                {
                    transform.position += ab;
                }
            }
        }

        protected virtual void Impacted()
        {
            state = State.IMPACTED;

            if (IsTargetLive())
            {
                target.OnDamaged(damage);
            }

            Explosion();

            CacheManager.Destroy(gameObject);
        }

        protected virtual GameObject Explosion()
        {
            if (m_ExplosionPrefab != null)
            {
                var go = CacheManager.Instantiate(m_ExplosionPrefab);

                go.transform.position = transform.position;
                go.transform.rotation = Quaternion.identity;

                return go;
            }

            return null;
        }

        protected bool IsTargetLive()
        {
            return (target != null && target.state != MovingCharacter.State.Die);
        }
    }

}