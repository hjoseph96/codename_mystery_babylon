using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SS.TwoD
{
    public class DebugSpriteAnimatorController : SpriteAnimatorController
    {
        void Start()
        {
            Play("Idle");
        }

        void OnGUI()
        {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Idle"))
            {
                Play("Idle");
            }

            if (GUILayout.Button("Move"))
            {
                Play("Move");
            }

            if (GUILayout.Button("Attack"))
            {
                Play("Attack");
            }

            if (GUILayout.Button("Rotate"))
            {
                Rotate();
            }

            if (GUILayout.Button("<<"))
            {
                ChangeTotalDuration(0.25f);
            }

            if (GUILayout.Button(">>"))
            {
                ChangeTotalDuration(-0.25f);
            }

            GUILayout.EndHorizontal();
        }

        void Rotate()
        {
            if (direction < maxDirection - 1)
            {
                direction++;
            }
            else
            {
                direction = 0;
            }
        }

        void ChangeTotalDuration(float delta)
        {
            GetSpriteAnimator("Attack").totalDuration += delta;
            GetSpriteAnimator("Move").speed = GetSpriteAnimator("Attack").speed;
        }

        protected override void OnAnimationEvent(string animationName, int animationEventIndex)
        {
            switch (animationName)
            {
                case "Attack":
                    Debug.Log("Attack: " + Time.time);
                    break;
            }
        }
    }
}