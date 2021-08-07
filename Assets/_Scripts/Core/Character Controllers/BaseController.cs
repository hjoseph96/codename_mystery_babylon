using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Sirenix.OdinInspector;
using Animancer;

[RequireComponent(typeof(CapsuleCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class BaseController : MonoBehaviour
{

    [FoldoutGroup("Basic Information")]
    [SerializeField] protected float _WalkSpeed = 1;
    [FoldoutGroup("Basic Information")]
    [SerializeField] protected float _RunSpeed = 2;
    [FoldoutGroup("Basic Information")]
    [SerializeField] protected float _JumpDuration = 1.6f;
    [FoldoutGroup("Basic Information")]
    [SerializeField] protected bool _isSeated;
    public bool IsSeated { get => _isSeated; }

    protected bool _isIncapacitated;
    public bool IsIncapacitated { get => _isIncapacitated; }


    protected bool _isDying = false;
    protected bool _isDead;
    public bool IsDead { get => _isDead; }

    [FoldoutGroup("Animations"), SerializeField, ValueDropdown("ValidDirections")]
    protected Direction _StartingLookDirection;

    private List<Direction> ValidDirections = new List<Direction>
    {
        Direction.Down,
        Direction.Left,
        Direction.Up,
        Direction.Right
    };

    [FoldoutGroup("Animations")]
    [SerializeField, ReadOnly] protected Vector2 _Facing = Vector2.down;
    public Vector2 Facing { get => _Facing; }

    [FoldoutGroup("Animations")]
    protected AnimancerComponent _Animancer;
    [FoldoutGroup("Animations")]
    public DirectionalAnimationSet _Idle;
    [FoldoutGroup("Animations")]
    public DirectionalAnimationSet _Walk;
    [FoldoutGroup("Animations")]
    public DirectionalAnimationSet _Run;
    [FoldoutGroup("Animations")]
    public DirectionalAnimationSet _Push;
    [FoldoutGroup("Animations")]
    public DirectionalAnimationSet _Jump;
    [FoldoutGroup("Animations")]
    public DirectionalAnimationSet _InAir;
    [FoldoutGroup("Animations")]
    public DirectionalAnimationSet _Landing;
    [FoldoutGroup("Animations")]
    public DirectionalAnimationSet _Incapacitated;
    [FoldoutGroup("Animations")]
    public DirectionalAnimationSet _Death;
    [FoldoutGroup("Animations")]
    public DirectionalAnimationSet _Dead;
    [FoldoutGroup("Animations")]
    public DirectionalAnimationSet _StayIncapacitated;
    [FoldoutGroup("Animations")]
    public DirectionalAnimationSet _HelmetDrop;
    [FoldoutGroup("Animations"), ShowIf("CanDropHelmet")]
    public Transform HelmetSpawnPoint;
    private bool CanDropHelmet => _HelmetDrop != null;

    [FoldoutGroup("Animations")]
    [Header("Seated")]
    public DirectionalAnimationSet _Sitting;
    [FoldoutGroup("Animations")]
    public DirectionalAnimationSet _Sitting_Turn_Head_Down;
    [FoldoutGroup("Animations")]
    public DirectionalAnimationSet _Sitting_Turn_Head_Left;
    [FoldoutGroup("Animations")]
    public DirectionalAnimationSet _Sitting_Turn_Head_Right;
    [FoldoutGroup("Animations")]
    public DirectionalAnimationSet _Sitting_Turn_Head_Up;

    private SpriteRenderer _renderer;
    public SpriteRenderer Renderer { get => _renderer; }

    protected CapsuleCollider2D _Collider;
    protected Rigidbody2D _Rigidbody;

    protected FootstepController _FootstepController;
    protected JumpController _JumpController;

    protected Vector2 _Movement;
    protected DirectionalAnimationSet _CurrentAnimationSet;
    protected TimeSynchronisationGroup _MovementSynchronisation;

    // Start is called before the first frame update
    protected virtual void Awake()
    {
        _Animancer = GetComponent<AnimancerComponent>();
        _MovementSynchronisation = new TimeSynchronisationGroup(_Animancer) { _Walk, _Run, _Jump, _InAir, _Landing, _Sitting, _Push, _Incapacitated, _StayIncapacitated };

        _Collider = GetComponent<CapsuleCollider2D>();
        _Rigidbody = GetComponent<Rigidbody2D>();
        _renderer = GetComponent<SpriteRenderer>();

        _FootstepController = GetComponent<FootstepController>();
        _JumpController = GetComponent<JumpController>();
    }

    public void Rotate(Direction direction) => _Facing = direction.ToVector();

    public void RemoveIncapacitation() => _isIncapacitated = false;

    public void SetSeated() => _isSeated = true;

    public void StopSitting() => _isSeated = false;


    protected float MoveTo(Vector2 goal, float speed)
    {
        if (speed <= 0.0001f)
            return 0;

        var distance = Vector2.Distance(transform.position, goal);
        if (distance <= speed)
        {
            // Move to destination instantly
            transform.position = goal;
            speed -= distance;
        }
        else
        {
            _Movement = ((Vector3)goal - transform.position).normalized * speed;

            _Movement = _Walk.Snap(_Movement);

            transform.Translate(_Movement);
            speed = 0;
        }

        return speed;
    }

    protected float MoveToCell(Vector2 goal, float speed)
    {
        if (speed <= 0.0001f)
        {
            return 0;
        }

        var goalGridPos = WorldGrid.Instance.Grid.WorldToCell(goal);
        var goalWorldPos = WorldGrid.Instance.Grid.GetCellCenterWorld(goalGridPos);

        var distance = (transform.position - goalWorldPos).magnitude;
        if (distance <= speed)
        {
            // Move to destination instantly
            transform.position = goalWorldPos;
            speed -= distance;
        }
        else
        {
            _Movement = (goalWorldPos - transform.position).normalized * speed;

            _Movement = _Walk.Snap(_Movement);

            transform.Translate(_Movement);
            speed = 0;
        }

        return speed;
    }

}
