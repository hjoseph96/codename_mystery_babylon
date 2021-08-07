using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Com.LuisPedroFonseca.ProCamera2D;

public class FortTourCutscene : Cutscene
{
    public static FortTourCutscene Instance;

    [SerializeField] private SpriteCharacterControllerExt _TourGuideKnight;
    [SerializeField] private SpriteCharacterControllerExt _Artur;
    [SerializeField] private BuddyController _ArturFollowController;
    [SerializeField] private AnimatedDoor _bedroomDoor;
    [SerializeField] private AnimatedDoor _armoryDoor;

    private ProCamera2D _camera2D;
    private ProCamera2DTransitionsFX _cameraFX;

    public override void Init()
    {
        base.Init();

        Instance = this;
    }

    public IEnumerator StartFortTour()
    {
        var exitPoint = new Vector2Int(251, 66);

        var stageSet = false;

        _Artur.FreezeInput();

        _ArturFollowController = _Artur.GetComponent<BuddyController>();


        PlayerTeamController.Instance.Leader = _TourGuideKnight;
        PlayerTeamController.Instance.Init();
        PlayerTeamController.Instance.TeamMates.Add(_ArturFollowController);


        StartCoroutine(_TourGuideKnight.WalkToCoroutine(exitPoint));

        yield return new WaitForSecondsRealtime(1f);

        _Artur.DisableCollider();

        _ArturFollowController.OnFollowComplete += delegate ()
        {
            TransitionToHallway(delegate() { stageSet = true; });
        };
        _ArturFollowController.DeterminePosition();
        _ArturFollowController.StartFollowing(_TourGuideKnight.gameObject, 0);


        yield return new WaitUntil(() => stageSet);
    }

    private void TransitionToHallway(Action onTransitionEnter)
    {
        _camera2D = Camera.main.GetComponent<ProCamera2D>();
        _cameraFX = _camera2D.GetComponent<ProCamera2DTransitionsFX>();

        var worldGrid = WorldGrid.Instance;
        _cameraFX.OnTransitionExitEnded += delegate ()
        {
            var arturPos = new Vector2Int(205, 65);
            var tourGuidePos = new Vector2Int(203, 65);

            worldGrid.PlaceGameObject(_TourGuideKnight.gameObject, tourGuidePos);
            worldGrid.PlaceGameObject(_Artur.gameObject, arturPos);

            _TourGuideKnight.Rotate(Direction.Left);
            _TourGuideKnight.SetIdle();

            _ArturFollowController.StopFollowing();
            _ArturFollowController.Rotate(Direction.Left);
            _ArturFollowController.SetIdle();

            _cameraFX.OnTransitionEnterEnded += onTransitionEnter;
            _cameraFX.TransitionEnter();
        };

        _cameraFX.TransitionExit();
    }

    public IEnumerator WalkToCommandersRoom()
    {
        var tourGuidePos = new Vector2Int(148, 65);
        var arturPos = new Vector2Int(150, 65);

        StartCoroutine(_TourGuideKnight.WalkToCoroutine(tourGuidePos));

        yield return new WaitForSecondsRealtime(0.3f);

        var sequenceComplete = false;

        _ArturFollowController.OnFollowComplete += delegate ()
        {
            _ArturFollowController.StopFollowing();

            _TourGuideKnight.Rotate(Direction.Down);
            _TourGuideKnight.SetIdle();

            _ArturFollowController.Rotate(Direction.Down);
            _ArturFollowController.SetIdle();

            sequenceComplete = true;
        };

        _ArturFollowController.StartFollowing(_TourGuideKnight.gameObject, 0);

        yield return new WaitUntil(() => sequenceComplete);
    }

    public IEnumerator HeadToTheDungeonEntrance()
    {
        var tourGuidePos = new Vector2Int(143, 66);

        StartCoroutine(_TourGuideKnight.WalkToCoroutine(tourGuidePos));

        var transitionedToDungeonHallway = false;

        _ArturFollowController.OnFollowComplete += delegate ()
        {
            _ArturFollowController.StopFollowing();

            _ArturFollowController.Rotate(Direction.Left);
            _ArturFollowController.SetIdle();

            var worldGrid = WorldGrid.Instance;

            _cameraFX.OnTransitionExitEnded += delegate ()
            {
                tourGuidePos = new Vector2Int(102, 65);
                var arturPos = new Vector2Int(104, 65);

                worldGrid.PlaceGameObject(_TourGuideKnight.gameObject, tourGuidePos);
                worldGrid.PlaceGameObject(_Artur.gameObject, arturPos);


                _cameraFX.TransitionEnter();
            };

            _cameraFX.OnTransitionEnterEnded += delegate ()
            {
                transitionedToDungeonHallway = true;
            };

            _cameraFX.TransitionExit();

        };

        yield return new WaitForSecondsRealtime(0.4f);


        _ArturFollowController.StartFollowing(_TourGuideKnight.gameObject, 0);

        yield return new WaitUntil(() => transitionedToDungeonHallway);
        
        tourGuidePos = new Vector2Int(85, 65);
                
        StartCoroutine(_TourGuideKnight.WalkToCoroutine(tourGuidePos));

        yield return new WaitForSecondsRealtime(0.8f);

        var stoppedMoving = false;

        _ArturFollowController.OnFollowComplete += delegate () { 
            _ArturFollowController.StopFollowing();

            _ArturFollowController.Rotate(Direction.Left);
            _ArturFollowController.SetIdle();


            stoppedMoving = true;
        };

        _ArturFollowController.StartFollowing(_TourGuideKnight.gameObject, 0);

        yield return new WaitUntil(() => stoppedMoving);
    }

    public IEnumerator BackToTheMessHall()
    {
        var tourGuidePos = new Vector2Int(107, 66);
        var arturPos = new Vector2Int(147, 66);

        StartCoroutine(_TourGuideKnight.WalkToCoroutine(tourGuidePos));

        yield return new WaitForSecondsRealtime(1.5f);

        var reachedMainHallway = false;

        var worldGrid = WorldGrid.Instance;
        _ArturFollowController.OnFollowComplete += delegate ()
        {
            _ArturFollowController.StopFollowing();

            _cameraFX.OnTransitionExitEnded += delegate ()
            {
                tourGuidePos = new Vector2Int(149, 66);

                worldGrid.PlaceGameObject(_TourGuideKnight.gameObject, tourGuidePos);
                worldGrid.PlaceGameObject(_ArturFollowController.gameObject, arturPos);

                _ArturFollowController.Rotate(Direction.Right);
                _ArturFollowController.SetIdle();

                _cameraFX.TransitionEnter();
            };

            _cameraFX.OnTransitionEnterEnded += delegate ()
            {
                reachedMainHallway = true;
            };

            _cameraFX.TransitionExit();
        };

        _ArturFollowController.StartFollowing(_TourGuideKnight.gameObject, 0);

        yield return new WaitUntil(() => reachedMainHallway);

        
        tourGuidePos = new Vector2Int(208, 66);
        StartCoroutine(_TourGuideKnight.WalkToCoroutine(tourGuidePos));

        yield return new WaitForSecondsRealtime(0.5f);

        var reachedMessHall = false;
        
        _ArturFollowController.OnFollowComplete += delegate ()
        {
            _ArturFollowController.StopFollowing();

            _cameraFX.OnTransitionExitEnded += delegate ()
            {
                tourGuidePos = new Vector2Int(255, 66);
                arturPos = new Vector2Int(253, 66);

                worldGrid.PlaceGameObject(_TourGuideKnight.gameObject, tourGuidePos);
                worldGrid.PlaceGameObject(_ArturFollowController.gameObject, arturPos);


                _ArturFollowController.Rotate(Direction.Right);
                _ArturFollowController.SetIdle();

                _cameraFX.TransitionEnter();
            };

            _cameraFX.OnTransitionEnterEnded += delegate ()
            {
                reachedMessHall = true;
            };

            _cameraFX.TransitionExit();
        };

        _ArturFollowController.StartFollowing(_TourGuideKnight.gameObject, 0);

        yield return new WaitUntil(() => reachedMessHall);

        tourGuidePos = new Vector2Int(284, 67);
        StartCoroutine(_TourGuideKnight.WalkToCoroutine(tourGuidePos));

        yield return new WaitForSecondsRealtime(0.6f);

        var reachedStairsToBarracks = false;

        _ArturFollowController.OnFollowComplete += delegate ()
        {
            _ArturFollowController.StopFollowing();


            _TourGuideKnight.Rotate(Direction.Right);
            _TourGuideKnight.SetIdle();

            _ArturFollowController.Rotate(Direction.Right);
            _ArturFollowController.SetIdle();

            reachedStairsToBarracks = true;
        };

        _ArturFollowController.StartFollowing(_TourGuideKnight.gameObject, 0);

        yield return new WaitUntil(() => reachedStairsToBarracks);
    }

    public IEnumerator GoUpToBarracks()
    {
        var tourGuidePos = new Vector2Int(287, 69);
        var arturPos = new Vector2Int(322, 69);

        StartCoroutine(_TourGuideKnight.WalkToCoroutine(tourGuidePos));

        yield return new WaitForSecondsRealtime(0.6f);

        var worldGrid = WorldGrid.Instance;

        var reachedBarracks = false;

        _ArturFollowController.OnFollowComplete += delegate ()
        {
            _ArturFollowController.StopFollowing();

            _cameraFX.OnTransitionExitEnded += delegate ()
            {
                tourGuidePos = new Vector2Int(324, 69);

                worldGrid.PlaceGameObject(_TourGuideKnight.gameObject, tourGuidePos);
                worldGrid.PlaceGameObject(_ArturFollowController.gameObject, arturPos);


                _ArturFollowController.Rotate(Direction.Right);
                _ArturFollowController.SetIdle();

                _cameraFX.TransitionEnter();
            };

            _cameraFX.OnTransitionEnterEnded += delegate ()
            {
                reachedBarracks = true;
            };

            _cameraFX.TransitionExit();
        };

        _ArturFollowController.StartFollowing(_TourGuideKnight.gameObject, 0);

        yield return new WaitUntil(() => reachedBarracks);

        tourGuidePos = new Vector2Int(357, 75);

        StartCoroutine(_TourGuideKnight.WalkToCoroutine(tourGuidePos));

        yield return new WaitForSecondsRealtime(0.7f);

        var bedroomDoorIsOpen = false;
        _ArturFollowController.OnFollowComplete += delegate ()
        {
            _ArturFollowController.StopFollowing();

            _bedroomDoor.OnDoorOpened += delegate ()
            {
                bedroomDoorIsOpen = true;
            };

            _bedroomDoor.Open();
        };

        _ArturFollowController.StartFollowing(_TourGuideKnight.gameObject, 0);

        yield return new WaitUntil(() => bedroomDoorIsOpen);

        tourGuidePos = new Vector2Int(358, 86);
        StartCoroutine(_TourGuideKnight.WalkToCoroutine(tourGuidePos));

        yield return new WaitForSecondsRealtime(0.7f);

        var insideBedroom = false;
        _ArturFollowController.OnFollowComplete += delegate ()
        {
            _ArturFollowController.StopFollowing();

            _ArturFollowController.Rotate(Direction.Up);
            _ArturFollowController.SetIdle();

            _TourGuideKnight.Rotate(Direction.Down);
            _TourGuideKnight.SetIdle();

            insideBedroom = true;
        };

        _ArturFollowController.StartFollowing(_TourGuideKnight.gameObject, 0);

        yield return new WaitUntil(() => insideBedroom);
    }

    public IEnumerator HeadToTheArmory()
    {
        var tourGuidePos = new Vector2Int(399, 71);
        StartCoroutine(_TourGuideKnight.WalkToCoroutine(tourGuidePos));

        yield return new WaitForSecondsRealtime(0.5f);

        var worldGrid = WorldGrid.Instance;

        var arturPos = new Vector2Int(418, 71);

        var inArmoryHallway = false;
        _ArturFollowController.OnFollowComplete += delegate ()
        {
            _ArturFollowController.StopFollowing();

            _cameraFX.OnTransitionExitEnded += delegate ()
            {
                tourGuidePos = new Vector2Int(420, 71);

                worldGrid.PlaceGameObject(_TourGuideKnight.gameObject, tourGuidePos);
                worldGrid.PlaceGameObject(_ArturFollowController.gameObject, arturPos);

                _cameraFX.TransitionEnter();
            };

            _cameraFX.OnTransitionEnterEnded += delegate ()
            {
                inArmoryHallway = true;
            };
        };

        _ArturFollowController.StartFollowing(_TourGuideKnight.gameObject, 0);

        yield return new WaitUntil(() => inArmoryHallway);

        tourGuidePos = new Vector2Int(429, 75);
        StartCoroutine(_TourGuideKnight.WalkToCoroutine(tourGuidePos));

        yield return new WaitForSecondsRealtime(0.5f);

        var armoryDoorIsOpen = false;
        _ArturFollowController.OnFollowComplete += delegate ()
        {
            _ArturFollowController.StopFollowing();

            _armoryDoor.OnDoorOpened += delegate ()
            {
                armoryDoorIsOpen = true;
            };
            _armoryDoor.Open();
        };

        _ArturFollowController.StartFollowing(_TourGuideKnight.gameObject, 0);

        yield return new WaitUntil(() => armoryDoorIsOpen);

        tourGuidePos = new Vector2Int(430, 89);
        StartCoroutine(_TourGuideKnight.WalkToCoroutine(tourGuidePos));

        yield return new WaitForSecondsRealtime(0.5f);

        arturPos = new Vector2Int(428, 89);

        _ArturFollowController.enabled = false;
        _Artur.enabled = true;

        yield return _Artur.WalkToCoroutine(arturPos);

        MeetingHaroldCutscene.Instance.Play();
    }
}
