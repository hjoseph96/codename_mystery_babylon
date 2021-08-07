using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Animancer;

public class ChristianAnimationSet : MonoBehaviour
{
    [Header("Christian As Knight")]
    public DirectionalAnimationSet _ScaredIdle;
    public DirectionalAnimationSet _Death;
    public DirectionalAnimationSet _Dead;
    public DirectionalAnimationSet _Walk;
    public DirectionalAnimationSet _HelmetDrop;
    public DirectionalAnimationSet _StayIncapacitated;

    [Header("Christian Disrobed")]
    public DirectionalAnimationSet _IdleDisrobed;
    public DirectionalAnimationSet _WalkDisrobed;
    public DirectionalAnimationSet _DeathDisrobed;
    public DirectionalAnimationSet _DeadDisrobed;
    public DirectionalAnimationSet _TiedToTree;


    public GameObject _helmetPrefab;
    public Transform HelmetSpawnPoint;


    private SpriteCharacterControllerExt _controller;

    private void Awake()
    {
        _controller = GetComponent<SpriteCharacterControllerExt>();
    }

    public void OverrideKnightAnimations()
    {
        _controller._Idle = _ScaredIdle;
        _controller._Death = _Death;
        _controller._Dead = _Dead;
        _controller._Walk = _Walk;
        _controller._HelmetDrop = _HelmetDrop;
        _controller.HelmetSpawnPoint = HelmetSpawnPoint;
        _controller._StayIncapacitated = _StayIncapacitated;
    }

    public void OverrideDisrobedAnimations()
    {
        _controller._Idle   = _IdleDisrobed;
        _controller._Walk   = _WalkDisrobed;
        _controller._Death  = _DeathDisrobed;
        _controller._Dead   = _DeadDisrobed;
    }
}
