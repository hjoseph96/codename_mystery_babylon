using System;
using System.Collections.Generic;
using UnityEngine;

using Com.LuisPedroFonseca.ProCamera2D;

public class ArkCinematicCamera : MonoBehaviour, IInitializable
{
    public static ArkCinematicCamera Instance;

    private ProCamera2DTransitionsFX _cameraTransitions;

    [HideInInspector] public Action OnCameraFadedIn;
    [HideInInspector] public Action OnCameraFadedOut;

    public void Init()
    {
        Instance = this;

        _cameraTransitions = GetComponent<ProCamera2DTransitionsFX>();
    }

    public void FadeIn()
    {
        _cameraTransitions.OnTransitionEnterStarted += delegate ()
        {
            OnCameraFadedIn?.Invoke();
            OnCameraFadedIn = null;
        };
        _cameraTransitions.TransitionEnter();
    }

    public void FadeOut()
    {
        _cameraTransitions.OnTransitionExitEnded += delegate ()
        {
            OnCameraFadedOut?.Invoke();
            OnCameraFadedOut = null;
        };
        _cameraTransitions.TransitionExit();
    }
}
