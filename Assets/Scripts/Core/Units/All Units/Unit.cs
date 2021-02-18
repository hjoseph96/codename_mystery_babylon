using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Animancer;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;


[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
public class Unit : SerializedMonoBehaviour, IInitializable
{
    [FoldoutGroup("Basic properties")]
    [DistinctUnitType]
    public UnitType UnitType;

    [FoldoutGroup("Basic properties")] 
    [SerializeField] private float _moveSpeed = 4f;
    [FoldoutGroup("Basic properties")] 
    [SerializeField] private float _moveAnimationSpeed = 1.75f;

    [FoldoutGroup("Base Stats")]
    [UnitStats, OdinSerialize, HideIf("IsPlaying")]
    private Dictionary<UnitStat, EditorStat> _statsDictionary = new Dictionary<UnitStat, EditorStat>();

    [FoldoutGroup("Stats"), ShowIf("IsPlaying"), PropertyOrder(0)]
    [ProgressBar(0, "MaxHealth", ColorGetter = "HealthColor", BackgroundColorGetter = "BackgroundColor", Height = 20)]
    public int CurrentHealth;

    [FoldoutGroup("Stats"), ShowIf("IsPlaying"), PropertyOrder(1)]
    [UnitStats]
    public Dictionary<UnitStat, Stat> Stats;

    [FoldoutGroup("Animations")] 
    [SerializeField] private DirectionalAnimationSet _idleAnimation;
    [FoldoutGroup("Animations")]
    [SerializeField] private DirectionalAnimationSet _walkAnimation;

    [FoldoutGroup("Items")]
    [SerializeField] private ScriptableItem[] _startingItems;

    public UnitInventory Inventory { get; private set; }
    public Weapon EquippedWeapon { get; private set; }
    public bool HasWeapon => EquippedWeapon != null;

    [HideInInspector] public Vector2Int GridPosition;

    [HideInInspector] public int CurrentMovementPoints;

    private Color HealthColor = new Color(0.25f, 1.0f, 0.35f);
    private Color BackgroundColor = Color.black;
    private int MaxHealth => Stats[UnitStat.MaxHealth].ValueInt;

    public int TeamId => Player.TeamId;
    public bool IsLocalPlayerUnit => Player == Player.LocalPlayer;

    protected virtual Player Player { get; }

    private AnimancerComponent _animancer;
    private Vector2 _lookDirection;

    public void Init()
    {
        GridPosition = GridUtility.SnapToGrid(this);
        WorldGrid.Instance[GridPosition].Unit = this;

        Player.AddUnit(this);

        InitStats();

        _animancer = GetComponent<AnimancerComponent>();
        Rotate(Direction.Down);
        PlayAnimation(_idleAnimation);

        Inventory = new UnitInventory();
        foreach (var item in _startingItems)
            Inventory.AddItem(item.GetItem());

        var weapons = Inventory.GetItems<Weapon>();
        if (weapons.Length > 0)
            EquipWeapon(weapons[0]);
    }

    public bool IsAlly(Unit unit) => Player.IsAlly(unit.Player);

    public bool IsEnemy(Unit unit) => !IsAlly(unit);

    public void LookAt(Vector2 position)
    {
        _lookDirection = position - (Vector2) transform.position;
    }

    public void Rotate(Direction direction)
    {
        LookAt((Vector2) transform.position + direction.ToVector());
    }

    public void PlayAnimation(DirectionalAnimationSet animations, float speed = 1f)
    {
        var clip = animations.GetClip(_lookDirection);
        _animancer.Play(clip).Speed = speed;
    }

    public void EquipWeapon(Weapon weapon)
    {
        EquippedWeapon = weapon;
    }

    public void UnequippedWeapon()
    {
        EquippedWeapon = null;
    }

    public bool CanAttack() {
        var immediatePositions = GridUtility.GetReachableCells(this, 0);
        var attackableCells = GridUtility.GetAttackableCells(this, immediatePositions);
        if (attackableCells.Count > 0)
            return true;
        
        return false;
    }

    public bool Trade(Unit otherUnit, Item ourItem)
    {
        if (otherUnit.Inventory.IsFull || !Inventory.HasItem(ourItem))
            return false;

        Inventory.RemoveItem(ourItem);
        otherUnit.Inventory.AddItem(ourItem);

        return true;
    }

    public bool Trade(Unit otherUnit, Item ourItem, Item theirItem)
    {
        if (!Inventory.HasItem(ourItem) || !otherUnit.Inventory.HasItem(theirItem))
            return false;

        Inventory.RemoveItem(ourItem);
        otherUnit.Inventory.RemoveItem(theirItem);

        Inventory.AddItem(theirItem);
        otherUnit.Inventory.AddItem(ourItem);

        return true;
    }

    public IEnumerator MovementCoroutine(GridPath path)
    {
        var nextPathGridPosition = GridPosition;
        var nextPathPosition = transform.position;
        var reachedGoal = false;

        var goal = path.Goal;
        WorldGrid.Instance[GridPosition].Unit = null;

        PlayAnimation(_walkAnimation, _moveAnimationSpeed);

        while (!reachedGoal)
        {
            var speed = _moveSpeed * Time.deltaTime;

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
                        Rotate(direction);
                        PlayAnimation(_walkAnimation, _moveAnimationSpeed);
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

        PlayAnimation(_idleAnimation);

        GridPosition = goal;
        WorldGrid.Instance[GridPosition].Unit = this;
    }

    private float MoveTo(Vector2 goal, float speed)
    {
        if (speed <= 0.0001f)
        {
            return 0;
        }

        var distance = (transform.position - (Vector3) goal).magnitude;
        if (distance <= speed)
        {
            // Move to destination instantly
            transform.position = goal;
            speed -= distance;
        }
        else
        {
            var moveVector = ((Vector3) goal - transform.position).normalized * speed;
            transform.Translate(moveVector);
            speed = 0;
        }

        return speed;
    }

    private void InitStats()
    {
        Stats = new Dictionary<UnitStat, Stat>();

        // Copy base stats
        foreach (var stat in _statsDictionary.Keys)
        {
            var value = _statsDictionary[stat];
            Stats[stat] = new Stat(stat.ToString(), value.Value, value.GrowthRate);
            //Stats[stat].AddEffect(new FlatBonusEffect(10));
            //Stats[stat].AddEffect(new PercentageBonusEffect(0.2f));
            //if (stat != UnitStat.MaxHealth)
            //    Stats[stat].AddEffect(new DependOnOtherStatEffect(Stats[UnitStat.MaxHealth], 0.1f));
        }

        CurrentMovementPoints = Stats[UnitStat.Movement].ValueInt;
        CurrentHealth = Stats[UnitStat.MaxHealth].ValueInt;
    }

    private static bool IsPlaying => Application.isPlaying;
}