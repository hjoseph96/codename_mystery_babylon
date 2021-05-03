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
    [FoldoutGroup("Basic Properties"), SerializeField]
    private float _walkSpeed;
    [FoldoutGroup("Basic Properties"), SerializeField]
    private float _runSpeed;
    [FoldoutGroup("Basic Properties"), SerializeField]
    private float _runAnimationSpeed = 1.5f;
    [FoldoutGroup("Basic Properties"), SerializeField, HideIf("IsPlaying")]
    private CarriageControlMode _ControlMode;
    [FoldoutGroup("Basic Properties"), ShowInInspector, ShowIf("IsPlaying")]
    public CarriageControlMode ControlMode { get => _ControlMode; }



    [FoldoutGroup("Animations"), SerializeField, ValueDropdown("ValidDirections")]
    private Direction startingLookDirection;

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
        { Direction.Right,   Vector2.right },
    };

    [FoldoutGroup("Animations")]
    [SerializeField] private DirectionalAnimationSet _CarriageIdles;
    [FoldoutGroup("Animations")]
    [SerializeField] private DirectionalAnimationSet _CarriageMoving;

    [FoldoutGroup("Audio"), SerializeField, SoundGroup]
    private string _movingSound;



    private List<Horse> _horses = new List<Horse>();
    [FoldoutGroup("References")]
    [ShowInInspector]
    public List<Horse> Horses { get => _horses; }

    [FoldoutGroup("References"), SerializeField]
    private List<GameObject> _horseHolders = new List<GameObject>();
    public List<GameObject> WoodenHorseHolders { get => _horseHolders; }

    private TimeSynchronisationGroup _MovementSynchronisation;
    private DirectionalAnimationSet _CurrentAnimationSet;
    private AnimancerComponent _animancer;
    private Vector2 _Movement;
    private Vector2 _Facing = Vector2.down;

    private List<CachedPositions> _spritePositions = new List<CachedPositions>();


    // Start is called before the first frame update
    void Start()
    {
        foreach (var horse in GetComponentsInChildren<Horse>())
            _horses.Add(horse);

        _animancer = GetComponent<AnimancerComponent>();
        _MovementSynchronisation = new TimeSynchronisationGroup(_animancer) { _CarriageIdles, _CarriageMoving };

        foreach (var cachedPosition in GetComponentsInChildren<CachedPositions>())
            _spritePositions.Add(cachedPosition);

        Play(_CarriageIdles);
    }

    // Update is called once per frame
    void Update()
    {
        switch (ControlMode)
        {
            case CarriageControlMode.Auto:
                break;
            case CarriageControlMode.Player:
                HandlePlayerMovement();
                break;
        }
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
            var isRunning   = Input.GetButton("Fire3"); // Left Shift by default.
            if (isRunning)
            {
                speed       = _runSpeed;
                animSpeed   = _runAnimationSpeed;
                MakeHorsesRun(_runAnimationSpeed);
            }
            else
                MakeHorsesWalk();



            transform.Translate(_Movement.normalized * speed * Time.deltaTime);

            Play(_CarriageMoving, animSpeed);
        } else
        {
            Play(_CarriageIdles);
            MakeHorsesIdle();
        }
    }

    private void SwapPositionForFacing()
    {
        var cachedPositions = GetCachedPositionsByDirection();

        var horseHolders    = _horseHolders;

        var horsePositions      = cachedPositions.Where((positions) => positions.GroupName == "Horses").First();
        var horseBarPositions   = cachedPositions.Where((positions) => positions.GroupName == "Horse Bars");

        if (horseBarPositions.Count() == 0)
            foreach (var horseBar in horseBarPositions)
                horseBar.SetActive(false);

        if (_Movement == Direction.Up.ToVector())
            SetHorsesOrderInLayer(20);
        else
            SetHorsesOrderInLayer(24);

        SetHorsesInPosition(horsePositions);

        if (horseBarPositions.Count() > 0)
            SetHorseBarsInPosition(horseBarPositions.First());
        else
            HideHorseHolders();


    }

    private void SetHorsesOrderInLayer(int orderInLayer)
    {
        foreach (var horse in _horses)
            horse.SetOrderInLayer(orderInLayer);
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

    private void ShowHorseHolders()
    {
        foreach (var holder in _horseHolders)
            holder.SetActive(true);
    }

    public bool IsPlaying => Application.IsPlaying(this);
}
