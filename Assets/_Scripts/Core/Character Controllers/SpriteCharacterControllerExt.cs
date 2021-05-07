// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using System;
using UnityEngine;
using Sirenix.OdinInspector;
using DarkTonic.MasterAudio;

using Animancer;

public enum ControllerState
{
    Ground,
    AutoMovement,
    Jumping,
    InTransition
}

public enum AutoMovementType
{
    None,
    Walk,
    Run
}

/// <summary>
/// A more complex version of the <see cref="SpriteMovementController"/> which adds running and pushing animations
/// as well as the ability to actually move around.
/// </summary>
/// <example><see href="https://kybernetik.com.au/animancer/docs/examples/directional-sprites/character-controller">Character Controller</see></example>
/// https://kybernetik.com.au/animancer/api/Animancer.Examples.DirectionalSprites/SpriteCharacterController
/// 
[AddComponentMenu("Directional Sprites - Sprite Character Controller")]
public sealed class SpriteCharacterControllerExt : MonoBehaviour
{
    private ControllerState _state;
    public ControllerState State { get => _state; }

    private AutoMovementType _autoMovement;
    public AutoMovementType AutoMovement { get => _autoMovement; }

    /************************************************************************************************************************/

    [FoldoutGroup("Physics")]
    [SerializeField] private CapsuleCollider2D _Collider;
    [SerializeField] private Rigidbody2D _Rigidbody;
    [SerializeField] private float _WalkSpeed = 1;
    [SerializeField] private float _RunSpeed = 2;
    [SerializeField] private float _JumpDuration = 1.6f;

    [FoldoutGroup("Animations")]
    [SerializeField] private AnimancerComponent _Animancer;
    [SerializeField] private DirectionalAnimationSet _Idle;
    [SerializeField] private DirectionalAnimationSet _Walk;
    [SerializeField] private DirectionalAnimationSet _Run;
    [SerializeField] private DirectionalAnimationSet _Push;
    [SerializeField] private DirectionalAnimationSet _Jump;
    [SerializeField] private DirectionalAnimationSet _InAir;
    [SerializeField] private DirectionalAnimationSet _Landing;
    [SerializeField] private Vector2 _Facing = Vector2.down;

    [HideInInspector] public Action OnAutoMoveComplete;

    private Vector2 _Movement;
    private SpriteRenderer _renderer;
    private DirectionalAnimationSet _CurrentAnimationSet;
    private TimeSynchronisationGroup _MovementSynchronisation;
    private FootstepController _footstepController;

    // Mecanim AnimationEvent Listeners
    private AnimationEventReceiver _OnPlayFootsteps;
    private AnimationEventReceiver _OnBeganJumping;


    private JumpController _jumpController;

    private JumpTrigger _jumpTrigger;
    private float _startedJumpTime;
    private Vector2 _jumpDestination;
    private bool _reachedJumpApex = false;
    private bool _spawnedLandingEffect = false;

    private Vector2 _autoMoveTarget = Vector2.negativeInfinity;

    /************************************************************************************************************************/

    private Unit _unit;

    private void Awake()
    {
        _unit = GetComponent<Unit>();
        _jumpController = GetComponent<JumpController>();
    }

    private void Start()
    {
        transform.parent = null;
        DontDestroyOnLoad(this.gameObject);

        _state = ControllerState.Ground;

        _renderer = GetComponent<SpriteRenderer>();
        _footstepController = GetComponent<FootstepController>();
        _MovementSynchronisation = new TimeSynchronisationGroup(_Animancer) { _Walk, _Run, _Jump, _InAir, _Push };

        Play(_Idle);
    }

    /************************************************************************************************************************/

    private void Update()
    {
        switch (State)
        {
            case ControllerState.Ground:
                HandleGroundMovement();
                break;
            case ControllerState.Jumping:
                break;
            case ControllerState.AutoMovement:
                HandleAutoMovement();
                break;
            case ControllerState.InTransition:
                break;
        }
    }

    public void FreezeInput()
    {
        _Movement = Vector2.zero;
        Play(_Idle);
        _state = ControllerState.InTransition;
    }

    public void AllowInput()
    {
        Play(_Idle);
        _state = ControllerState.Ground;
    }

    public void Jump(JumpTrigger jumpTrigger)
    {
        AttachJumpEvents();
        _jumpController.Jump(jumpTrigger);
    }

    private void AttachJumpEvents()
    {
        _jumpController.OnBeginJump += delegate ()
        {
            Play(_Jump);
            _state = ControllerState.InTransition;

            _jumpController.OnBeginJump = null;
        };

        _jumpController.UponLanding += delegate ()
        {
            Play(_Landing);

            _jumpController.UponLanding = null;
        };

        _jumpController.WhileInAir += delegate ()
        {
            Play(_InAir);

            _jumpController.WhileInAir = null;
        };
    }


    private void HandleGroundMovement()
    {
        _Movement = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (_Movement != Vector2.zero)
        {
            _Facing = _Movement;
            UpdateMovementState();

            // Snap the movement to the exact directions we have animations for.
            // When using DirectionalAnimationSets this means the character will only move up/right/down/left.
            // But DirectionalAnimationSet8s will allow diagonal movement as well.
            _Movement = _CurrentAnimationSet.Snap(_Movement);

            // If we are moving horizontally
            if (Mathf.Abs(_Movement.y) <= 0.001f)
            {
                var pos = (Vector2Int)WorldGrid.Instance.Grid.WorldToCell(transform.position);
                var cell = WorldGrid.Instance[pos];
                if (cell.IsStairs(_unit))
                {
                    if (cell.GetStairsOrientation(_unit) == StairsOrientation.LeftToRight)
                        _Movement.y = _Movement.x;
                    else
                        _Movement.y = -_Movement.x;
                }
            }

            _Movement = Vector2.ClampMagnitude(_Movement, 1);
        }
        else
            Play(_Idle);
    }

    private void HandleAutoMovement()
    {
        if (_autoMoveTarget == Vector2.negativeInfinity)
            throw new Exception("You have entered ControllerState.AutoMovement but somehow haven't set a valid point to travel to...");

        var reachedDestination = Vector2.Distance(transform.position, _autoMoveTarget) < 0.1;

        if (_Movement != Vector2.zero && !reachedDestination)
        {
            _Facing = _Movement;
            Play(_Walk);
                
            // Snap the movement to the exact directions we have animations for.
            // When using DirectionalAnimationSets this means the character will only move up/right/down/left.
            // But DirectionalAnimationSet8s will allow diagonal movement as well.
            _Movement = _CurrentAnimationSet.Snap(_Movement);

            // If we are moving horizontally
            if (Mathf.Abs(_Movement.y) <= 0.001f)
            {
                var pos = (Vector2Int)WorldGrid.Instance.Grid.WorldToCell(transform.position);
                var cell = WorldGrid.Instance[pos];
                if (cell.IsStairs(_unit))
                {
                    if (cell.GetStairsOrientation(_unit) == StairsOrientation.LeftToRight)
                        _Movement.y = _Movement.x;
                    else
                        _Movement.y = -_Movement.x;
                }
            }

            _Movement = Vector2.ClampMagnitude(_Movement, 1);
        }
        else
        {
            Play(_Idle);

            if (reachedDestination)
                if (OnAutoMoveComplete != null)
                {
                    _autoMoveTarget = Vector2.negativeInfinity;
                    OnAutoMoveComplete.Invoke();
                    OnAutoMoveComplete = null;
                }

        }
    }


    /************************************************************************************************************************/

    private void Play(DirectionalAnimationSet animations)
    {
        // Store the current time.
        _MovementSynchronisation.StoreTime(_CurrentAnimationSet);

        _CurrentAnimationSet = animations;

        var directionalClip = animations.GetClip(_Facing);
        if (!_Animancer.IsPlayingClip(directionalClip))
        {
            var state = _Animancer.Play(directionalClip);

            if (animations == _Walk || animations == _Run)
                _OnPlayFootsteps.Set(state, PlayFootstepSound());

            if (animations == _Jump)
                _OnBeganJumping.Set(state, TriggerJumpingState());

            if (animations == _Landing)
                state.Events.OnEnd = delegate ()
                {
                    _state = ControllerState.Ground;
                    _spawnedLandingEffect = false;
                };

            // If the new animation is in the synchronisation group, give it the same time the previous animation had.
            _MovementSynchronisation.SyncTime(_CurrentAnimationSet);
        }
    }

    public void WalkTo(Vector2 worldPosition)
    {
        _state = ControllerState.AutoMovement;

        _autoMoveTarget = worldPosition;

        var currentPosition = (Vector2)transform.position;

        var movement = Vector2.ClampMagnitude(worldPosition - currentPosition, 1);

        _Movement = movement;
    }

       

    /************************************************************************************************************************/

    // Pre-allocate an array of contact points so Unity doesn't need to allocate a new one every time we call
    // _Collider.GetContacts. This example will never have more than 4 contact points, but you might consider a
    // higher number in a real game. Even a large number like 64 would be better than making new ones every time.
    private static readonly ContactPoint2D[] Contacts = new ContactPoint2D[4];

    private void UpdateMovementState()
    {
        /*var contactCount = _Collider.GetContacts(Contacts);
        for (int i = 0; i < contactCount; i++)
        {
            // If we are moving directly towards an object (or within 30 degrees of it), we are pushing it.
            if (Vector2.Angle(Contacts[i].normal, _Movement) > 180 - 30)
            {
                Play(_Push);
                return;
            }
        }*/

        var isRunning = Input.GetButton("Fire3"); // Left Shift by default.
        Play(isRunning ? _Run : _Walk);
    }

    /************************************************************************************************************************/

    private void FixedUpdate()
    {
        // Determine the desired speed based on the current animation.
        var speed = _CurrentAnimationSet == _Run ? _RunSpeed : _WalkSpeed;
        _Rigidbody.velocity = _Movement * speed;
    }

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

    private void BeganJumping(AnimationEvent animationEvent)
    {
        _OnBeganJumping.SetFunctionName("BeganJumping");
        _OnBeganJumping.HandleEvent(animationEvent);
    }

    private System.Action<AnimationEvent> TriggerJumpingState()
    {
        return delegate (AnimationEvent animationEvent)
        {
            _startedJumpTime = Time.time;
            _state = ControllerState.Jumping;
        };
    }

    /************************************************************************************************************************/
#if UNITY_EDITOR
    /************************************************************************************************************************/

    /// <summary>[Editor-Only]
    /// Sets the character's starting sprite in Edit Mode so you can see it while working in the scene.
    /// </summary>
    /// <remarks>
    /// Called by the Unity Editor in Edit Mode whenever an instance of this script is loaded or a value is changed
    /// in the Inspector.
    /// </remarks>
    private void OnValidate()
    {
        if (_Idle == null)
            return;

        AnimancerUtilities.EditModePlay(_Animancer, _Idle.GetClip(_Facing), false);
    }

    /************************************************************************************************************************/
#endif
    /************************************************************************************************************************/
}
