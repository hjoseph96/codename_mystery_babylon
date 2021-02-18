using UnityEngine;
using System.Collections;

namespace SS.TwoD
{
    public class SpriteRendererEffect : UpdateRegister
    {
        [SerializeField] AudioSource m_AttackSfx;

        public enum State
        {
            NORMAL,
            NORMAL_TO_DAMAGE,
            DAMAGE_TO_NORMAL
        }

        protected float m_Time;

        protected SpriteAnimator m_Animator;
        protected State m_State;
        protected Color m_DamageColor;
        protected Color m_NormalColor;

        public virtual void Normal()
        {
            m_State = State.NORMAL;
            color = m_NormalColor;
            m_Time = 0;
        }

        public virtual void Damage()
        {
            m_State = State.NORMAL_TO_DAMAGE;
            m_Time = 0;
            color = m_DamageColor;

            if (m_AttackSfx != null)
            {
                m_AttackSfx.Play();
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            Normal();
        }

        public override void FixedUpdateMe()
        {
            switch (m_State)
            {
                case State.NORMAL_TO_DAMAGE:
                    if (m_Time >= 0.1f)
                    {
                        m_State = State.DAMAGE_TO_NORMAL;
                        m_Time = 0;
                    }
                    else
                    {
                        color = Color.Lerp(m_NormalColor, m_DamageColor, m_Time / 0.1f);
                        m_Time += Time.fixedDeltaTime;
                    }
                    break;
                case State.DAMAGE_TO_NORMAL:
                    if (m_Time >= 0.2f)
                    {
                        Normal();
                    }
                    else
                    {
                        color = Color.Lerp(m_DamageColor, m_NormalColor, m_Time / 0.2f);
                        m_Time += Time.fixedDeltaTime;
                    }
                    break;
            }
        }

        protected virtual void Awake()
        {
            m_Animator = GetComponent<SpriteAnimator>();
            m_NormalColor = new Color(1f, 1f, 1f, 1f);
            m_DamageColor = new Color(1f, 0.5f, 0.5f, 1f);
        }

        protected Color color
        {
            set
            {
                if (m_Animator != null)
                {
                    m_Animator.bodyRenderer.color = value;
                    m_Animator.colorRenderer.color = value;
                }
            }
        }
    }
}