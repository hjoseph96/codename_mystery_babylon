using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Sirenix.OdinInspector;
using DarkTonic.MasterAudio;
using Animancer;


public enum CarriageControlMode
{
    Auto,
    Player
}

public class CarriageController : SerializedMonoBehaviour
{
    [SerializeField, ValueDropdown("ValidDirections")]
    private Direction _lookingDirection;
    [Button("Look This Way")]
    private void LookThisWay()
    {
        _renderer = GetComponent<SpriteRenderer>();

        foreach (var horse in GetComponentsInChildren<Horse>())
            _horses.Add(horse);

        _animancer = GetComponent<AnimancerComponent>();
        _MovementSynchronisation = new TimeSynchronisationGroup(_animancer) { _CarriageIdles, _CarriageMoving };

        foreach (var cachedPosition in GetComponentsInChildren<CachedPositions>())
            _spritePositions.Add(cachedPosition);
        
        LookAt(_lookingDirection);
    }

    [FoldoutGroup("Basic Properties"), SerializeField]
    private float _walkSpeed;
    [FoldoutGroup("Basic Properties"), SerializeField]
    private float _runSpeed;
    [FoldoutGroup("Basic Properties"), SerializeField]
    private float _runAnimationSpeed = 1.5f;
    [FoldoutGroup("Basic Properties"), SerializeField]
    private AnimatedDoor _exteriorDoor;
    public AnimatedDoor ExteriorDoor { get => _exteriorDoor; }

    [FoldoutGroup("Basic Properties"), SerializeField, HideIf("IsPlaying")]
    private CarriageControlMode _ControlMode;
    [FoldoutGroup("Basic Properties"), ShowInInspector, ShowIf("IsPlaying")]
    public CarriageControlMode ControlMode { get => _ControlMode; }



    [FoldoutGroup("Animations"), SerializeField, ValueDropdown("ValidDirections")]
    private Direction _startingLookDirection;

    private List<Direction> ValidDirections = new List<Direction>
    {
        Direction.Down,
        Direction.Left,
        Direction.Up,
        Direction.Right
    };

    private Dictionary<Direction, Vector2> DirectionToFacing = new Dictionary<Direction, Vector2>
    {
        { Direction.Down,   Vector2.down },
        { Direction.Left,   Vector2.left },
        { Direction.Up,     Vector2.up },
        { Direction.Right,  Vector2.right },
    };

    [FoldoutGroup("Animations")]
    [SerializeField] private DirectionalAnimationSet _CarriageIdles;
    [FoldoutGroup("Animations")]
    [SerializeField] private DirectionalAnimationSet _CarriageMoving;

    [FoldoutGroup("Colliders")]
    [SerializeField] private GameObject _horizontalColliders;
    [FoldoutGroup("Colliders")]
    [SerializeField] private GameObject _verticalColliders;

    [FoldoutGroup("Audio"), SerializeField, SoundGroup]
    private string _movingSound;



    private List<Horse> _horses = new List<Horse>();
    [FoldoutGroup("References")]
    [ShowInInspector]
    public List<Horse> Horses { get => _horses; }

    [FoldoutGroup("References"), SerializeField]
    private List<GameObject> _horseHolders = new List<GameObject>();
    public List<GameObject> WoodenHorseHolders { get => _horseHolders; }

    private SpriteRenderer _renderer;
    private TimeSynchronisationGroup _MovementSynchronisation;
    private DirectionalAnimationSet _CurrentAnimationSet;
    private AnimancerComponent _animancer;
    private Vector2 _Movement;
    private Vector2 _Facing = Vector2.down;


    private Vector2 _movementTarget = Vector2.negativeInfinity;
    [SerializeField]
    private bool _isRunning = false;
    private bool _isMoving = false;
    private bool _hasPathBeenSet = false;
    public Vector2Int GridPosition { get; private set; }

    private List<CachedPositions> _spritePositions = new List<CachedPositions>();

    [HideInInspector] public Action OnFinishedMoving;


    // Start is called before the first frame update
    void Start()
    {
        _renderer = GetComponent<SpriteRenderer>();

        foreach (var horse in GetComponentsInChildren<Horse>())
            _horses.Add(horse);

        _animancer = GetComponent<AnimancerComponent>();
        _MovementSynchronisation = new TimeSynchronisationGroup(_animancer) { _CarriageIdles, _CarriageMoving };

        foreach (var cachedPosition in GetComponentsInChildren<CachedPositions>())
            _spritePositions.Add(cachedPosition);

        LookAt(_startingLookDirection);

        Play(_CarriageIdles);
        MakeHorsesIdle();

        GridPosition = (Vector2Int)WorldGrid.Instance.Grid.WorldToCell(transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        switch (ControlMode)
        {
            case CarriageControlMode.Auto:
                HandleAutoMovement();
                break;
            case CarriageControlMode.Player:
                HandlePlayerMovement();
                break;
        }
    }

    public void RunTo(Vector2 movementTarget)
    {
        _isRunning = true;
        _ControlMode = CarriageControlMode.Auto;
        _movementTarget = movementTarget;
    }

    public void WalkTo(Vector2 movementTarget)
    {
        _isRunning = false;
        _ControlMode = CarriageControlMode.Auto;
        _movementTarget = movementTarget;
    }

    private void Play(DirectionalAnimationSet animations, float animationSpeed = 1)
    {
        _CurrentAnimationSet = animations;

        // Store the current time.
        _MovementSynchronisation.StoreTime(_CurrentAnimationSet);

        var directionalClip = animations.GetClip(_Facing);
        if (!_animancer.IsPlayingClip(directionalClip))
        {
            var state = _animancer.Play(directionalClip);
            state.Speed = animationSpeed;

            // Handle Flipping Door based on direction + don't show door for up/down idles
            if (animations == _CarriageIdles)
            {
                if (_lookingDirection == Direction.Left || _lookingDirection == Direction.Right)
                    _exteriorDoor.SetActive(true);
                else
                    _exteriorDoor.SetActive(false);
                    
                if (_exteriorDoor.IsActive())
                {
                    if (_lookingDirection == Direction.Left && _exteriorDoor.IsFlipped)
                        _exteriorDoor.FlipX();
                    else if (_lookingDirection == Direction.Right && !_exteriorDoor.IsFlipped)
                        _exteriorDoor.FlipX();
                }
            }

            // If the new animation is in the synchronisation group, give it the same time the previous animation had.
            _MovementSynchronisation.SyncTime(_CurrentAnimationSet);

            if (!MasterAudio.IsSoundGroupPlaying(_movingSound))
                MasterAudio.PlaySound3DFollowTransform(_movingSound, CampaignManager.AudioListenerTransform);
        }
    }

    private void HandlePlayerMovement()
    {
        _Movement = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if (_Movement != Vector2.zero)
        {
            SwapPositionForFacing();
            
            _Facing     = _Movement;
            _Movement   = _CurrentAnimationSet.Snap(_Movement);

            var speed       = _walkSpeed;
            var animSpeed   = 1f;
            _isRunning   = Input.GetButton("Fire3"); // Left Shift by default.
            if (_isRunning)
            {
                speed       = _runSpeed;
                animSpeed   = _runAnimationSpeed;
                MakeHorsesRun(_runAnimationSpeed);
            }
            else
                MakeHorsesWalk();

            // Disable Exterior Door Sprite -- only show when idle

            if (_exteriorDoor.IsActive())
                _exteriorDoor.SetActive(false);

            transform.Translate(_Movement.normalized * speed * Time.deltaTime);

            Play(_CarriageMoving, animSpeed);
        } else
        {
            Play(_CarriageIdles);
            MakeHorsesIdle();
        }
    }

    private void HandleAutoMovement()
    {
        if (_movementTarget.x != Vector2.negativeInfinity.x && _movementTarget.y != Vector2.negativeInfinity.y)
        {
            if (!_hasPathBeenSet)
            {

                var targetCell = (Vector2Int)WorldGrid.Instance.Grid.WorldToCell(_movementTarget);
                var path = GridUtility.FindPath(_renderer.sortingLayerID, GridPosition, targetCell);
                StartCoroutine(MovementCoroutine(path));

                _hasPathBeenSet = true;
            }
        } else
        {
            Play(_CarriageIdles);
            MakeHorsesIdle();
        }
    }

    private IEnumerator MovementCoroutine(GridPath path)
    {
        var nextPathGridPosition = GridPosition;
        var nextPathPosition = transform.position;
        var reachedGoal = false;

        var goal = path.Goal;


        if (WorldGrid.Instance[GridPosition].Unit != null)
            WorldGrid.Instance[GridPosition].Unit = null;


        DirectionalAnimationSet moveAnimation = _CarriageMoving;

        _isMoving = true;

        // Disable Exterior Door Sprite -- only show when idle
        if (_exteriorDoor.IsActive())
            _exteriorDoor.SetActive(false);

        while (!reachedGoal)
        {
            var speed = _walkSpeed * Time.deltaTime;
            var animSpeed = 1f;
            if (_isRunning)
            {
                speed       = _runSpeed * Time.deltaTime;
                animSpeed = _runAnimationSpeed;
                MakeHorsesRun(_runAnimationSpeed);
            }
            else
                MakeHorsesWalk();

            if (!_animancer.IsPlaying(moveAnimation.GetClip(_Facing)))
                Play(moveAnimation, animSpeed);

            while (speed > 0.0001f)
            {
                speed = MoveTo(nextPathPosition, speed);
                if (speed > 0.0001f)
                {
                    if (path.Length > 0)
                    {
                        // Get new destination position and direction
                        var newGridPosition = path.Pop();
                        var direction = GridUtility.GetDirection(nextPathGridPosition, newGridPosition, true);

                        nextPathPosition = WorldGrid.Instance.Grid.GetCellCenterWorld((Vector3Int)newGridPosition);
                        nextPathGridPosition = newGridPosition;

                        // Rotate
                        LookAt(direction);
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

        Play(_CarriageIdles);
        MakeHorsesIdle();

        GridPosition = goal;

        _isMoving = false;
        _movementTarget = Vector2.negativeInfinity;

        if (OnFinishedMoving != null)
            OnFinishedMoving.Invoke();

    }

    private float MoveTo(Vector2 goal, float speed)
    {
        if (speed <= 0.0001f)
            return 0;

        var distance = (transform.position - (Vector3)goal).magnitude;
        if (distance <= speed)
        {
            // Move to destination instantly
            transform.position = goal;
            speed -= distance;
        }
        else
        {
            var moveVector = ((Vector3)goal - transform.position).normalized * speed;
            transform.Translate(moveVector);
            speed = 0;
        }

        return speed;
    }

    private void SwapPositionForFacing()
    {
        var cachedPositions = GetCachedPositionsByDirection();

        SetColliders();

        var horseHolders    = _horseHolders;

        var horsePositions      = cachedPositions.Where((positions) => positions.GroupName == "Horses").First();
        var horseBarPositions   = cachedPositions.Where((positions) => positions.GroupName == "Horse Bars");

        if (horseBarPositions.Count() == 0)
            foreach (var horseBar in horseBarPositions)
                horseBar.SetActive(false);

        SetHorsesInPosition(horsePositions);

        if (horseBarPositions.Count() > 0)
            SetHorseBarsInPosition(horseBarPositions.First());
        else
            HideHorseHolders();

    }

    private void LookAt(Direction direction)
    {
        _Facing = direction.ToVector();

        var cachedPositions = GetCachedPositionsByDirection(direction);
        SetColliders();


        var horseHolders = _horseHolders;

        var horsePositions = cachedPositions.Where((positions) => positions.name.Contains("Horses")).First();
        var horseBarPositions = cachedPositions.Where((positions) => positions.name.Contains("Horse Bars"));

        if (horseBarPositions.Count() == 0)
            foreach (var horseBar in horseBarPositions)
                horseBar.SetActive(false);

        SetHorsesInPosition(horsePositions);

        if (horseBarPositions.Count() > 0)
            SetHorseBarsInPosition(horseBarPositions.First());
        else
            HideHorseHolders();

    }

    private void SetColliders()
    {
        if (_Facing.x > 0 || _Facing.x < 0)
        {
            _horizontalColliders.SetActive(true);
            _verticalColliders.SetActive(false);
        }

        if (_Facing.y > 0 || _Facing.y < 0)
        {
            _verticalColliders.SetActive(true);
            _horizontalColliders.SetActive(false);
        }
    }

    private void SetHorsesInPosition(CachedPositions cache)
    {
        var horses = new List<Horse>(_horses);

        var movedHorses = new List<Horse>();

        for (var i = 0; i < cache.Positions.Count; i++)
        {
            var horsePos = cache.Positions[i];
            var horse = horses[i];

            horse.transform.position = horsePos.position;
            horse.Rotate(cache.Direction);
            horse.PlayCurrentAnimSet();
            horse.SetActive(true);

            if (_Facing.y > 0 || _Facing.y < 0)
                horse.HideHarnessStrap();
            else
                horse.ShowHarnessStrap();

            movedHorses.Add(horse);
        }

        foreach (var horse in horses.Except(movedHorses))
            horse.SetActive(false);
    }

    private void SetHorseBarsInPosition(CachedPositions cache)
    {
        var horseBars = new List<GameObject>(_horseHolders);

        for(var i = 0; i < cache.Positions.Count; i++)
        {
            var horseBarPos = cache.Positions[i];

            if (horseBarPos.name == "Front Bar")
            {
                var horseBar = horseBars.Where((bar) => bar.name == "Front Bar").First();
                horseBar.transform.position = horseBarPos.position;
                horseBar.SetActive(true);
            } else
            {
                var horseBar = horseBars.Where((bar) => bar.name == "Back Bar").First();
                horseBar.transform.position = horseBarPos.position;
                horseBar.SetActive(true);
            }
        }
    }

    private List<CachedPositions> GetCachedPositionsByDirection()
    {
        var cachedPositions = new List<CachedPositions>();

        if (_Movement == Direction.Left.ToVector())
            return _spritePositions.Where((pos) => pos.Direction == Direction.Left).ToList();
        
        if (_Movement == Direction.Up.ToVector())
            return _spritePositions.Where((pos) => pos.Direction == Direction.Up).ToList();
        
        if (_Movement == Direction.Down.ToVector())
            return _spritePositions.Where((pos) => pos.Direction == Direction.Down).ToList();
        
        if (_Movement == Direction.Right.ToVector())
            return _spritePositions.Where((pos) => pos.Direction == Direction.Right).ToList();

        return cachedPositions;
    }

    private List<CachedPositions> GetCachedPositionsByDirection(Direction direction)
    {
        var cachedPositions = new List<CachedPositions>();

        if (direction == Direction.Left)
            return _spritePositions.Where((pos) => pos.Direction == Direction.Left).ToList();

        if (direction == Direction.Up)
            return _spritePositions.Where((pos) => pos.Direction == Direction.Up).ToList();

        if (direction == Direction.Down)
            return _spritePositions.Where((pos) => pos.Direction == Direction.Down).ToList();

        if (direction == Direction.Right)
            return _spritePositions.Where((pos) => pos.Direction == Direction.Right).ToList();

        return cachedPositions;
    }

    private void MakeHorsesRun(float animSpeed)
    {
        foreach (var horse in _horses)
            horse.Run(_Facing, animSpeed);
    }

    private void MakeHorsesWalk()
    {
        foreach (var horse in _horses)
            horse.Walk(_Facing);
    }

    private void MakeHorsesIdle()
    {
        foreach (var horse in _horses)
            horse.SetIdle();
    }

    private void HideHorseHolders()
    {
        foreach (var holder in _horseHolders)
            holder.SetActive(false);
    }

    public bool IsPlaying => Application.IsPlaying(this);
}
