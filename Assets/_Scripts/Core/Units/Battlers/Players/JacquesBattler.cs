using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Sirenix.OdinInspector;

public class JacquesBattler : Battler
{
    [SerializeField] float _tweenSpeed;
    [ReadOnly] private bool _isTweening = false;
    [ReadOnly] private Vector2 _tweenTarget;

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        if (_isTweening)
        {
            transform.position = Vector2.Lerp(transform.position, _tweenTarget, _tweenSpeed * Time.deltaTime);
            var distance = Vector2.Distance(transform.position, _tweenTarget);
            if (distance <= 0.3)
            {
                transform.position = _tweenTarget;
                _isTweening = false;
            }
        }
    }

    private void BeginTweening()
    {
        var animState = Animator.GetCurrentAnimatorStateInfo(0);
        if (animState.IsName("Attack"))
            _tweenTarget = new Vector2(startingPoint.x - 0.927f, startingPoint.y);
        else if (animState.IsName("Critical Attack"))
            _tweenTarget = new Vector2(startingPoint.x - 0.7f, startingPoint.y + 1.3f);

        _isTweening = true;
    }

    private void ReturnToPlace()
    {
        _tweenSpeed *= 1.13f;
        _tweenTarget = startingPoint;
        _isTweening = true;
    }

}
