using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Com.LuisPedroFonseca.ProCamera2D;

public class TalkToGatekeepersCutscene : Cutscene
{
    public static TalkToGatekeepersCutscene Instance;

    [SerializeField] private AnimatedDoor _Portcullis;

    private ProCamera2D _camera;
    private ProCamera2DTransitionsFX _cameraTransitions;

    private SpriteCharacterControllerExt _artur;


    public override void Init()
    {
        base.Init();

        Instance = this;

        var mainPlayer = GameObject.FindGameObjectWithTag("Main Player");
        
        _artur = mainPlayer.GetComponent<SpriteCharacterControllerExt>();
        _artur.GetComponent<ArturKnightAnimationSet>().OverrideToKnightAnimations();
        _artur.EnableCollider();

        _camera = Camera.main.GetComponent<ProCamera2D>();
        _cameraTransitions = _camera.GetComponent<ProCamera2DTransitionsFX>();

        _camera.SetSingleTarget(mainPlayer.transform);
    }

    void Update()
    {
        if (IsTriggered)
        {
            StartCoroutine(MoveIntoPosition());

            IsTriggered = false;
        }
    }

    public IEnumerator OpenThePortcullis()
    {
        var portcullisOpen = false;

        _Portcullis.OnDoorOpened += delegate () { portcullisOpen = true; };
        _Portcullis.Open();

        yield return new WaitUntil(() => portcullisOpen);

        _artur.AllowInput();
    }
    
    private IEnumerator MoveIntoPosition()
    {
        var arturPosition = new Vector2Int(64, 88);

        yield return _artur.WalkToCoroutine(arturPosition);
        
        _artur.Rotate(Direction.Up);
        _artur.SetIdle();

        Play();
    }
}
