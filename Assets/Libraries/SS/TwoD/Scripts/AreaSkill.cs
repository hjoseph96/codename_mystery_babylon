using UnityEngine;
using System.Collections;
using SS.Cache;

namespace SS.TwoD
{
    public class AreaSkill : UpdateRegister
    {
        enum State
        {
            APPEAR,
            DAMAGED
        }

        [SerializeField] float m_DamageTime;
        [SerializeField] float m_Radius;

        float m_Time;
        float m_SqrRadius;
        State m_State;

        public int teamIndex
        {
            get;
            set;
        }

        public int damage
        {
            get;
            set;
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            m_Time = 0;
            m_State = State.APPEAR;
            m_SqrRadius = Sqr(m_Radius);
        }

        public override void FixedUpdateMe()
        {
            switch (m_State)
            {
                case State.APPEAR:
                    if (m_Time >= m_DamageTime)
                    {
                        m_State = State.DAMAGED;
                        Damage();
                    }
                    else
                    {
                        m_Time += Time.fixedDeltaTime;
                    }
                    break;
            }
        }

        void Damage()
        {
            if (CharacterManager.instance != null)
            {
                for (int i = 0; i < CharacterManager.instance.list.Count; i++)
                {
                    if (i != teamIndex)
                    {
                        for (int j = 0; j < CharacterManager.instance.list[i].Count; j++)
                        {
                            Vector3 pos = CharacterManager.instance.list[i][j].transform.position;

                            if (Sqr(pos.x - transform.position.x) + Sqr(pos.z - transform.position.z) <= m_SqrRadius)
                            {
                                CharacterManager.instance.list[i][j].OnDamaged(damage);
                            }
                        }
                    }
                }
            }
        }

        float Sqr(float x)
        {
            return x * x;
        }

        #if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (!Application.isPlaying)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, m_Radius);
            }
        }
        #endif
    }

}