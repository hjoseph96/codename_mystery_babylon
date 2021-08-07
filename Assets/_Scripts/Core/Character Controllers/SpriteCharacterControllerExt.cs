// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

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
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AnimancerComponent))]
[AddComponentMenu("Sprite Character Controller")]
public class SpriteCharacterControllerExt : BaseController, ISpeakableEntity
{
    [FoldoutGroup("Basic Information")]
    [SerializeField]
    private ControllerState _state;
    public ControllerState State { get => _state; }

    [FoldoutGroup("AI Patrol")]
    [SerializeField] protected bool _IsPatrolling = false;
    [FoldoutGroup("AI Patrol")]
    [SerializeField] protected float _CooldownTime = 3f;
    [FoldoutGroup("AI Patrol")]
    [ReadOnly] protected int _CurrentPatrolPointIndex = 0;
    [FoldoutGroup("AI Patrol")]
    [SerializeField] protected List<Vector2Int> _PatrolPoints = new List<Vector2Int>();


    /************************************************************************************************************************/

    [HideInInspector] public Action OnAutoMoveComplete;


    protected bool _spawnedHelmet = false;
    
	protected bool _isRestrained = false;



    // Mecanim AnimationEvent Listeners
    private AnimationEventReceiver _OnPlayFootsteps;
    private AnimationEventReceiver _OnBeganJumping;

    private float _startedJumpTime;

    protected GridPath _autoMovePath;

    public Vector2Int GridPosition;
    /// <summary>
    /// Used to update dialogue bubbles and animations to face player position 
    /// </summary>
    public Action<Vector3> OnPositionChanged;
    /// <summary>
    /// Used to be notified whenever the grid position of this character has changed
    /// </summary>
    public Action<Vector2Int, Vector2Int> OnGridPositionChanged;

    /// <summary>
    /// Called when on Running is begun, or ended.
    /// </summary>
    public Action<bool> OnRunningSet;

    private bool _paused = false;

    protected Unit _unit;

    /************************************************************************************************************************/

    #region Monobehaviour
    protected override void Awake()
    {
        base.Awake();

        if (IsSeated)
            _Collider.enabled = false;

        PauseMenu.OnGamePaused += SetPausedState;
    }

    protected virtual void Start()
    {
        _unit = GetComponent<Unit>();
        GridPosition = WorldGrid.Instance != null ? (Vector2Int)WorldGrid.Instance.Grid.WorldToCell(transform.position) : new Vector2Int();

        if (_state != ControllerState.AutoMovement)
            _state = ControllerState.Ground;
        else if (_IsPatrolling)
            StartPatrolling();

        _Facing = DirectionUtility.DirectionToFacing[_StartingLookDirection];
    }

    private void OnDestroy()
    {
        PauseMenu.OnGamePaused -= SetPausedState;
    }

    private void Update()
    {
        if (_paused)
            return;

        switch (_state)
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
    #endregion

    /************************************************************************************************************************/

    /// <summary>
    /// This sets a bool and prevents actions from happening automatically and prevents user input from affecting controller
    /// <br>Can be expanded to stop animations from playing and resume afterwards</br>
    /// </summary>
    /// <param name="isPaused"></param>
    private void SetPausedState(bool isPaused)
    {
        _paused = isPaused;
        _Movement = Vector2.zero;
        _Rigidbody.velocity = Vector2.zero;
        _Rigidbody.angularVelocity = 0;
        Play(_Idle);
    }

    public BubblePositionController.EntityInfo GetEntityInfo()
    {
        //TODO: Determine standing/sitting/mounted based on what current animation branch is, or other checks
        return new BubblePositionController.EntityInfo { facingDirection = DirectionUtility.FacingToDirection[_Facing], mounted = false, sitting = IsSeated || IsIncapacitated };
    }

    public void BecomeCorpse()
    {
        _isDead = true;
        Play(_Dead);
    }

    public void SetAutoMode() => _state = ControllerState.AutoMovement;


    public void SwitchToBattleMode()
    {
        this.enabled = false;

        var buddyAI = GetComponent<BuddyController>();
        if (buddyAI != null) buddyAI.enabled = false;
    }

    public void SetIdle() => Play(_Idle);

    public void DisableCollider() => _Collider.enabled = false;
    public void EnableCollider() => _Collider.enabled = true;

    public void FreezeInput()
    {
        _state = ControllerState.InTransition;

        _Movement = Vector2.zero;
        _Rigidbody.velocity = Vector2.zero;
        _Rigidbody.angularVelocity = 0;

        Play(_Idle);
    }

    public void AllowInput()
    {
        _Collider.enabled = true;
        _state = ControllerState.Ground;

        Play(_Idle);
    }

    public void Die()
    {
        _isDying = true;
        Play(_Death);
    }

    protected IEnumerator PatrolToNextPoint()
    {
        yield return new WaitForSecondsRealtime(_CooldownTime);

        _CurrentPatrolPointIndex++;
        if (_CurrentPatrolPointIndex == _PatrolPoints.Count)
            _CurrentPatrolPointIndex = 0;

        var currentCell = (Vector2Int)WorldGrid.Instance.Grid.WorldToCell(this.transform.position);

        var nextPatrolPoint = _PatrolPoints[_CurrentPatrolPointIndex];
        
        _autoMovePath = GridUtility.FindPath(Renderer.sortingLayerID, currentCell, nextPatrolPoint);
        
        StartCoroutine(AutoMovementCoroutine());
    }

    public void StartPatrolling(List<Vector2Int> patrolPoints = null)
    {
        if (patrolPoints != null)
        {
            foreach (var patrolPoint in patrolPoints)
                if (!WorldGrid.Instance.PointInGrid(patrolPoint))
                    throw new Exception($"[SpriteCharacterController] ({gameObject.name}): Given grid cell point [{patrolPoint.x}, {patrolPoint.y}] is not within the WorldGrid");

            _PatrolPoints = patrolPoints;
        }

        _IsPatrolling = true;

        var currentCell = (Vector2Int)WorldGrid.Instance.Grid.WorldToCell(this.transform.position);

        _state = ControllerState.AutoMovement;
        _autoMovePath = GridUtility.FindPath(Renderer.sortingLayerID, currentCell, _PatrolPoints[0]);

        StartCoroutine(AutoMovementCoroutine());
    }



    public void WalkTo(Vector2 worldPosition)
    {
        _state = ControllerState.AutoMovement;

        var currentCell = (Vector2Int)WorldGrid.Instance.Grid.WorldToCell(this.transform.position);
        var goalCell = (Vector2Int)WorldGrid.Instance.Grid.WorldToCell(worldPosition);

        _autoMovePath = GridUtility.FindPath(Renderer.sortingLayerID, currentCell, goalCell);

        StartCoroutine(AutoMovementCoroutine());
    }

    public void WalkTo(Vector2Int gridPosition)
    {
        _state = ControllerState.AutoMovement;

        var currentCell = (Vector2Int)WorldGrid.Instance.Grid.WorldToCell(this.transform.position);
        var goalCell = gridPosition;

        _autoMovePath = GridUtility.FindPath(Renderer.sortingLayerID, currentCell, goalCell);

        StartCoroutine(AutoMovementCoroutine());
    }

    public IEnumerator WalkToCoroutine(Vector2Int gridPosition, bool shouldBeRunning = false)
    {
        if (_paused)
            yield return new WaitForEndOfFrame();

        _state = ControllerState.AutoMovement;

        var currentCell = (Vector2Int)WorldGrid.Instance.Grid.WorldToCell(this.transform.position);
        var goalCell = gridPosition;

        _autoMovePath = GridUtility.FindPath(Renderer.sortingLayerID, currentCell, goalCell);

/*        for (int i = 0; i < _autoMovePath.Length; i++)
        {
            Debug.Log(_autoMovePath.Path()[i]);
        }*/

        if (_autoMovePath == null || _autoMovePath.Length == 0)
            Debug.Log("Catch Me! *Ensure Final Path tile is a movable tile!*");

        yield return AutoMovementCoroutine(shouldBeRunning);
    }

    public IEnumerator WalkToCoroutine(Vector2 worldPosition, bool shouldBeRunning = false)
    {
        if (_paused)
            yield return new WaitForEndOfFrame();

        _state = ControllerState.AutoMovement;

        var currentCell = (Vector2Int)WorldGrid.Instance.Grid.WorldToCell(this.transform.position);
        var goalCell = (Vector2Int)WorldGrid.Instance.Grid.WorldToCell(worldPosition);


        _autoMovePath = GridUtility.FindPath(Renderer.sortingLayerID, currentCell, goalCell);

        yield return AutoMovementCoroutine(shouldBeRunning);
    }

    public IEnumerator WalkToCoroutine(GridPath path, bool shouldBeRunning = false)
    {
        if (_paused)
            yield return new WaitForEndOfFrame();

        _state = ControllerState.AutoMovement;

        _autoMovePath = path;

        yield return AutoMovementCoroutine(shouldBeRunning);
    }

    public void Jump(JumpTrigger jumpTrigger)
    {
        AttachJumpEvents();
        _JumpController.Jump(jumpTrigger);
    }

    private void AttachJumpEvents()
    {
        _JumpController.OnBeginJump += delegate ()
        {
            Play(_Jump);
            _state = ControllerState.InTransition;

            _JumpController.OnBeginJump = null;
        };

        _JumpController.UponLanding += delegate ()
        {
            Play(_Landing);
            _state = ControllerState.Ground;


            _JumpController.UponLanding = null;
        };

        _JumpController.WhileInAir += delegate ()
        {
            Play(_InAir);

            _JumpController.WhileInAir = null;
        };
    }

    public void SetIncapacitated()
    {
        _isIncapacitated = true;
        Play(_Incapacitated);
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
                if (cell.IsStairs(Renderer.sortingLayerID))
                    _Movement.y = _Movement.x;
            }

            _Movement = Vector2.ClampMagnitude(_Movement, 1);

            OnPositionChanged?.Invoke(transform.position);

            var currentGridPos = (Vector2Int)WorldGrid.Instance.Grid.WorldToCell(transform.position);
            if (GridPosition != currentGridPos)
            {
                if (_unit != null)
                    _unit.SetGridPosition(currentGridPos);

                OnGridPositionChanged?.Invoke(GridPosition, currentGridPos);
                GridPosition = currentGridPos;
            }
        }
        else
        {
            if (IsSeated)
                Play(_Sitting);
            else if (IsIncapacitated)
                Play(_Incapacitated);
            else if (_isRestrained)
                PlayRestrainedAnim();
            else
                Play(_Idle);

            OnRunningSet?.Invoke(false);
        }
    }

    protected virtual void HandleAutoMovement()
    {
        if (_autoMovePath == null || _autoMovePath.Length == 0)
        {
            if (!_spawnedHelmet && _isIncapacitated) return;

            if (IsSeated)
                Play(_Sitting);
            else if (IsIncapacitated)
                Play(_StayIncapacitated);
            else if (_isRestrained)
                PlayRestrainedAnim();
            else if (_isDying)
                return;
            else if (IsDead)
                Play(_Dead);
            else
                Play(_Idle);
        }
    }

    protected IEnumerator AutoMovementCoroutine(bool running = false)
    {
        if (_autoMovePath.Length == 0)
            yield break;

        StopSitting();

        var currentGridPosition = (Vector2Int)WorldGrid.Instance.Grid.WorldToCell(this.transform.position);
        var nextPathGridPosition = currentGridPosition;
        var nextPathPosition = transform.position;
        var reachedGoal = false;

        var origin = currentGridPosition;
        var goal = _autoMovePath.Goal;


        DirectionalAnimationSet moveAnimation = _Walk;

        while (!reachedGoal)
        {
            if (_paused)
                yield return new WaitForEndOfFrame();

            var speed = _WalkSpeed * Time.deltaTime;

            if (running) // Left Shift by default.
            {
                moveAnimation = _Run;
                speed = _RunSpeed * Time.deltaTime;
            }
            else
                moveAnimation = _Walk;

            if (!_Animancer.IsPlaying(moveAnimation.GetClip(_Facing)))
                Play(moveAnimation);

            while (speed > 0.0001f)
            {
                if (_paused)
                    yield return new WaitForEndOfFrame();

                speed = MoveTo(nextPathPosition, speed);
                if (speed > 0.0001f)
                {
                    if (_autoMovePath.Length > 0)
                    {
                        // Get new destination position and direction
                        var newGridPosition = _autoMovePath.Pop();
                        var direction = GridUtility.GetDirection(nextPathGridPosition, newGridPosition, true);
                        nextPathPosition = WorldGrid.Instance.Grid.GetCellCenterWorld((Vector3Int)newGridPosition);
                        nextPathGridPosition = newGridPosition;

                        // Rotate
                        Rotate(direction);

                        var currentGridPos = (Vector2Int)WorldGrid.Instance.Grid.WorldToCell(transform.position);
                        if (GridPosition != currentGridPos)
                        {
                            if (_unit != null)
                                _unit.SetGridPosition(currentGridPos);

                            OnGridPositionChanged?.Invoke(GridPosition, currentGridPos);
                            GridPosition = currentGridPos;
                        }
                    }
                    else // End movement
                    {
                        reachedGoal = true;
                        break;
                    }
                }
            }

            yield return new WaitForEndOfFrame();

        }

        if (IsSeated)
            Play(_Sitting);
        else
            Play(_Idle);

        OnAutoMoveComplete?.Invoke();
        OnAutoMoveComplete = null;

        if (_IsPatrolling)
            yield return PatrolToNextPoint();
    }

    public void MoveTo(Vector2 worldPosition, bool shouldBeRunning = false)
    {
        _state = ControllerState.AutoMovement;

        var currentCell = (Vector2Int)WorldGrid.Instance.Grid.WorldToCell(this.transform.position);
        var goalCell = (Vector2Int)WorldGrid.Instance.Grid.WorldToCell(worldPosition);

        _autoMovePath = GridUtility.FindPath(Renderer.sortingLayerID, currentCell, goalCell);

        StartCoroutine(AutoMovementCoroutine(shouldBeRunning));
    }

    /************************************************************************************************************************/

    protected virtual void Play(DirectionalAnimationSet animations)
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
                };

            if (animations == _Death)
                state.Events.OnEnd += delegate ()
                {
                    _isDying = false;
                    _isDead = true;

                    Play(_Dead);
                };

            if (animations == _Incapacitated)
                state.Events.OnEnd = delegate ()
                {
                    Play(_StayIncapacitated);
                };

            if (animations == _StayIncapacitated)
            {
                var christianAnimSet = GetComponent<ChristianAnimationSet>();
                state.Events.OnEnd = delegate ()
                {
                    if (!_spawnedHelmet && christianAnimSet != null)
                    {
                        christianAnimSet.OverrideKnightAnimations();
                        Play(_HelmetDrop);
                    }
                };
            }

            if (animations == _HelmetDrop)
                state.Events.OnEnd = delegate ()
                {
                    SpawnHelmet();
                };

            // If the new animation is in the synchronisation group, give it the same time the previous animation had.
            _MovementSynchronisation.SyncTime(_CurrentAnimationSet);
        }
    }

    // Christian Specific
    public void PlayRestrainedAnim()
    {
        _isRestrained = true;
        var christianAnimSet = GetComponent<ChristianAnimationSet>();
        Play(christianAnimSet._TiedToTree);
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
        OnRunningSet?.Invoke(isRunning);
        Play(isRunning ? _Run : _Walk);
    }

    /************************************************************************************************************************/

    private void FixedUpdate()
    {
        if (_paused)
            return;

        if (_state == ControllerState.Ground)
        {
            // Determine the desired speed based on the current animation. 
            var speed = _CurrentAnimationSet == _Run ? _RunSpeed : _WalkSpeed;
            _Rigidbody.velocity = _Movement * speed;
        }
    }

    private void OnDrawGizmos()
    {
        if (Application.IsPlaying(this) && _autoMovePath != null && _autoMovePath.Length > 0)
            foreach (var pos in _autoMovePath.Path())
                Gizmos.DrawSphere(WorldGrid.Instance.Grid.GetCellCenterWorld((Vector3Int)pos), 0.5f);
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

            var currentSortingLayer = Renderer.sortingLayerID;
            var worldCell = WorldGrid.Instance[currentGridPosition];

            var walkingOnSurface = worldCell.TileAtSortingLayer(currentSortingLayer).SurfaceType;

            _FootstepController.PlaySound(walkingOnSurface);
        };
    }

    private void BeganJumping(AnimationEvent animationEvent)
    {
        _OnBeganJumping.SetFunctionName("BeganJumping");
        _OnBeganJumping.HandleEvent(animationEvent);
    }

    private Action<AnimationEvent> TriggerJumpingState()
    {
        return delegate (AnimationEvent animationEvent)
        {
            _startedJumpTime = Time.time;
            _state = ControllerState.Jumping;
        };
    }

    private void SpawnHelmet()
    {
        if (!_spawnedHelmet)
        {
            var christianAnimation = GetComponent<ChristianAnimationSet>();
            var prefab = christianAnimation._helmetPrefab;
            Instantiate(prefab, HelmetSpawnPoint.position, Quaternion.identity, null);

            Play(_StayIncapacitated);

            _spawnedHelmet = true;
        }
    }
}