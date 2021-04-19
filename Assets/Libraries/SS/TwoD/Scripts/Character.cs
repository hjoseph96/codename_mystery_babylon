using UnityEngine;
using System.Collections;
using SS.Cache;

namespace SS.TwoD
{
    public class Character : SpriteAnimatorController
    {
        public readonly static float ELLIPSE_RATIO = 0.71f;

        public enum State
        {
            Idle,
            Moving,
            Attacking,
            Die
        }

        public enum RangeType
        {
            Melee,
            RangeTarget,
            Area
        }

        [SerializeField] protected int m_TeamIndex;
        [SerializeField] protected int m_MaxHp;
        [SerializeField] protected int m_Damage;
        [SerializeField] protected float m_AttackRange = 0.15f;
        [SerializeField] protected float m_ArmLength;
        [SerializeField] protected RangeType m_RangeType;
        [SerializeField] protected GameObject m_SkillPrefab;
        [SerializeField] protected GameObject m_DieEffectPrefab;
        [SerializeField] protected Transform m_Center;
        [SerializeField] protected string m_IdleAnimation = "Idle";
        [SerializeField] protected string m_AttackAnimation = "Attack";

        protected float m_EllipseHalfMajorAxis;
        protected float m_EllipseHalfMinorAxis;
        protected float m_SqrDistanceToTarget;
        protected float m_SqrRealAttackRange;
        protected SpriteRendererEffect m_SpriteEffect;
        protected Transform m_Target;
        protected Character m_TargetCharacter;
        protected DirectionTools m_DirectionTools;

        #region Public
        public int teamIndex
        {
            get { return m_TeamIndex; }
            set
            {
                CharacterManager.instance.Remove(this);
                m_TeamIndex = value;
                renderColor = (m_TeamIndex != 0);
                CharacterManager.instance.Add(this);
            }
        }

        public int maxHp
        {
            get { return m_MaxHp; }
            set { m_MaxHp = value; }
        }

        public int hp
        {
            get;
            set;
        }

        public int damage
        {
            get { return m_Damage; }
            set { m_Damage = value; }
        }

        public float attackRange
        {
            get { return m_AttackRange; }
            set { m_AttackRange = value; }
        }

        public Transform center
        {
            get { return m_Center; }
        }

        public State state
        {
            get;
            set;
        }

        public Transform target
        {
            get { return m_Target; }
            set
            {
                if (value != null)
                {
                    if (m_Target != value)
                    {
                        m_Target = value;
                        UpdateTargetInfo();
                    }
                }
                else
                {
                    m_Target = null;
                }
            }
        }

        public float sqrDistanceToTarget
        {
            get { return m_SqrDistanceToTarget; }
            set { m_SqrDistanceToTarget = value; }
        }

        public virtual float GetRadius()
        {
            return 0;
        }
        #endregion

        #region Protected
        protected override void Awake()
        {
            base.Awake();

            m_SpriteEffect = GetComponent<SpriteRendererEffect>();
            m_DirectionTools = new DirectionTools(maxDirection);
            m_EllipseHalfMajorAxis = m_ArmLength;
            m_EllipseHalfMinorAxis = m_ArmLength * ELLIPSE_RATIO;

            renderColor = (m_TeamIndex != 0);
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            m_Target = null;
            m_SqrRealAttackRange = 0;
            hp = maxHp;
            state = State.Idle;

            if (CharacterManager.instance == null)
            {
                new GameObject("Character Manager").AddComponent<CharacterManager>();
            }

            CharacterManager.instance.Add(this);
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            if (CharacterManager.instance != null)
            {
                CharacterManager.instance.Remove(this);
            }
        }

        public override void FixedUpdateMe()
        {
            base.FixedUpdateMe();

            if (target != null && m_TargetCharacter.state != State.Die)
            {
                m_SqrDistanceToTarget = (transform.position - target.position).sqrMagnitude;
            }
        }

        protected override void OnAnimationEvent(string animationName, int animationEventIndex)
        {
            if (animationName == m_AttackAnimation)
            {
                if (target != null)
                {
                    OnAttack();
                }
            }
        }

        protected virtual void OnAttack()
        {
            switch (m_RangeType)
            {
                case RangeType.Melee:
                    target.GetComponent<Character>().OnDamaged(damage);
                    break;

                case RangeType.RangeTarget:
                    InstantiateSkillTarget();
                    break;

                case RangeType.Area:
                    InstantiateSkillArea();
                    break;
            }
        }

        public virtual void OnDamaged(int damage)
        {
            m_SpriteEffect.Damage();

            hp -= damage;

            if (hp <= 0)
            {
                hp = 0;
                state = State.Die;

                CacheManager.Destroy(gameObject);

                GameObject dieFx = CacheManager.Instantiate(m_DieEffectPrefab);
                dieFx.transform.position = center.position;
            }
        }

        protected virtual void InstantiateSkillTarget()
        {
            GameObject go = CacheManager.Instantiate(m_SkillPrefab);

            go.transform.position = GetHandPositionFromDirection(m_Center.position);

            TargetSkill skill = go.GetComponent<TargetSkill>();
            skill.target = target.GetComponent<Character>();
            skill.damage = damage;
        }

        protected virtual void InstantiateSkillArea()
        {
            GameObject go = CacheManager.Instantiate(m_SkillPrefab);

            go.transform.position = GetHandPositionFromDirection(transform.position);

            AreaSkill skill = go.GetComponent<AreaSkill>();
            skill.teamIndex = teamIndex;
            skill.damage = damage;
        }

        protected virtual void FixedUpdateIdle()
        {
            Play(m_IdleAnimation, m_AttackAnimation);

            if (currentAnimation == m_IdleAnimation)
            {
                state = State.Idle;
                target = null;
            }
        }

        protected virtual void FixedUpdateAttack()
        {
            state = State.Attacking;
            LookAtTarget();
            Play(m_AttackAnimation);
        }

        protected virtual void UpdateTargetInfo()
        {
            if (target != null)
            {
                m_TargetCharacter = target.GetComponent<Character>();
                m_SqrRealAttackRange = Sqr(this.GetRadius() + m_TargetCharacter.GetRadius() + attackRange);
            }
        }

        protected bool InAttackRange()
        {
            return (m_SqrDistanceToTarget <= m_SqrRealAttackRange);
        }

        protected Vector3 GetHandPositionFromDirection(Vector3 pivot)
        {
            float a = m_EllipseHalfMajorAxis;
            float b = m_EllipseHalfMinorAxis;

            int degree = 90 - direction * 30;

            if (degree < 0)
            {
                degree += 360;
            }

            if (degree == 90)
            {
                return pivot + new Vector3(0, 0, b);
            }
            else if (degree == 270)
            {
                return pivot + new Vector3(0, 0, -b);
            }

            float radian = degree * Mathf.Deg2Rad;
            float tan = Mathf.Tan(radian);

            int signX = (degree > 90 && degree < 270) ? -1 : 1;
            int signY = (degree > 0 && degree < 180) ? 1 : -1;

            float x = signX * (a * b) / Mathf.Sqrt(b * b + a * a * (tan * tan));
            float y = signY * (a * b) / Mathf.Sqrt(a * a + b * b / (tan * tan));

            return pivot + new Vector3(x, 0, y);
        }

        protected int GetDirectionByXZ(float x, float z)
        {
            return m_DirectionTools.GetDirection(x, z);
        }

        protected int GetDirectionByAlpha(float alpha)
        {
            return m_DirectionTools.GetDirection(alpha);
        }

        protected void LookAtTarget()
        {
            direction = m_DirectionTools.GetDirection(transform.position, target.position);
        }

        protected float Sqr(float x)
        {
            return x * x;
        }
        #endregion

        #if UNITY_EDITOR
        [ContextMenu ("Idle")]
        protected void Idle0()
        {
            SetupDictionary();
            GetSpriteAnimator(m_IdleAnimation).SetDefaultSprite();
        }

        [ContextMenu ("Attack Right")]
        protected void AttackRight()
        {
            AttackDirection(maxDirection / 4);
        }

        [ContextMenu ("Attack Bottom")]
        protected void AttackBottom()
        {
            AttackDirection(maxDirection / 2);
        }

        protected void AttackDirection(int direction)
        {
            SetupDictionary();
            GetSpriteAnimator(m_AttackAnimation).SetDefaultAnimation(direction);
        }

        protected virtual void OnDrawGizmos()
        {
            if (!Application.isPlaying)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube(this.center.position, new Vector3(m_ArmLength * 2, 0, m_ArmLength * ELLIPSE_RATIO * 2));
            }
        }
        #endif
    }
}