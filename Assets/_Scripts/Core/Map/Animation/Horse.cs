using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using Sirenix.OdinInspector;
using Animancer;



public enum HorseColor
{
    Brown,
    Caramel,
    White
}

public class Horse : MonoBehaviour
{
    [SerializeField]
    private HorseColor _color;
    public HorseColor Color { get => _color; }

    [SerializeField]
    private DirectionalAnimationSet _horseIdles;
    
    [SerializeField]
    private DirectionalAnimationSet _horseWalks;
    
    [SerializeField]
    private DirectionalAnimationSet _horseRuns;

    [SerializeField]
    private GameObject _harnessStrap;



    private TimeSynchronisationGroup _MovementSynchronisation;
    private DirectionalAnimationSet _CurrentAnimationSet;
    private AnimancerComponent _animancer;
    private SpriteRenderer _renderer;

    private AnimationEventReceiver _OnPlayFootsteps;
    private FootstepController _footstepController;


    private Vector2 _Facing = Vector2.down;
    public Vector2 Facing { get => _Facing; }

    // Start is called before the first frame update
    private void Start()
    {
        _renderer                   = GetComponent<SpriteRenderer>();
        _animancer                  = GetComponent<AnimancerComponent>();
        _footstepController         = GetComponent<FootstepController>();
        _MovementSynchronisation    = new TimeSynchronisationGroup(_animancer) { _horseIdles, _horseWalks, _horseRuns };
    }

    public void PlayCurrentAnimSet()
    {
        if (_CurrentAnimationSet == _horseIdles)
            SetIdle();

        if (_CurrentAnimationSet == _horseWalks)
            Walk(_Facing);

        if (_CurrentAnimationSet == _horseRuns)
            Run(_Facing);
    }

    public void Walk(Vector2 facing)
    {
        _CurrentAnimationSet = _horseWalks;

        var directionalClip = _horseWalks.GetClip(facing);
        if (!_animancer.IsPlayingClip(directionalClip))
        {
            var state = _animancer.Play(directionalClip);

            _OnPlayFootsteps.Set(state, PlayFootstepSound());

            // If the new animation is in the synchronisation group, give it the same time the previous animation had.
            _MovementSynchronisation.SyncTime(_CurrentAnimationSet);
            
            _Facing = facing;
        }
    }


    public void Run(Vector2 facing, float animSpeed = 1f)
    {
        _CurrentAnimationSet = _horseRuns;

        var directionalClip = _horseRuns.GetClip(facing);
        if (!_animancer.IsPlayingClip(directionalClip))
        {
            var state = _animancer.Play(directionalClip);
            state.Speed = animSpeed;

            _OnPlayFootsteps.Set(state, PlayFootstepSound());

            // If the new animation is in the synchronisation group, give it the same time the previous animation had.
            _MovementSynchronisation.SyncTime(_CurrentAnimationSet);

            _Facing = facing;
        }
    }

    public void SetIdle()
    {
        _CurrentAnimationSet = _horseIdles;

        var directionalClip = _horseIdles.GetClip(_Facing);
        if (!_animancer.IsPlayingClip(directionalClip))
        {
            var state = _animancer.Play(directionalClip);

            _OnPlayFootsteps.Set(state, PlayFootstepSound());
            
            // If the new animation is in the synchronisation group, give it the same time the previous animation had.
            _MovementSynchronisation.SyncTime(_CurrentAnimationSet);
        }
    }

    public void LookAt(Vector2 position) => _Facing = position - (Vector2)transform.position;

    public void Rotate(Direction direction) => LookAt((Vector2)transform.position + direction.ToVector());


    public void HideHarnessStrap() => _harnessStrap.SetActive(false);
    public void ShowHarnessStrap() => _harnessStrap.SetActive(true);

    public void SetOrderInLayer(int orderInLayer) => _renderer.sortingOrder = orderInLayer;

    /************************************************************************************************************************/
    //  Animation Event Listeners and Logic
    /************************************************************************************************************************/

    private void PlayFootsteps(AnimationEvent animationEvent)
    {
        _OnPlayFootsteps.SetFunctionName("PlayFootsteps");
        _OnPlayFootsteps.HandleEvent(animationEvent);
    }

    private Action<AnimationEvent> PlayFootstepSound()
    {
        return delegate (AnimationEvent animationEvent)
        {
            var currentGridPosition = (Vector2Int)WorldGrid.Instance.Grid.WorldToCell(transform.position);

            var currentSortingLayer = _renderer.sortingLayerID;
            var worldCell = WorldGrid.Instance[currentGridPosition];
            var walkingOnSurface = worldCell.TileAtSortingLayer(currentSortingLayer).SurfaceType;

            _footstepController.PlaySound(walkingOnSurface);
        };
    }
}
