using UnityEngine;
using System.Collections;

namespace SS.TwoD
{
    public class SampleCustomController : MonoBehaviour
    {
        public Transform target;

        SpriteAnimatorController anim;
        DirectionTools dir;

        void Start()
        {
            anim = GetComponent<SpriteAnimatorController>();
            anim.onAnimationEvent += OnAnimationEvent;
            anim.direction = 1;
            anim.Play("Idle");

            dir = new DirectionTools(anim.maxDirection);
        }

        void OnDestroy()
        {
            anim.onAnimationEvent -= OnAnimationEvent;
        }

        void Update()
        {
            if (target != null)
            {
                anim.direction = dir.GetDirection(transform.position, target.position);
                transform.position = Vector3.Lerp(transform.position, target.position, anim.GetSpriteAnimator("Move").speed * Time.deltaTime);

                if (Vector3.Distance(transform.position, target.position) > 0.5f)
                {
                    anim.Play("Move");
                }
                else
                {
                    anim.Play("Attack");
                }
            }
            else
            {
                anim.Play("Idle");
            }
        }

        void OnGUI()
        {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Speed up"))
            {
                anim.GetSpriteAnimator("Attack").totalDuration = 0.25f;
                anim.GetSpriteAnimator("Move").speed = anim.GetSpriteAnimator("Attack").speed;
            }

            GUILayout.EndHorizontal();
        }

        public void OnAnimationEvent(string animationName, int animationEventIndex)
        {
            switch (animationName)
            {
                case "Attack":
                    Debug.Log(animationName + ": " + animationEventIndex);
                    break;
            }
        }
    }
}