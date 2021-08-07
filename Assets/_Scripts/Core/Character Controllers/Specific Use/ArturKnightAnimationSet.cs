
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Animancer;

public class ArturKnightAnimationSet : MonoBehaviour
{
    [Header("Artur As Knight")]
    public DirectionalAnimationSet _Idle;
    public DirectionalAnimationSet _Walk;
    public DirectionalAnimationSet _Run;

    private SpriteCharacterControllerExt _controller;

    private void Awake()
    {
        _controller = GetComponent<SpriteCharacterControllerExt>();
    }


    public void OverrideToKnightAnimations()
    {
        _controller._Idle  = _Idle;
        _controller._Walk  = _Walk;
        _controller._Run = _Run;
    }
}
