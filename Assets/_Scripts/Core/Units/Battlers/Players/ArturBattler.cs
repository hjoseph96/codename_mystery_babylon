using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Sirenix.OdinInspector;

public class ArturBattler : Battler
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
        _tweenTarget = new Vector2(startingPoint.x - 0.927f, startingPoint.y);
        _isTweening = true;
    }

    private void ReturnToPlace()
    {
        _tweenSpeed *= 1.13f;
        _tweenTarget = startingPoint;
        _isTweening = true;
    }

}
