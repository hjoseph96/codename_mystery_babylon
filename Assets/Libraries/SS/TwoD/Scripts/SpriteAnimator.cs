using UnityEngine;
using System.Collections;

namespace SS.TwoD
{
    [System.Serializable]
    public class SpriteDirection : System.Object
    {
        public Sprite[] body;
        public Sprite[] shadows;
        public Sprite[] colors;
    }

    public class SpriteAnimator : UpdateRegister
    {
        public delegate void OnAnimationEventDelegate(string animationName, int animationEventIndex);
        public OnAnimationEventDelegate onAnimationEvent;

        public enum State
        {
            STOP,
            PLAYING_FRAME,
            PLAYING_DELAY
        }

        [SerializeField] string m_AnimationName;
        [SerializeField] string m_IdleAnimationName;
        [SerializeField] float m_AnimationDuration;
        [SerializeField] float m_NextAnimationDelay;
        [SerializeField] int[] m_AnimationFrameEvents;
        [SerializeField] float[] m_AnimationTimeEvents;
        [SerializeField] SpriteRenderer m_BodyRenderer;
        [SerializeField] SpriteRenderer m_ShadowRenderer;
        [SerializeField] SpriteRenderer m_ColorRenderer;

        public SpriteDirection[] directionSprites;

        public string animationName
        {
            get { return m_AnimationName; }
            set { m_AnimationName = value; }
        }

        public float animationDuration
        {
            get { return m_AnimationDuration; }
            set { m_AnimationDuration = value; }
        }

        public int[] animationFrameEvents
        {
            get { return m_AnimationFrameEvents; }
        }

        public SpriteRenderer bodyRenderer
        {
            get { return m_BodyRenderer; }
            set { m_BodyRenderer = value; }
        }

        public SpriteRenderer shadowRenderer
        {
            get { return m_ShadowRenderer; }
            set { m_ShadowRenderer = value; }
        }

        public SpriteRenderer colorRenderer
        {
            get { return m_ColorRenderer; }
            set { m_ColorRenderer = value; }
        }

        public State state
        {
            get;
            protected set;
        }

        public float remainAnimationTime
        {
            get
            {
                return (totalDuration - (m_SpriteCounter * m_NextRepeatRate + m_TimeNext));
            }
        }

        /// <summary>
        /// Don't try to set this value. It's internal.
        /// </summary>
        /// <value>The internal loop.</value>
        public bool internalLoop
        {
            get;
            set;
        }

        public float speed
        {
            get
            {
                return m_Speed;
            }

            set
            {
                if (value < 0)
                    return;

                float prevTotalDuration = m_TotalDuration;

                m_Speed = value;
                m_TotalDuration = (m_AnimationDuration + m_NextAnimationDelay) / m_Speed;
                UpdateTimeCommon(prevTotalDuration);
            }
        }

        public float totalDuration
        {
            get
            {
                return m_TotalDuration;
            }
            set
            {
                if (value <= 0)
                {
                    return;
                }

                float prevTotalDuration = m_TotalDuration;

                m_TotalDuration = value;
                m_Speed = (m_AnimationDuration + m_NextAnimationDelay) / m_TotalDuration;
                UpdateTimeCommon(prevTotalDuration);
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

                if (m_SpriteMax == 1)
                {
                    SetSprite(direction, 0);
                }
            }
        }

        public int maxDirection
        {
            get
            {
                return directionSprites.Length;
            }
        }

        public void Play()
        {
            if (state == State.STOP)
            {
                state = State.PLAYING_FRAME;

                if ((m_AnimationDuration + m_NextAnimationDelay) > 0)
                {
                    m_TimeTotal = 0;
                    internalLoop = true;
                    PlayLoop();
                }
                else
                {
                    SetSprite(direction, 0);
                }
            }
        }

        public void SetSprite(Sprite body, Sprite shadow, Sprite color)
        {
            if (body != null)
            {
                m_BodyRenderer.sprite = body;
            }

            if (shadow != null)
            {
                m_ShadowRenderer.sprite = shadow;
            }

            if (color != null)
            {
                m_ColorRenderer.sprite = color;
            }
        }

        public void SetSprite(int direction, int frame)
        {
            SetSprite(directionSprites[direction].body[frame], directionSprites[direction].shadows[frame], directionSprites[direction].colors[frame]);
        }

        public void Stop()
        {
            m_SpriteCounter = 0;

            state = State.STOP;
        }

        public void SetDefaultSprite()
        {
            for (int i = 0; i < maxDirection; i++)
            {
                if (directionSprites[i].body.Length > 0 && directionSprites[i].body[0] != null)
                {
                    SetSprite(i, 0);
                    break;
                }
            }
        }

        public void SetDefaultAnimation(int direction)
        {
            if (animationFrameEvents.Length > 0)
            {
                SetSprite(direction, animationFrameEvents[0]);
            }
            else
            {
                SetSprite(direction, 0);
            }
        }

        public Sprite GetBodySprite(int direction, int index)
        {
            return directionSprites[direction].body[index];
        }

        public Sprite GetShadowSprite(int direction, int index)
        {
            return directionSprites[direction].shadows[index];
        }

        public Sprite GetColorSprite(int direction, int index)
        {
            return directionSprites[direction].colors[index];
        }

        int m_Direction;
        int m_SpriteCounter;
        int m_SpriteMax;
        int m_TimeEventIndex;
        float m_TotalDuration;
        float m_NextRepeatRate;
        float m_TimeTotal;
        float m_TimeNext;
        float m_Speed = 1f;
        bool m_IsNext;

        SpriteAnimator m_IdleAnimator;

        void Awake()
        {
            m_SpriteMax = directionSprites[0].body.Length;
            m_TotalDuration = (m_AnimationDuration + m_NextAnimationDelay) / m_Speed;
            m_NextRepeatRate = (m_AnimationDuration / m_Speed) / m_SpriteMax;

            if (!string.IsNullOrEmpty(m_IdleAnimationName))
            {
                SpriteAnimator[] spriteAnimators = GetComponents<SpriteAnimator>();

                for (int i = 0; i < spriteAnimators.Length; i++)
                {
                    if (spriteAnimators[i].animationName == m_IdleAnimationName)
                    {
                        m_IdleAnimator = spriteAnimators[i];
                        break;
                    }
                }
            }
        }

        public override void FixedUpdateMe()
        {
            if (state == State.PLAYING_FRAME || state == State.PLAYING_DELAY)
            {
                if (internalLoop)
                {
                    m_TimeTotal += Time.fixedDeltaTime;

                    if (m_TimeTotal >= m_TotalDuration)
                    {
                        m_TimeTotal = (m_TimeTotal - m_TotalDuration);
                        PlayLoop();
                    }
                }

                if (m_IsNext)
                {
                    m_TimeNext += Time.fixedDeltaTime;

                    if (m_TimeNext >= m_NextRepeatRate)
                    {
                        m_TimeNext = (m_TimeNext - m_NextRepeatRate);
                        Next();
                    }
                }

                for (int i = 0; i < m_AnimationTimeEvents.Length; i++)
                {
                    if (m_TimeEventIndex < i && m_TimeTotal >= m_AnimationTimeEvents[i])
                    {
                        m_TimeEventIndex = i;
                        if (onAnimationEvent != null)
                        {
                            onAnimationEvent(this.animationName, i);
                        }
                    }
                }
            }
        }

        void PlayLoop()
        {
            state = State.PLAYING_FRAME;
            m_IsNext = true;
            m_TimeNext = 0;
            m_TimeEventIndex = -1;
            Next();
        }

        void Next()
        {
            int maxOffset = (m_NextAnimationDelay == 0) ? 0 : 1;

            if (m_SpriteCounter < directionSprites[direction].body.Length)
            {
                SetSprite(direction, m_SpriteCounter);
            }
            else
            {
                if (m_IdleAnimator != null)
                {
                    SetSprite(m_IdleAnimator.GetBodySprite(direction, 0), m_IdleAnimator.GetShadowSprite(direction, 0), m_IdleAnimator.GetColorSprite(direction, 0));
                }
                else
                {
                    SetSprite(direction, 0);
                }
            }

            for (int i = 0; i < m_AnimationFrameEvents.Length; i++)
            {
                if (m_AnimationFrameEvents[i] == m_SpriteCounter)
                {
                    if (onAnimationEvent != null)
                    {
                        onAnimationEvent(this.animationName, i);
                    }
                    break;
                }
            }

            m_SpriteCounter++;

            if (m_SpriteCounter >= m_SpriteMax + maxOffset)
            {
                m_SpriteCounter = 0;
                m_IsNext = false;

                if (m_NextAnimationDelay > 0)
                {
                    state = State.PLAYING_DELAY;
                }

                if (!internalLoop)
                {
                    if (m_NextAnimationDelay == 0)
                    {
                        state = State.STOP;
                    }
                }
            }
        }

        void UpdateTimeCommon(float prevTotalDuration)
        {
            m_NextRepeatRate = (m_AnimationDuration / m_Speed) / m_SpriteMax;
            m_TimeNext = m_TimeNext * m_TotalDuration / prevTotalDuration;
            m_TimeTotal = m_TimeTotal * m_TotalDuration / prevTotalDuration;
        }
    }
}