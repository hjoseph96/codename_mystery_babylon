using System;
using System.Collections.Generic;
using UnityEngine;

public class DetectAnimationEnd : MonoBehaviour
{
    [SerializeField] private string _animationName;
    [HideInInspector] public Action OnAnimationEnd;
    [HideInInspector] public Action OnHaltSecondaryEffect;

    public void Play()
    {
        GetComponent<Animator>().Play(_animationName);
    }

    // Called from AnimationEvent
    private void OnAnimationEnded() => OnAnimationEnd.Invoke();
    private void OnHaltedSceondaryEffect() => OnHaltSecondaryEffect.Invoke();
}
