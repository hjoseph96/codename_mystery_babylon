using System;
using System.Collections.Generic;
using UnityEngine;

using DarkTonic.MasterAudio;
using Sirenix.OdinInspector;

public class JumpController : MonoBehaviour
{
    [SerializeField] private float _JumpDuration = 1.6f;

    [HideInInspector] public Action OnBeginJump;
    [HideInInspector] public Action UponLanding;
    [HideInInspector] public Action WhileInAir;


    private JumpTrigger _jumpTrigger;
    private float       _startedJumpTime;
    private Vector2     _jumpDestination;
    private bool        _currentlyJumping       = false;
    private bool        _reachedJumpApex        = false;
    private bool        _spawnedLandingEffect   = false;

    public void Jump(JumpTrigger jumpTrigger)
    {
        _jumpTrigger        = jumpTrigger;
        _jumpDestination    = _jumpTrigger.Destination;

        if (OnBeginJump != null)
            OnBeginJump.Invoke();

        _startedJumpTime  = Time.time;
        _currentlyJumping = true;
    }


    private void HandleJumping()
    {
        MoveOnParabola();

        var currentPosition = (Vector2)transform.position;
        if (Vector2.Distance(currentPosition, _jumpTrigger.GetHighestPoint()) < 0.05f)
            _reachedJumpApex = true;

        var distanceToDestination = Vector2.Distance(currentPosition, _jumpDestination);
        if (distanceToDestination < 0.04f)
        {
            if (UponLanding != null)
                UponLanding.Invoke();

            if (!_spawnedLandingEffect)
            {
                MasterAudio.PlaySound3DFollowTransform(_jumpTrigger.LandingSound, CampaignManager.AudioListenerTransform);
                Instantiate(_jumpTrigger.LandingEffect, _jumpTrigger.Destination, Quaternion.identity, this.transform);
                _spawnedLandingEffect = true;
            }

            _currentlyJumping = false;
        }
        else
            if (WhileInAir != null)
                WhileInAir.Invoke();

    }

    private void MoveOnParabola()
    {
        float duration      = _reachedJumpApex ? _JumpDuration / 1.2f : _JumpDuration;

        float t = (Time.time - _startedJumpTime) / duration;
        var parabolaTime    = Mathf.SmoothStep(0, 1, t);
        
        transform.position  = _jumpTrigger.GetPositionAtTime(parabolaTime);
    }

    // Update is called once per frame
    void Update()
    {
        if (_currentlyJumping)
            HandleJumping();
    }
}
