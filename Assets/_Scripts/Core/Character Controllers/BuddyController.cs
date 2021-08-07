using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Sirenix.OdinInspector;
using Animancer;
using System;
using MysteryBabylone.Utils.Tasks;

public enum BuddyMovementMode
{
    Following,
    Expanding,
    Collapsing
}
public class BuddyController : BaseController
{
    // We want to be slightly faster than the player, but since the players speed is set via velocity it's
    // harder to match it precisely. If the player speeds change, these need to change to match and
    // then beat the player speeds
    public bool IsFollowing { get; private set; }
    [Header("Character Variables")]

    /// <summary>
    /// An object that should have spriteCharacterControllerExt and any other relevant reference scripts.
    /// </summary>
    [ShowIf("IsFollowing"), SerializeField] private GameObject playerObject;
    [ShowIf("IsFollowing"), ReadOnly] public float currentSpeed = 7.5f;
    [ShowIf("IsFollowing"), ReadOnly] public Vector2 positionToMoveTo = Vector2.negativeInfinity;

    [HideInInspector] public Action OnFollowComplete;

    private SpriteCharacterControllerExt _controller;

    private float _startedJumpTime;
    private GridPath _autoMovePath;

    // Mecanim AnimationEvent Listeners
    private AnimationEventReceiver _OnPlayFootsteps;
    private AnimationEventReceiver _OnBeganJumping;

    private DirectionalAnimationSet moveAnimation;

    //Coroutine that handles movement. Use this class instead of directly starting/stoping coroutines.
    private Task movementCoroutine;

    //Cached variables for the buddy following process
    public bool IsCollapsed;
    public int followerIndex { get; private set; }
    private Vector3 nextPathPosition;
    private Vector2Int nextPathGridPosition;
    private bool reachedGoal;
    private Action OnCollapseComplete;
    private Action OnExpandComplete;

    #region Debug Variables
    GridUtility _debugGridUtility = new GridUtility();
    public bool DebugPathfinding;
    #endregion

    protected override void Awake()
    {
        base.Awake();

        IsFollowing = false;

        movementCoroutine = new Task(AutoMovementCoroutine(), false);
        OnFollowComplete += OnExpandComplete;
        OnFollowComplete += OnCollapseComplete;

    }


    public void StartFollowing(GameObject leader, int fIndex)
    {
        followerIndex = fIndex;
        playerObject = leader;

        var myController = GetComponent<SpriteCharacterControllerExt>();
        if (myController != null) myController.enabled = false;

        var spriteCharacterController = leader.GetComponent<SpriteCharacterControllerExt>();
        if (spriteCharacterController != null)
        {
            _controller = spriteCharacterController;
            _controller.OnRunningSet += SetSpeed;
        }

        moveAnimation = _Walk;
        movementCoroutine.Start();
        // _Collider.enabled = true;

        IsFollowing = true;
    }

    public void StopFollowing()
    {
        playerObject = null;
        _controller = null;
        _Collider.enabled = false;
        IsFollowing = false;

        SetIdle();

        var myController = GetComponent<SpriteCharacterControllerExt>();
        if (myController != null) myController.FreezeInput();

        movementCoroutine.Stop();
    }

    [Button("Collapse")]
    public void Collapse(Action OnComplete = null)
    {
        if (IsCollapsed)
            return;

        movementCoroutine.Start();
        DeterminePosition(BuddyMovementMode.Collapsing);

        OnFollowComplete += Hide;
        OnCollapseComplete = OnComplete;
        OnExpandComplete = null;
    }

    public void Expand(Vector2 fromPosition, Action OnComplete = null)
    {
        if (!IsCollapsed)
            return;

        gameObject.transform.position = fromPosition;
        gameObject.SetActive(true);
        movementCoroutine.Start();
        DeterminePosition(BuddyMovementMode.Expanding);
        IsCollapsed = false;

        OnFollowComplete -= Hide;
        OnExpandComplete = OnComplete;
        OnCollapseComplete = null;
    }

    [Button("Expand")]
    public void Expand(Action OnComplete = null)
    {
        Expand(playerObject.transform.position, OnComplete);
    }


    public void SetIdle() => Play(_Idle);


    private void Update()
    {
        /*if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug_FindLeaderPosition();
        }*/

        if (!IsFollowing)
            return;
        var distance = Vector2.Distance(transform.position, positionToMoveTo);



/*      if (distance <= .5f)
        {
            Debug.Log("Hitting idle");
            SetIdle();
        }*/


    }

    /*private void OnDestroy()
    {
        _controller.OnRunningSet -= SetSpeed;
    }

    private void OnDisable()
    {
        _controller.OnRunningSet -= SetSpeed;
    }
*/
    private void SetSpeed(bool playerIsRunning)
    {
        if (playerIsRunning)
        {
            moveAnimation = _Run;
            currentSpeed = _RunSpeed;
        }

        else
        {
            moveAnimation = _Walk;
            currentSpeed = _WalkSpeed;
        }
    }

    public void DeterminePosition(BuddyMovementMode movementMode = BuddyMovementMode.Following)
    {
        Vector2Int currentGridPosition = (Vector2Int)WorldGrid.Instance.Grid.WorldToCell(transform.position);
        Vector2Int? targetGridPosition = GetTargetPosition(movementMode);

        if (targetGridPosition == null)
            return;

        bool includeOccupiedCells = movementMode == BuddyMovementMode.Collapsing;
        positionToMoveTo = WorldGrid.Instance.Grid.GetCellCenterWorld((Vector3Int)targetGridPosition);

        _autoMovePath = GridUtility.FindPath(Renderer.sortingLayerID, currentGridPosition, targetGridPosition.Value, includeOccupiedCells);

        if (_autoMovePath != null && _autoMovePath.Length > 0)
        {
            var newGridPosition = _autoMovePath[0];
            var direction = GridUtility.GetDirection(currentGridPosition, newGridPosition, true);
            nextPathPosition = WorldGrid.Instance.Grid.GetCellCenterWorld((Vector3Int)newGridPosition);
            Rotate(direction);
            reachedGoal = false;
        }

    }

    protected Vector2Int? GetTargetPosition(BuddyMovementMode movementMode)
    {
        switch (movementMode)
        {
            case BuddyMovementMode.Following:
            case BuddyMovementMode.Expanding:
                return PlayerTeamController.Instance.GetFollowingDestinationByIndex(followerIndex);
            case BuddyMovementMode.Collapsing:
                return (Vector2Int)WorldGrid.Instance.Grid.WorldToCell(playerObject.transform.position);
            default:
                return null;
        }
    }


    protected IEnumerator AutoMovementCoroutine(bool running = false)
    {

        yield return new WaitUntil(() => _autoMovePath != null && _autoMovePath.Length > 0);

        var currentGridPosition = (Vector2Int)WorldGrid.Instance.Grid.WorldToCell(this.transform.position);
        nextPathGridPosition = currentGridPosition;
        nextPathPosition = transform.position;
        reachedGoal = false;

        var origin = currentGridPosition;
        var goal = _autoMovePath.Goal;

        while (true)
        {
            SetSpeed(running);

            var speed = currentSpeed * Time.deltaTime;
            
            if (moveAnimation != null && !_Animancer.IsPlaying(moveAnimation.GetClip(_Facing)))
                Play(moveAnimation);
          
            while (speed > 0.0001f && !reachedGoal)
            {
                speed = MoveTo(nextPathPosition, speed);

                if (speed > 0.0001f)
                {
                    if (_autoMovePath != null && _autoMovePath.Length > 0)
                    {
                        // Get new destination position and direction
                        var newGridPosition = _autoMovePath.Pop();
                        var direction = GridUtility.GetDirection(nextPathGridPosition, newGridPosition, true);

                        nextPathPosition = WorldGrid.Instance.Grid.GetCellCenterWorld((Vector3Int)newGridPosition);
                        nextPathGridPosition = newGridPosition;

                        // Rotate
                        Rotate(direction);
                    }
                    else // End movement
                    {
                        Debug.Log("Reached destination");
                        reachedGoal = true;
                        transform.position = positionToMoveTo;
                        
                        SetIdle();

                        OnFollowComplete?.Invoke();
                        OnFollowComplete = null;
                        break;
                    }
                }
                yield return new WaitForEndOfFrame();
            }
            yield return new WaitForEndOfFrame();
        }


    }

    private void Hide()
    {
        movementCoroutine.Stop();
        IsCollapsed = true;
        OnCollapseComplete?.Invoke();
        gameObject.SetActive(false);

    }

    #region Functions to collapse to an interface or inherited class?
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


            // If the new animation is in the synchronisation group, give it the same time the previous animation had.
            _MovementSynchronisation.SyncTime(_CurrentAnimationSet);
        }
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

            _JumpController.OnBeginJump = null;
        };

        _JumpController.UponLanding += delegate ()
        {
            Play(_Landing);

            _JumpController.UponLanding = null;
        };

        _JumpController.WhileInAir += delegate ()
        {
            Play(_InAir);

            _JumpController.WhileInAir = null;
        };
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

    private Action<AnimationEvent> TriggerJumpingState()
    {
        return delegate (AnimationEvent animationEvent)
        {
            _startedJumpTime = Time.time;
        };
    }
    #endregion



    private void OnDrawGizmos()
    {
        if (!Application.IsPlaying(this))
            return;

        if (DebugPathfinding && _autoMovePath != null)
            foreach (var pos in _autoMovePath.Path())
            {
                Gizmos.color = Color.gray;
                Gizmos.DrawSphere(WorldGrid.Instance.Grid.GetCellCenterWorld((Vector3Int)pos), 0.5f);
            }

        if (DebugPathfinding && _debugGridUtility.gizmosCells != null)
        {
            foreach (var item in _debugGridUtility.gizmosCells)
            {
                //Gizmos.color = Color.gray;
                //Gizmos.DrawSphere(WorldGrid.Instance.Grid.GetCellCenterWorld((Vector3Int)item), 0.5f);


                var travelCostColorRatio = WorldGrid.Instance[item.x, item.y].GetTravelCost(Renderer.sortingLayerID, UnitType.Ground) / (float)int.MaxValue;
                Gizmos.color = new Color(travelCostColorRatio, travelCostColorRatio, travelCostColorRatio);
                Gizmos.DrawSphere(WorldGrid.Instance.Grid.GetCellCenterWorld((Vector3Int)item), 0.5f);
            }
        }

        //if (positionToMoveTo != Vector2.negativeInfinity)
        //    Gizmos.DrawCube(positionToMoveTo, Vector3.one);

    }

    //<summary>
    //Function for debugging the path finding using gizmos.
    //</summary>
    private void Debug_FindLeaderPosition()
    {
        var currentGridPosition = (Vector2Int)WorldGrid.Instance.Grid.WorldToCell(transform.position);
        var targetGridPosition = PlayerTeamController.Instance.GetFollowingDestinationByIndexOrDefault(followerIndex, currentGridPosition);

        positionToMoveTo = WorldGrid.Instance.Grid.GetCellCenterWorld((Vector3Int)targetGridPosition);
        StartCoroutine(_debugGridUtility.Debug_FindPath(Renderer.sortingLayerID, currentGridPosition, targetGridPosition));
    }
}
