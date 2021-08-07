using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Animancer;

public class CarriageDriverController : SpriteCharacterControllerExt
{
    public DirectionalAnimationSet _Whistling;

    private bool _isWhistling = false;

    protected override void Start()
    {
        base.Start();
    }

    protected override void Awake()
    {
        base.Awake();
    }

    public void StartWhistling() => _isWhistling = true;

    protected override void HandleAutoMovement()
    {

        if (_autoMovePath == null || _autoMovePath.Length == 0)
        {
            if (!_spawnedHelmet && _isIncapacitated) return;

            if (IsSeated)
                Play(_Sitting);
            else if (_isWhistling)
                Play(_Whistling);
            else if (IsIncapacitated)
                Play(_StayIncapacitated);
            else if (_isRestrained)
                PlayRestrainedAnim();
            else if (_isDying)
                return;
            else if (IsDead)
                Play(_Dead);
            else
            {
                Play(_Idle);
            }

            if (_CurrentAnimationSet == _Whistling)
                _isWhistling = true;
            else
                _isWhistling = false;
        }
    }

}
