using UnityEngine;
using System.Collections;
using SS.TwoD;

public class Sample2dInputController : MonoBehaviour
{
    enum Direction
    {
        RIGHT = 1,
        LEFT = 3
    }

    SpriteAnimatorController anim;
    bool pressButton;

	void Start ()
    {
        anim = GetComponent<SpriteAnimatorController>();
        anim.direction = (int)Direction.RIGHT;
	}

	void Update ()
    {
        if (!pressButton)
        {
            anim.Play("Idle", "Attack");
        }
	}

    void OnGUI()
    {
        GUILayout.BeginHorizontal();

        pressButton = false;

        if (GUILayout.RepeatButton("Attack"))
        {
            pressButton = true;
            anim.Play("Attack");
        }

        if (GUILayout.RepeatButton("<<"))
        {
            pressButton = true;
            anim.direction = (int)Direction.LEFT;
            anim.Play("Move", "Attack");
            transform.Translate(-Time.deltaTime, 0, 0);
        }

        if (GUILayout.RepeatButton(">>"))
        {
            pressButton = true;
            anim.direction = (int)Direction.RIGHT;
            anim.Play("Move", "Attack");
            transform.Translate(Time.deltaTime, 0, 0);
        }

        GUILayout.EndHorizontal();
    }
}
