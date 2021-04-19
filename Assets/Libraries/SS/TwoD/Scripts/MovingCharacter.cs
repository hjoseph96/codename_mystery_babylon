using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_5_5_OR_NEWER
using UnityEngine.AI;
#endif

namespace SS.TwoD
{
    public class MovingCharacter : Character
    {
        public enum TrafficJamState
        {
            GOOD,
            TRAFFIC_JAM,
            TRAFFIC_JAM_RESOLVED
        }
        
        [SerializeField] protected string m_MoveAnimation = "Move";
        [Range(1,10)] [SerializeField] protected int m_Weight = 5;
        
        float m_TrafficJamTime;
        NavMeshAgent m_NavAgent;
        
        public TrafficJamState trafficJamState
        {
            get;
            set;
        }
        
        public int offsetPriority
        {
            get;
            set;
        }
        
        protected override void Awake()
        {
            base.Awake();
            
            m_NavAgent = GetComponent<NavMeshAgent>();
        }
        
        protected override void OnEnable()
        {
            base.OnEnable();
            
            m_TrafficJamTime = 0;
            m_TargetCharacter = null;
            
            trafficJamState = TrafficJamState.GOOD;
            offsetPriority = 0;
            
            if (TrafficJamResolver.instance == null)
            {
                new GameObject("TrafficJam Resolver").AddComponent<TrafficJamResolver>();
            }
            
            TrafficJamResolver.instance.Add(this);
        }
        
        protected override void OnDisable()
        {
            base.OnDisable();
            
            m_NavAgent.enabled = false;
            
            if (TrafficJamResolver.instance != null)
            {
                TrafficJamResolver.instance.Remove(this);
            }
        }
        
        public override void FixedUpdateMe()
        {
            base.FixedUpdateMe();
            
            if (target != null && m_TargetCharacter.state != State.Die)
            {
                if (InAttackRange())
                {
                    FixedUpdateAttack();
                }
                else
                {
                    FixedUpdateMove();
                }
            }
            else
            {
                FixedUpdateIdle();
            }
        }
        
        protected override void FixedUpdateIdle()
        {
            base.FixedUpdateIdle();
            
            if (currentAnimation == m_IdleAnimation)
            {
                if (m_NavAgent.enabled)
                {
                    StopNavAgent(true);
                }
                m_TrafficJamTime = 0;
            }
        }
        
        protected override void FixedUpdateAttack()
        {
            base.FixedUpdateAttack();
            
            m_TrafficJamTime = 0;
            m_NavAgent.avoidancePriority = (10 - m_Weight + 50);
            StopNavAgent(true);
        }
        
        protected virtual void FixedUpdateMove()
        {
            Play(m_MoveAnimation, m_AttackAnimation);
            
            if (currentAnimation == m_MoveAnimation)
            {
                state = State.Moving;
                
                StopNavAgent(false);
                m_NavAgent.avoidancePriority = (10 - m_Weight) + offsetPriority;
                m_NavAgent.destination = target.position;
                
                float velocity = m_NavAgent.velocity.magnitude;
                if (velocity < m_NavAgent.speed / 5)
                {
                    CheckTrafficJam();
                }
                else
                {
                    CheckTrafficJamResolve();
                }
                
                if (m_NavAgent.hasPath && velocity > m_NavAgent.speed / 5)
                {
                    LookByVelocity();
                }
            }
        }
        
        public override float GetRadius()
        {
            return m_NavAgent.radius;
        }
        
        protected void LookByVelocity()
        {
            float x = m_NavAgent.velocity.x;
            float z = m_NavAgent.velocity.z;
            
            direction = GetDirectionByXZ(x, z);
        }
        
        protected override void UpdateTargetInfo()
        {
            base.UpdateTargetInfo();
            
            if (target != null)
            {
                m_NavAgent.stoppingDistance = this.GetRadius() + m_TargetCharacter.GetRadius() + attackRange;
                m_NavAgent.enabled = true;
                m_NavAgent.destination = target.position;
            }
        }
        
        void CheckTrafficJam()
        {
            if (trafficJamState == TrafficJamState.GOOD)
            {
                m_TrafficJamTime += Time.fixedDeltaTime;
                
                if (m_TrafficJamTime >= 0.5f)
                {
                    trafficJamState = TrafficJamState.TRAFFIC_JAM;
                    m_TrafficJamTime = 0;
                }
            }
        }
        
        void CheckTrafficJamResolve()
        {
            if (trafficJamState == TrafficJamState.TRAFFIC_JAM_RESOLVED)
            {
                m_TrafficJamTime += Time.fixedDeltaTime;
                
                if (m_TrafficJamTime >= 1f)
                {
                    trafficJamState = TrafficJamState.GOOD;
                    m_TrafficJamTime = 0;
                    offsetPriority = 0;
                }
            }
        }
        
        void StopNavAgent(bool isStopped)
        {
            #if UNITY_5_6_OR_NEWER
            m_NavAgent.isStopped = isStopped;
            #else
            if (isStopped)
            {
                m_NavAgent.Stop();
            }
            else
            {
                m_NavAgent.Resume();
            }
            #endif
        }
    }
}