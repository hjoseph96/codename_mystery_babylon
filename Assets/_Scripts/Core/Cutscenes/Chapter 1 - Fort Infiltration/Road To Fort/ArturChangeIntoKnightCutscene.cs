using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Com.LuisPedroFonseca.ProCamera2D;

public class ArturChangeIntoKnightCutscene : Cutscene
{
    public static ArturChangeIntoKnightCutscene Instance;
    
    public SpriteCharacterControllerExt _Artur;

    public override void Init()
    {
        base.Init();

        Instance = this;

        DeactivateTriggers();
    }

    // Update is called once per frame
    void Update()
    {
        if (IsTriggered)
        {
            StartCoroutine(WalkToCenter());

            IsTriggered = false;
        }
    }

    private IEnumerator WalkToCenter()
    {
        var center = new Vector2Int(14, 40);
        
        yield return _Artur.WalkToCoroutine(center);

        DialogueManager.Instance.OnDialogueComplete += delegate () { _Artur.AllowInput(); };

        Play();
    }

    public IEnumerator ArturChangeIntoKnight()
    {
        var changingPoint = new Vector2Int(31, 40);

        yield return _Artur.WalkToCoroutine(changingPoint);

        var camera = Camera.main.GetComponent<ProCamera2D>();
        var cameraTransitions = camera.GetComponent<ProCamera2DTransitionsFX>();

        var fadedOut = false;
        cameraTransitions.OnTransitionExitEnded = null;
        cameraTransitions.OnTransitionExitEnded += delegate ()
        {
            fadedOut = true;

            var knightAnimset = _Artur.GetComponent<ArturKnightAnimationSet>();
            knightAnimset.OverrideToKnightAnimations();
        };

        cameraTransitions.TransitionExit();

        yield return new WaitUntil(() => fadedOut);

        yield return new WaitForSeconds(1.5f);

        cameraTransitions.TransitionEnter();

        var returnPoint = new Vector2Int(14, 40);

        yield return _Artur.WalkToCoroutine(returnPoint);
    }
}
