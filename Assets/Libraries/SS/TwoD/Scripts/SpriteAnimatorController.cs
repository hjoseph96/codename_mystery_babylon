using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SS.TwoD
{
    public class SpriteAnimatorController : UpdateRegister
    {
        public delegate void OnAnimationEventDelegate(string animationName, int animationEventIndex);
        public OnAnimationEventDelegate onAnimationEvent;

        protected SpriteAnimator[] m_SpriteAnimators;
        protected Dictionary<string, int> m_SpriteAnimatorDictionary = new Dictionary<string, int>();

        int m_AnimatorIndex = -1;
        int m_Direction;

        public string currentAnimation
        {
            get
            {
                if (m_AnimatorIndex != -1)
                {
                    return m_SpriteAnimators[m_AnimatorIndex].animationName;
                }
                else
                {
                    return null;
                }
            }
        }

        public int direction
        {
            get { return m_Direction; }
            set
            {
                if (value > maxDirection - 1)
                    return;

                m_Direction = value;

                if (m_AnimatorIndex != -1)
                {
                    m_SpriteAnimators[m_AnimatorIndex].direction = value;
                }
            }
        }

        public int maxDirection
        {
            get
            {
                return m_SpriteAnimators[0].maxDirection;
            }
        }

        public bool renderColor
        {
            set
            {
                for (int i = 0; i < m_SpriteAnimators.Length; i++)
                {
                    m_SpriteAnimators[i].colorRenderer.gameObject.SetActive(value);
                }
            }
        }

        public SpriteAnimator GetSpriteAnimator(string animationName)
        {
            return m_SpriteAnimators[m_SpriteAnimatorDictionary[animationName]];
        }

        public void Play(int index, params string[] waitAnims)
        {
            if (m_AnimatorIndex != index)
            {
                bool needWait = false;
                
                for (int i = 0; i < waitAnims.Length; i++)
                {
                    if (currentAnimation == waitAnims[i])
                    {
                        needWait = true;
                        break;
                    }
                }
                
                if (!needWait)
                {
                    if (m_AnimatorIndex != -1)
                    {
                        m_SpriteAnimators[m_AnimatorIndex].Stop();
                    }
                    
                    m_AnimatorIndex = index;
                    m_SpriteAnimators[m_AnimatorIndex].direction = direction;
                    m_SpriteAnimators[m_AnimatorIndex].Play();
                }
                else
                {
                    if (m_AnimatorIndex != -1)
                    {
                        switch (m_SpriteAnimators[m_AnimatorIndex].state)
                        {
                            case SpriteAnimator.State.STOP:
                            case SpriteAnimator.State.PLAYING_DELAY:
                                m_SpriteAnimators[m_AnimatorIndex].Stop();
                                m_AnimatorIndex = index;
                                m_SpriteAnimators[m_AnimatorIndex].direction = direction;
                                m_SpriteAnimators[m_AnimatorIndex].Play();
                                break;
                                
                            case SpriteAnimator.State.PLAYING_FRAME:
                                m_SpriteAnimators[m_AnimatorIndex].internalLoop = false;
                                break;
                        }
                    }
                }
            }
            else
            {
                m_SpriteAnimators[m_AnimatorIndex].internalLoop = true;
            }
        }

        public void Play(string anim, params string[] waitAnims)
        {
            if (m_SpriteAnimatorDictionary.ContainsKey(anim))
            {
                int index = m_SpriteAnimatorDictionary[anim];
                Play(index, waitAnims);
            }
        }

        protected virtual void Awake()
        {
            SetupDictionary();

            for (int i = 0; i < m_SpriteAnimators.Length; i++)
            {
                m_SpriteAnimators[i].onAnimationEvent += OnAnimationEventBridge;
            }
        }

        protected virtual void OnDestroy()
        {
            for (int i = 0; i < m_SpriteAnimators.Length; i++)
            {
                if (m_SpriteAnimators[i] != null)
                {
                    m_SpriteAnimators[i].onAnimationEvent -= OnAnimationEventBridge;
                }
            }
        }

        public override void FixedUpdateMe()
        {
            
        }

        protected virtual void OnAnimationEvent(string animationName, int animationEventIndex)
        {
        }

        protected void SetupDictionary()
        {
            m_SpriteAnimators = GetComponents<SpriteAnimator>();

            for (int i = 0; i < m_SpriteAnimators.Length; i++)
            {
                if (!m_SpriteAnimatorDictionary.ContainsKey(m_SpriteAnimators[i].animationName))
                {
                    m_SpriteAnimatorDictionary.Add(m_SpriteAnimators[i].animationName, i);
                }
            }
        }

        void OnAnimationEventBridge(string animationName, int animationEventIndex)
        {
            if (onAnimationEvent != null)
            {
                onAnimationEvent(animationName, animationEventIndex);
            }

            OnAnimationEvent(animationName, animationEventIndex);
        }
    }
}