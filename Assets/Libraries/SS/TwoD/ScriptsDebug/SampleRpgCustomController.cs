using UnityEngine;
using System.Collections;

namespace SS.TwoD
{
    public class SampleRpgCustomController : MonoBehaviour
    {
        const float MOVE_SPEED = 0.5f;
        const float ROTATE_SPEED = 7f;

        SpriteAnimatorController anim;
        DirectionTools dir;
        Vector3 target;
        Transform lookAtTarget;

        void Start()
        {
            anim = GetComponent<SpriteAnimatorController>();
            anim.Play("Idle");

            dir = new DirectionTools(anim.maxDirection);

            lookAtTarget = new GameObject("LookAtTarget").transform;
            lookAtTarget.SetParent(transform);
            lookAtTarget.localPosition = Vector3.zero;
        }

        void Update()
        {
            MouseInput();
            Move();
            Rotate();
        }

        void MouseInput()
        {
            if (Input.GetMouseButton(0))
            {
                target = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                target.y = 0;
            }
        }

        void Move()
        {
            Vector3 ab = (target - transform.position);
            float sqrDistance = ab.sqrMagnitude;

            Vector3 ac = ab.normalized * Time.deltaTime * MOVE_SPEED;

            if (ac.sqrMagnitude < sqrDistance)
            {
                transform.position += ac;
            }
            else
            {
                transform.position += ab;
            }
        }

        void Rotate()
        {
            if (Vector3.Distance(transform.position, target) > 0.1f)
            {
                anim.Play("Move");

                // Rotate lookAtTarget to the target smoothly
                Quaternion targetRotation = Quaternion.LookRotation(target - transform.position, Vector3.up);
                lookAtTarget.rotation = Quaternion.Slerp(lookAtTarget.rotation, targetRotation, Time.deltaTime * ROTATE_SPEED);

                // Set animation direction
                anim.direction = dir.GetDirection(90 - lookAtTarget.eulerAngles.y);
            }
            else
            {
                anim.Play("Idle");
            }
        }
    }
}