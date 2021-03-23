using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Com.LuisPedroFonseca.ProCamera2D;
using DarkTonic.MasterAudio;
using DG.Tweening;
using TMPro;

public class StartScreen : MonoBehaviour, IInitializable
{
    [SerializeField] private ProCamera2D _uiCamera;
    [SerializeField, SoundGroupAttribute] private string startMenuMusic;
    [SerializeField] private TextMeshProUGUI _pressAnyKeyText;
    
    private ProCamera2DTransitionsFX _cameraTransitions;

    private Transform AudioListenerTransform => _uiCamera.transform;


    // Start is called before the first frame update
    public void Init()
    {
        _cameraTransitions = _uiCamera.GetComponent<ProCamera2DTransitionsFX>();
    }

    void Start()
    {
        _cameraTransitions.OnTransitionEnterStarted += delegate ()
        {
            StartCoroutine(PlayMusic());
        };
        _cameraTransitions.TransitionEnter();
        _pressAnyKeyText.DOFade(1f, 1.3f).SetLoops(-1, LoopType.Yoyo);
    }


    IEnumerator PlayMusic()
    {
        yield return new WaitForSecondsRealtime(1f);
        MasterAudio.PlaySound3DAtTransform(startMenuMusic, AudioListenerTransform);
    }
}
