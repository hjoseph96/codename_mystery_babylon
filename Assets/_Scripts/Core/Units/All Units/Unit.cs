using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using Animancer;

using Sirenix.OdinInspector;
using Sirenix.Serialization;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AnimancerComponent))]
[RequireComponent(typeof(DayLightCollider2D))]
[RequireComponent(typeof(AllIn1Shader))]
[RequireComponent(typeof(FootstepController))]
[RequireComponent(typeof(SpriteFlashTool))]
[RequireComponent(typeof(SpriteFlashTool))]
[RequireComponent(typeof(UnitAttackAnimations))]
[RequireComponent(typeof(Portrait))]
public class Unit : SerializedMonoBehaviour, IInitializable
{
    #region Basic Properties
    [FoldoutGroup("Basic Properties")]
    [SerializeField] private bool _isInCombat;
    public bool IsInCombat { get => _isInCombat; }

    [FoldoutGroup("Basic Properties")]
    [SerializeField] private string _name;
    public string Name => _name;

    [FoldoutGroup("Basic Properties")]
    [DistinctUnitType] public UnitType UnitType;

    [FoldoutGroup("Basic Properties")]
    [SerializeField] private bool _unkillable;
    public bool Unkillable { get => _unkillable; }

    [FoldoutGroup("Basic Properties")]
    [SerializeField] private float _walkSpeed = 4f;
    public float WalkSpeed { get => _walkSpeed; }

    [FoldoutGroup("Basic Properties")]
    [SerializeField] private float _runSpeed = 4f;
    public float RunSpeed { get => _runSpeed; }

    [FoldoutGroup("Basic Properties")]
    [SerializeField] private float _moveAnimationSpeed = 1.75f;
    public float MoveAnimationSpeed { get => _moveAnimationSpeed; }

    [FoldoutGroup("Basic Properties")]
    [SerializeField] private Direction _startingLookDirection;

    [FoldoutGroup("Basic Properties")]
    public GameObject BattlerPrefab;

    [FoldoutGroup("Basic Properties")]
    [ReadOnly] public Portrait Portrait;

    [FoldoutGroup("Basic Properties")]
    public bool IsAlive => CurrentHealth > 0;

    [FoldoutGroup("Basic Properties")]
    [ReadOnly] private bool _isIncapacitated;
    public bool Incapacitated { get => _isIncapacitated; }
    #endregion

    #region Cheat Variables
    //Cheat variables
    private float defaultMoveAnimationSpeed;
    private float defaultRunSpeed;
    private float defaultWalkSpeed;
    #endregion

    #region Animations
    [FoldoutGroup("Animations")]
    [SerializeField] protected DirectionalAnimationSet _idleAnimation;

    [FoldoutGroup("Animations")]
    [SerializeField] protected DirectionalAnimationSet _walkAnimation;

    [FoldoutGroup("Animations")]
    [SerializeField] protected DirectionalAnimationSet _runAnimation;

    [FoldoutGroup("Animations")]
    [SerializeField] protected DirectionalAnimationSet _drinkPotionAnimation;

    [FoldoutGroup("Animations")]
    [SerializeField] protected DirectionalAnimationSet _Jump;

    [FoldoutGroup("Animations")]
    [SerializeField] protected DirectionalAnimationSet _InAir;

    [FoldoutGroup("Animations")]
    [SerializeField] protected DirectionalAnimationSet _Landing;

    [FoldoutGroup("Animations")]
    [SerializeField] protected DirectionalAnimationSet _Damage;

    [FoldoutGroup("Animations")]
    [SerializeField] protected DirectionalAnimationSet _Death;

    [FoldoutGroup("Animations")]
    [SerializeField] protected DirectionalAnimationSet _Dead;


    [FoldoutGroup("Animations"), ShowIf("Unkillable")]
    [SerializeField] protected DirectionalAnimationSet _Incapacitated;

    [FoldoutGroup("Animations"), ShowIf("Unkillable")]
    [SerializeField] protected DirectionalAnimationSet _StayIncapacited;
    #endregion

    #region Stats
    public static int MAX_LEVEL = 40;
    [FoldoutGroup("Base Stats")]
    [SerializeField] private int _level;
    public int Level => _level;

    [FoldoutGroup("Base Stats")]
    public ScriptableUnitClass UnitClass;

    private UnitClass _unitClass;
    [FoldoutGroup("Base Stats")]
    public UnitClass Class => _unitClass;

    [FoldoutGroup("Base Stats")]
    [SerializeField] private int _experience;
    public int Experience => _experience;
    public static int MAX_EXP_AMOUNT = 100;

    public bool IsLeader;
    // TODO: Leadership rank and influence radius

    [FoldoutGroup("Base Stats")]
    [UnitStats, OdinSerialize, HideIf("IsInBattle")]
    private Dictionary<UnitStat, EditorStat> _statsDictionary = new Dictionary<UnitStat, EditorStat>();
    public Dictionary<UnitStat, EditorStat> EditorStats { get => _statsDictionary; }

    [FoldoutGroup("Stats"), ShowIf("IsInBattle"), PropertyOrder(0)]
    [ProgressBar(0, "MaxHealth", ColorGetter = "HealthColor", BackgroundColorGetter = "BackgroundColor", Height = 20)]
    [SerializeField] protected int currentHealth;
    public int CurrentHealth => currentHealth;

    public bool IsInBattle()
    {
        if (!IsPlaying) return false;

        var campaignManager = CampaignManager.Instance;
        if (campaignManager == null) return false;

        return campaignManager.IsInCombat;
    }

    [FoldoutGroup("Stats"), ShowIf("IsPlaying"), PropertyOrder(1)]
    [UnitStats]
    public Dictionary<UnitStat, Stat> Stats;

    #endregion

    #region Items
    [FoldoutGroup("Items")]
    [SerializeField] private ScriptableItem[] _startingItems;

    public UnitInventory Inventory { get; private set; }

    [FoldoutGroup("Items")]
    // Temporary measure until Playerunit have Stats serialized
    [SerializeField] private WeaponRank _startingRank = WeaponRank.D;

    private Dictionary<WeaponType, WeaponRank> _weaponProfiency = new Dictionary<WeaponType, WeaponRank>();
    [FoldoutGroup("Items")]
    [ShowInInspector] public Dictionary<WeaponType, WeaponRank> WeaponProfiency => _weaponProfiency;

    private Dictionary<MagicType, WeaponRank> _magicProfiency = new Dictionary<MagicType, WeaponRank>();
    [FoldoutGroup("Items")]
    [ShowInInspector] public Dictionary<MagicType, WeaponRank> MagicProfiency => _magicProfiency;
    #endregion

    #region GameStatus
    [FoldoutGroup("Game Status")]
    public int TeamId => Player.TeamId;

    private bool _hasEscaped;
    [FoldoutGroup("Game Status"), ShowInInspector]
    public bool HasEscaped { get => _hasEscaped; }

    [FoldoutGroup("Game Status")]
    private Weapon _equippedWeapon;

    [FoldoutGroup("Game Status")]
    [ShowInInspector] public Weapon EquippedWeapon => _equippedWeapon; // TODO: Implement Gear. EquippedGear. one 1 Gear equipped per unit (ie. shield)
    public bool HasWeapon => EquippedWeapon != null;

    private Vector2Int _gridPosition;

    /// <summary>
    /// This returns the position the unit is physically confirmed at. <br></br>To use variables utilizing giving commands to a unit, use GridCursor.Instance.GridPosition)
    /// </summary>
    [FoldoutGroup("Game Status")]
    [ShowInInspector]
    public Vector2Int GridPosition {
        get { return _gridPosition; }
        private set {
            _gridPosition = value;
        }
    }

    public List<Vector2Int> CellsWithMyEffects;

    // Key == GridPosition && Value == _lookDirection
    private KeyValuePair<Vector2Int, Vector2> _initialGridPosition;
    public KeyValuePair<Vector2Int, Vector2> InitialGridPosition => _initialGridPosition; // Where Unit began the turn.

    private bool _hasTakenAction;
    [FoldoutGroup("Game Status")]
    [ShowInInspector] public bool HasTakenAction => _hasTakenAction; // Has the unit moved this turn?

    protected int currentMovementPoints;
    protected int leftoverMovementPoints;
    [FoldoutGroup("Game Status")]
    [ShowInInspector] public int CurrentMovementPoints => currentMovementPoints;

    private bool _canJump = false;
    [FoldoutGroup("Game Status")]
    public bool CanJump { get => _canJump; }

    public virtual Vector2Int PreferredDestination { get => GridPosition; }
    #endregion

#if UNITY_EDITOR
    [Button("Save As JSON")]
    private void SerializeUnit() => UnitRepository.Write(this);
#endif

    #region Events
    [HideInInspector] public Action OnFinishedMoving;
    [HideInInspector] public Action<Unit> UponDeath;
    [HideInInspector] public Action<Unit> UponLevelUp;
    [HideInInspector] public Action UponJumpLanding;

    // On Map Battle System events
    [HideInInspector] public Action UponAttackLaunched;
    [HideInInspector] public Action UponAttackAnimationEnd;
    [HideInInspector] public Action UponAttackComplete;
    [HideInInspector] public Action UponDodgeComplete;
    [HideInInspector] public Action UponDamageCalcComplete;

    [HideInInspector] public Action UponHealComplete;
    [HideInInspector] public Action<Vector2> OnLookDirectionChanged;
    #endregion

    #region Stat Getters
    // Stat Convenience Methods
    public int Weight => Stats[UnitStat.Weight].ValueInt;
    public int Strength => Stats[UnitStat.Strength].ValueInt;
    public int Skill => Stats[UnitStat.Skill].ValueInt;
    public int Resistance => Stats[UnitStat.Resistance].ValueInt;
    public int MaxHealth => Stats[UnitStat.MaxHealth].ValueInt;
    public int Magic => Stats[UnitStat.Magic].ValueInt;
    public int Luck => Stats[UnitStat.Luck].ValueInt;
    public int Defense => Stats[UnitStat.Defense].ValueInt;
    public int Constitution => Stats[UnitStat.Constitution].ValueInt;
    public int Speed => Stats[UnitStat.Speed].ValueInt;
    public int MaxMoveRange => Stats[UnitStat.Movement].ValueInt;
    public int Morale => Stats[UnitStat.Morale].ValueInt;
    #endregion

    #region Misc
    public bool IsLocalPlayerUnit => Player == Player.LocalPlayer;
    public int SortingLayerId => _renderer.sortingLayerID;
    public int OrderInLayer => _renderer.sortingOrder;

    protected virtual Player Player { get; }
    protected Battler Battler;
    protected bool isMoving;

    public void UpdateHealthBar() => _healthBar.Refresh(this);

    protected List<Unit> PotentialThreats = new List<Unit>();

    private Vector2 _lookDirection;
    public Vector2 LookDirection { get => _lookDirection; }

    protected AnimancerComponent _animancer;
    private SpriteFlashTool _flasher;
    private DayLightCollider2D _dayLightCollider;

    private FootstepController _footstepController;
    private JumpController _jumpController;
    private JumpTrigger _jumpTrigger;
    protected UnitAttackAnimations _attackAnimations;

    private MiniHealthBar _healthBar;
    public MiniHealthBar HealthBar { get => _healthBar; }

    public CombatText combatText;

    // Mecanim AnimationEvent Listeners
    private AnimationEventReceiver _OnPlayFootsteps;
    private AnimationEventReceiver _OnAttackLaunched;
    private AnimationEventReceiver _OnAttackFinished;

    protected SpriteRenderer _renderer;

    private Material _allInOneMat;
    private static readonly int StencilRef = Shader.PropertyToID("_StencilRef");

    private Direction _facingDirection;
    private Vector2 _dodgePosition;
    private Vector2 _dodgeReturnPosition;

    protected bool isDodging = false;
    public bool IsDodging { get => isDodging; }

    protected bool isDying = false;
    public bool IsDying { get => isDying; }

    protected bool isHitReacting = false;
    public bool IsHitReacting { get => isHitReacting; }

    private bool _didTrade;
    public bool DidTrade { get => _didTrade; }
    private bool _isMounted;
    private bool _performedSecondMove;
    /// <summary>
    /// Used exclusively for giving player Game Over if they die
    /// </summary>
    protected bool _importantUnit;
    public bool ImportantUnit { get => _importantUnit; }
    #endregion

    #region CheatFunctions
    /// <summary>
    /// This is a cheat feature to speed up animations for testing 
    /// </summary>
    /// <param name="obj"></param>
    private void AdjustSpeed(float obj)
    {
        _moveAnimationSpeed = obj * defaultMoveAnimationSpeed;
        _runSpeed = obj * defaultRunSpeed;
        _walkSpeed = obj * defaultWalkSpeed;
        _animancer.Playable.Speed = obj;
    }
    #endregion

    #region Initializers
    public virtual void Init()
    {
        //Cheat Variables
        GlobalVariables.Instance.OnGameSpeedChanged += AdjustSpeed;
        defaultMoveAnimationSpeed = _moveAnimationSpeed;
        defaultRunSpeed = RunSpeed;
        defaultWalkSpeed = WalkSpeed;

        // Snap to tile position
        SetGridPosition(GridUtility.SnapToGrid(this));

        // Get basic unit details
        _unitClass = UnitClass.GetUnitClass();
        _unitClass.StatusEffects.ForEach((se) => { se.SetOwnerName(Name); });

        // Init stats based on basic unit details
        InitStats();

        // Reference Collection
        _renderer = GetComponent<SpriteRenderer>();
        _animancer = GetComponent<AnimancerComponent>();
        _footstepController = GetComponent<FootstepController>();
        _jumpController = GetComponent<JumpController>();
        _attackAnimations = GetComponent<UnitAttackAnimations>();
        _healthBar = GetComponentInChildren<MiniHealthBar>(true);
        _flasher = GetComponent<SpriteFlashTool>();
        Portrait = GetComponent<Portrait>();
        _dayLightCollider = GetComponent<DayLightCollider2D>();
        _allInOneMat = GetComponent<Renderer>().material;
        if (combatText == null)
            combatText = transform.FindDeepChild("CombatText").GetComponent<CombatText>();
        combatText.SetRef(this);

        // Rotate to starting look direction
        Rotate(_startingLookDirection);

        // Save starting position
        _initialGridPosition = new KeyValuePair<Vector2Int, Vector2>(GridPosition, _lookDirection);

        // Play starting Animation
        PlayAnimation(_idleAnimation);

        // Enable effects
        EnableSilhouette();

        // TODO: load from serialization for PlayerUnits 
        // Assign starting items
        Inventory = new UnitInventory(this);
        foreach (var item in _startingItems)
            Inventory.AddItem(item.GetItem());

        var weapons = Inventory.GetItems<Weapon>();
        if (weapons.Length > 0)
            EquipWeapon(weapons[0]);
    }

    /// <summary>
    /// Assigns starting stats, and populates proficiency lists, Sets movement range from stat value and populates health data
    /// </summary>
    private void InitStats()
    {
        // TODO: Setup serialization of Unit Stats for PlayerUnits
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

        currentMovementPoints = Stats[UnitStat.Movement].ValueInt;
        currentHealth = MaxHealth;

        foreach (var weaponType in Class.UsableWeapons)
            if (!_weaponProfiency.ContainsKey(weaponType))
                _weaponProfiency.Add(weaponType, _startingRank);

        foreach (var magicType in Class.UsableMagic)
            if (!_magicProfiency.ContainsKey(magicType))
                _magicProfiency.Add(magicType, _startingRank);
    }
    #endregion

    #region Checking Methods
    public bool IsAlly(Unit unit) => Player.IsAlly(unit.Player);

    public bool IsEnemy(Unit unit) => !IsAlly(unit);
    #endregion

    #region Movement and Positioning
    public void SetInitialPosition() => _initialGridPosition = new KeyValuePair<Vector2Int, Vector2>(GridPosition, _lookDirection);

    /// <summary>
    /// Used when canceling unit movement choices, sets currentMovementPoints to value based on if unit performed trade
    /// </summary>
    public void ResetToInitialPosition()
    {
        // Move back to Start instantly
        this.transform.position = WorldGrid.Instance.Grid.GetCellCenterWorld((Vector3Int)InitialGridPosition.Key);

        // current movement points are set at 0 if not mounted and did trade, 
        currentMovementPoints = _didTrade ? (_isMounted ? leftoverMovementPoints : 0) : MaxMoveRange;
        // Reset Original Look Direction
        LookAt(InitialGridPosition.Value);
        SetIdle();

        // Change Grid Position locally and within WorldGrid's WorldCell.
        SetGridPosition(InitialGridPosition.Key);
    }

    public void LookAt(Vector2 position)
    {
        _lookDirection = position - (Vector2)transform.position;
        OnLookDirectionChanged?.Invoke(_lookDirection);
    }

    public void Rotate(Direction direction)
    {
        LookAt((Vector2)transform.position + direction.ToVector());
    }

    public void SetGridPosition(Vector2Int newGridPosition)
    {
        if (WorldGrid.Instance.PointInGrid(GridPosition))
            WorldGrid.Instance[GridPosition].Unit = null;

        GridPosition = newGridPosition;
        WorldGrid.Instance[GridPosition].Unit = this;
    }

    /// <summary>
    /// Triggers post combat, or specific turn ending action. 
    /// </summary>
    public virtual void TookAction()
    {
        _hasTakenAction = true;
        SetInactiveShader();

        RemoveAuras(InitialGridPosition.Key);
        ApplyAuras(GridPosition);
        
        // Reset Did trade and performed second move to false to allow actions again this turn if unit somehow gets more APS
        _didTrade = false;
        _performedSecondMove = false;
    }

    /// <summary>
    /// Called at the beginning of a players turn, allowing all the units to take an action
    /// </summary>
    public virtual void AllowAction()
    {
        _hasTakenAction = false;
        RemoveInactiveShader();
        currentMovementPoints = MaxMoveRange;
        leftoverMovementPoints = currentMovementPoints;
    }

    public IEnumerator MovementCoroutine(GridPath path)
    {
        if (path == null || path.Length == 0)
            yield break;

        var nextPathGridPosition = GridPosition;
        var nextPathPosition = transform.position;
        var reachedGoal = false;

        var origin = GridPosition;
        var goal = path.Goal;


        if (WorldGrid.Instance[GridPosition].Unit != null)
            WorldGrid.Instance[GridPosition].Unit = null;


        DirectionalAnimationSet moveAnimation = _walkAnimation;

        isMoving = true;
        while (!reachedGoal)
        {
            var speed = _walkSpeed * Time.deltaTime;

            //TODO: Change this input getter to the input system
            if (this is PlayerUnit && Input.GetButton("Fire3")) // Left Shift by default.
            {
                moveAnimation = _runAnimation;
                _moveAnimationSpeed = 1;
                speed = _runSpeed * Time.deltaTime;
            }
            else
                moveAnimation = _walkAnimation;

            if (!_animancer.IsPlaying(moveAnimation.GetClip(_lookDirection)))
                PlayAnimation(moveAnimation, _moveAnimationSpeed);

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

                        currentMovementPoints -= 1;
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

        isMoving = false;
        
        if (_isMounted)
        {
            if (_didTrade)
            {
                _performedSecondMove = true;
                currentMovementPoints = 0;
            }
            leftoverMovementPoints = currentMovementPoints;
        }

        OnFinishedMoving?.Invoke();
    }

    private float MoveTo(Vector2 goal, float speed)
    {
        if (speed <= 0.0001f)
        {
            return 0;
        }

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

            moveVector = _walkAnimation.Snap(moveVector);

            moveVector = _walkAnimation.Snap(moveVector);

            transform.Translate(moveVector);
            speed = 0;
        }

        return speed;
    }

    #endregion




    #region Attacking and Weapons
    public bool CanWield(Weapon weapon)
    {
        if (weapon.Type == WeaponType.Grimiore)
        {
            if (!MagicProfiency.Keys.Contains(weapon.MagicType))
                return false;

            if (MagicProfiency[weapon.MagicType] >= weapon.RequiredRank)
                return true;
            else
                return false;
        }

        if (WeaponProfiency.Keys.Contains(weapon.Type) && WeaponProfiency[weapon.Type] >= weapon.RequiredRank)
            return true;
        else
            return false;
    }

    public bool CanAttackLongRange() => WieldableWeapons().Any((weapon) => weapon.MaxRange > 1);

    public List<Weapon> WieldableWeapons()
    {
        var wieldableWeapons = new List<Weapon>();

        foreach (Weapon inventoryWeapon in Inventory.GetItems<Weapon>())
            if (CanWield(inventoryWeapon))
                wieldableWeapons.Add(inventoryWeapon);

        return wieldableWeapons;
    }

    public void EquipWeapon(Weapon weapon)
    {
        var availableWeapons = new List<Weapon>();
        foreach (Weapon inventoryWeapon in Inventory.GetItems<Weapon>())
        {
            inventoryWeapon.CanWield = CanWield(inventoryWeapon);
            availableWeapons.Add(inventoryWeapon);
        }

        if (availableWeapons.Contains(weapon) && weapon.CanWield)
        {
            UnequipWeapon();
            _equippedWeapon = weapon;
            weapon.IsEquipped = true;
        }
        else
            throw new Exception($"Provided Weapon: {weapon.Name} is not in Unit#{Name}'s Inventory...");
    }

    public void UnequipWeapon()
    {
        if (EquippedWeapon != null)
            EquippedWeapon.IsEquipped = false;

        _equippedWeapon = null;
    }

    public bool IsArmed()
    {
        var wieldableWeapons = WieldableWeapons();

        if (wieldableWeapons.Count == 0)
            return false;

        return !wieldableWeapons.All((weapon) => weapon.IsBroken == true);
    }

    #endregion

    #region Animations and Visuals
    private void PlayAnimation(DirectionalAnimationSet animations, float speed = 1f)
    {
        var clip = animations.GetClip(_lookDirection);

        var state = _animancer.Play(clip);
        state.Speed = speed;

        if (animations == _walkAnimation || animations == _runAnimation)
            _OnPlayFootsteps.Set(state, PlayFootstepSound());

        if (animations == _Damage)
        {
            isHitReacting = true;
            state.Events.OnEnd += delegate ()
            {
                SetIdle();
                isHitReacting = false;
            };
        }

        if (animations == _Death)
            state.Events.OnEnd += delegate ()
            {
                RemoveSelf(_Dead);
            };

        if (animations == _Incapacitated)
            state.Events.OnEnd += delegate ()
            {
                _isIncapacitated = true;
                RemoveSelf(_StayIncapacited, false);
            };
    }

    private void RemoveSelf(DirectionalAnimationSet animToPlay, bool noShadow = true)
    {
        isDying = false;

        PlayAnimation(animToPlay);

        if (noShadow)
            _dayLightCollider.enabled = false;

        ClearOnMapBattleEvents();

        CampaignManager.Instance.RemoveUnit(this);

        UponDamageCalcComplete?.Invoke();
    }

    public void ChangeName(string newName) => _name = newName;

    public void Die() => PlayAnimation(_Death);

    public void Incapacitate() => PlayAnimation(_Incapacitated);

    public void SetIdle() => PlayAnimation(_idleAnimation);

    public void EnableSilhouette() => _allInOneMat.SetInt(StencilRef, 1);

    public void DisableSilhouette() => _allInOneMat.SetInt(StencilRef, 2);

    private void SetInactiveShader() => _allInOneMat.EnableKeyword("HSV_ON");

    private void RemoveInactiveShader() => _allInOneMat.DisableKeyword("HSV_ON");
    #endregion

    #region Health

    public void ShowHealthBar() => _healthBar.Show(this);

    public void HideHealthBar() => _healthBar.Hide();

    public void DecreaseHealth(int amount)
    {
        int damageTaken = amount;

        if (damageTaken >= CurrentHealth)
        {
            if (Unkillable)
            {
                currentHealth = 1;
                UponDeath?.Invoke(this);
                return;
            }

            damageTaken = CurrentHealth;
            UponDeath?.Invoke(this);
        }

        currentHealth -= damageTaken;
    }

    public void IncreaseHealth(int amount)
    {
        int currentHealthAmount = CurrentHealth + amount;
        if (currentHealthAmount > MaxHealth)
            currentHealthAmount = MaxHealth;

        currentHealth = currentHealthAmount;

        combatText.StartDisplay(CombatText.TextModifier.Heal, string.Format("{0}", amount));
    }

    #endregion

    #region Jump Mechanics
    public void Jump()
    {
        AttachJumpEvents();

        _jumpController.Jump(_jumpTrigger);
    }

    private void AttachJumpEvents()
    {
        _jumpController.OnBeginJump += delegate ()
        {
            PlayAnimation(_Jump);

            _jumpController.OnBeginJump = null;
        };

        _jumpController.UponLanding += delegate ()
        {
            PlayAnimation(_Landing);

            if (UponJumpLanding != null)
                UponJumpLanding.Invoke();

            _jumpController.UponLanding = null;
        };

        _jumpController.WhileInAir += delegate ()
        {
            PlayAnimation(_InAir);

            _jumpController.WhileInAir = null;
        };
    }

    public void AllowJumping(JumpTrigger jumpTrigger)
    {
        _jumpTrigger = jumpTrigger;
        _canJump = true;
    }

    public void DisableJumping()
    {
        _jumpTrigger = null;
        _canJump = false;
    }
    #endregion

    #region Actions and Limiters
    public virtual void GainExperience(int amount)
    {
        int currentExpAmount = _experience + amount;



        if (currentExpAmount >= MAX_EXP_AMOUNT)
        {
            UponLevelUp += delegate (Unit unit)
            {
                if (_level < MAX_LEVEL)
                    _level += 1;

                //Only show the Level Up screen if this is actually a player unit
                if(this is PlayerUnit && IsAlive)
                    LevelUpGUI.VisualEffects();
            };

            currentExpAmount -= MAX_EXP_AMOUNT;
            LevelUpGUI.BeginDisplay(this);
            //Swap to the Start coroutine function to slow down the upon level up allowing the gui to be delayed until the stats change with true random
            //StartCoroutine(StatUpgrades());
            StatUpgradesPlain();

        }
        _experience = currentExpAmount;
    }

    /// <summary>
    /// Coroutine function for upgrading stats using True random
    /// </summary>
    /// <returns></returns>
    private IEnumerator StatUpgrades()
    {
        //TODO: TEMPORARY STAT IMPROVEMENT Remove this for loop after testing
        for (int i = 0; i < Stats.Count; i++)
        {
            yield return StartCoroutine(Stats[(UnitStat)i].Grow(UnityEngine.Random.Range(0, 5)));
        }

        // This needs to be invoked after the stats change to allow the level up UI to get the new values 
        UponLevelUp?.Invoke(this);
    }

    /// <summary>
    /// Non Coroutine function of upgrading stats using a non Coroutine Grow Temporary testing function?
    /// </summary>
    private void StatUpgradesPlain()
    {
        //TODO: TEMPORARY STAT IMPROVEMENT Remove this for loop after testing
        for (int i = 0; i < Stats.Count; i++)
        {
            Stats[(UnitStat)i].GrowPlain(UnityEngine.Random.Range(0, 5));
        }

        UponLevelUp?.Invoke(this);
    }

    public bool CanDefend()
    {
        bool canAttack = false;

        var immediatePositions = GridUtility.GetReachableCells(this, 0);

        var attackableCells = GridUtility.GetAttackableCells(this, immediatePositions, EquippedWeapon);
        if (attackableCells.Count > 0)
            canAttack = true;

        return canAttack;
    }

    /// <summary>
    /// Checks if this unit can attack the targetUnit given
    /// </summary>
    /// <param name="targetUnit"></param>
    /// <returns>Whether or not the targetUnit is a valid target that can be attacked</returns>
    public bool CanAttack(Unit targetUnit)
    {
        var weaponsThatCanHit = AttackableWeapons();

        if (weaponsThatCanHit.Count == 0 || !weaponsThatCanHit.Keys.Contains<Weapon>(EquippedWeapon))
            return false;

        return weaponsThatCanHit[EquippedWeapon].Contains(targetUnit.GridPosition);
    }

    /// <summary>
    /// Returns whether or not attackable weapons count is not 0 and if mounted, checks that they have not performed a second move
    /// </summary>
    public bool CanAttack() 
    {
        // If unit is mounted and performed a second move after trading, they cannot attack, therefore return false
        if (_isMounted && _performedSecondMove)
            return false;

        return AttackableWeapons().Count > 0; 
    }

    public Dictionary<Weapon, List<Vector2Int>> AttackableWeapons(bool currentPositionOnly = true)
    {
        var attackableWeapons = new Dictionary<Weapon, List<Vector2Int>>();

        var moveCost = -1;
        if (currentPositionOnly)
            moveCost = 0;
        var reachableCells = GridUtility.GetReachableCells(this, moveCost);

        foreach (var weapon in Inventory.GetItems<Weapon>())
        {
            var attackableCells = GridUtility.GetAttackableCells(this, reachableCells, weapon);
            if (attackableCells.Count > 0)
                attackableWeapons[weapon] = attackableCells;
        }

        // Sort by Attack Damage
        attackableWeapons.OrderBy((entry) => AttackDamage(entry.Key));

        return attackableWeapons;
    }

    public List<Vector2Int> AllTradableCells()
    {
        var allTradableCells = new List<Vector2Int>();

        foreach (var offset in GridUtility.DefaultNeighboursOffsets)
        {
            var pos = GridPosition + offset;
            var otherUnit = WorldGrid.Instance[pos].Unit;
            if (otherUnit != null && IsAlly(otherUnit))
                allTradableCells.Add(pos);
        }
        return allTradableCells;
    }

    public List<Vector2Int> AllLootableCells()
    {
        var allLootableCells = new List<Vector2Int>();

        if (WorldGrid.Instance[GridCursor.Instance.GridPosition].LootableBodiesCount() > 0)
            allLootableCells.Add(GridCursor.Instance.GridPosition);

        foreach (var offset in GridUtility.DefaultNeighboursOffsets)
        {
            var pos = GridCursor.Instance.GridPosition + offset;
            if (WorldGrid.Instance[pos].LootableBodiesCount() > 0)
                allLootableCells.Add(pos);
        }
        return allLootableCells;
    }

    public bool CanLoot()
    {
        if (_didTrade)
            return false;

        if (WorldGrid.Instance[GridCursor.Instance.GridPosition].LootableBodiesCount() > 0)
            return true;

        return GridUtility.DefaultNeighboursOffsets.Any(offset =>
        {
            if (!WorldGrid.Instance.PointInGrid(GridCursor.Instance.GridPosition + offset))
                return false;

            return WorldGrid.Instance[GridCursor.Instance.GridPosition + offset].LootableBodiesCount() > 0;
        });
    }

    public List<Vector2Int> AllAttackableCells(bool currentPositionOnly = true)
    {
        var allAttackableCells = new List<Vector2Int>();

        var moveCost = -1;
        if (currentPositionOnly)
            moveCost = 0;
        var reachableCells = GridUtility.GetReachableCells(this, moveCost);

        foreach (var weapon in Inventory.GetItems<Weapon>())
        {
            var attackableCells = GridUtility.GetAttackableCells(this, reachableCells, weapon);

            if (attackableCells.Count > 0)
                foreach (Vector2Int pos in attackableCells)
                    if (!allAttackableCells.Contains(pos))
                        allAttackableCells.Add(pos);
        }

        return allAttackableCells;
    }

    public List<Vector2Int> AttackableCells()
    {
        var attackableCellsByEquippedWeapon = new List<Vector2Int>();

        if (EquippedWeapon == null)
            return attackableCellsByEquippedWeapon;

        var reachableCells = GridUtility.GetReachableCells(this);

        var attackableCells = GridUtility.GetAttackableCells(this, reachableCells, EquippedWeapon);

        if (attackableCells.Count > 0)
            foreach (Vector2Int pos in attackableCells)
                if (!attackableCellsByEquippedWeapon.Contains(pos))
                    attackableCellsByEquippedWeapon.Add(pos);

        // Sort by distance from current position
        var sortedByDistance = attackableCellsByEquippedWeapon.OrderBy(
            (atkTarget) => GridUtility.GetBoxDistance(this.GridPosition, atkTarget)
        ).ToList();

        return sortedByDistance;
    }

    public Dictionary<Vector2Int, List<Vector2Int>> CellsWhereICanAttackFrom()
    {
        var cellsToNavigateToForAnAttack = new Dictionary<Vector2Int, List<Vector2Int>>();

        var attackableCellsByEquippedWeapon = AttackableCells();

        // TODO: Ensure errors aren't thrown when no weapon is equipped
        // Set radius based on weapon range's max range
        var radius = EquippedWeapon.Stats[WeaponStat.MaxRange].ValueInt;

        foreach (Vector2Int atkTarget in attackableCellsByEquippedWeapon)
        {
            var reachableAttackPoints = GridUtility.GetReachableCellNeighbours(atkTarget, radius, this);
            foreach (Vector2Int potentialAttackPoint in reachableAttackPoints)
            {
                if (!cellsToNavigateToForAnAttack.Keys.Contains(atkTarget))
                    cellsToNavigateToForAnAttack[atkTarget] = new List<Vector2Int>();

                cellsToNavigateToForAnAttack[atkTarget].Add(potentialAttackPoint);
            }
        }

        // order attack point lists by distance away, without linq
        foreach (KeyValuePair<Vector2Int, List<Vector2Int>> entry in cellsToNavigateToForAnAttack)
            entry.Value.Sort(delegate (Vector2Int atkPoint1, Vector2Int atkPoint2)
            {
                var firstDistance = GridUtility.GetBoxDistance(this.GridPosition, atkPoint1);
                var secondDistance = GridUtility.GetBoxDistance(this.GridPosition, atkPoint2);

                return firstDistance.CompareTo(secondDistance);
            });

        return cellsToNavigateToForAnAttack;
    }

    /// <summary>
    /// Checks that the unit has not already performed a trade and if there are any tradable units in range
    /// </summary>
    public bool CanTrade()
    {
        if (_didTrade)
            return false;

        return GridUtility.DefaultNeighboursOffsets.Any(offset =>
        {
            if (!WorldGrid.Instance.PointInGrid(GridPosition + offset))
                return false;

            var unit = WorldGrid.Instance[GridPosition + offset].Unit;
            return unit != null && unit.IsLocalPlayerUnit;
        });
    }

    public bool CanUseItems()
    {
        if (_isMounted)
            return !_performedSecondMove;

        return true;
    }

    public void Trade(Unit otherUnit, Item ourItem, Item theirItem)
    {
        // Two items selected
        if (ourItem != null && theirItem != null)
        {
            Inventory.ExchangeItems(ourItem, theirItem, otherUnit.Inventory);
        }
        // Item and empty slot selected
        else if (ourItem != null)
        {
            Inventory.MoveItem(ourItem, otherUnit.Inventory);
        }
        // Empty slot and item selected
        else if (theirItem != null)
        {
            otherUnit.Inventory.MoveItem(theirItem, Inventory);
        }
        else
        {
            throw new ArgumentException("Two empty slots provided or both items not found in inventories!");
        }

        // TODO: Insert DidTrade features
        _didTrade = true;
        _initialGridPosition = new KeyValuePair<Vector2Int, Vector2>(GridPosition, _lookDirection);
    }


    public bool IsPlaying => Application.IsPlaying(this);
    #endregion

    #region Battle Formulas
    /************************************************************************************************************************/
    //  Battle Formulas
    /************************************************************************************************************************/

    public int AttackDamage(Weapon weapon)
    {
        int weaponDamage = weapon.Stats[WeaponStat.Damage].ValueInt;

        if (EquippedWeapon.Type == WeaponType.Grimiore)  // MAGIC USER
            return Magic + weaponDamage;
        else
            return Strength + weaponDamage;
    }

    public int AttackDamage() => AttackDamage(EquippedWeapon);

    public int AttackSpeed(Weapon weapon)
    {
        int burden = 0;

        // This way, it's 0 burden if I'm unarmed.
        if (weapon != null)
        {
            int weaponWeight = weapon.Weight;
            burden = weaponWeight - Constitution;

            if (burden < 0)
                burden = 0;
        }

        return Stats[UnitStat.Speed].ValueInt - burden;
    }


    public int AttackSpeed() => AttackSpeed(EquippedWeapon);

    public Dictionary<string, int> PreviewAttack(Unit defender, Weapon weapon)
    {
        Dictionary<string, int> battleStats = new Dictionary<string, int>();

        int atkDmg;
        if (weapon.Type == WeaponType.Grimiore)
            atkDmg = AttackDamage(weapon) - defender.Resistance;
        else
            atkDmg = AttackDamage(weapon) - defender.Defense;

        battleStats["ATK_DMG"] = atkDmg;
        battleStats["ACCURACY"] = Accuracy(defender, weapon);
        battleStats["CRIT_RATE"] = CriticalHitRate(defender, weapon);


        return battleStats;
    }

    public bool CanDoubleAttack(Unit target, Weapon weapon)
    {
        int minDoubleAttackBuffer = 5;

        if ((this.AttackSpeed(weapon) - target.AttackSpeed(target.EquippedWeapon)) > minDoubleAttackBuffer)
            return true;

        return false;
    }

    public int CriticalHitRate(Unit target, Weapon weapon)
    {
        int weaponCritChance = weapon.Stats[WeaponStat.CriticalHit].ValueInt;

        int critRate = ((Skill / 2) + weaponCritChance) - target.Luck;
        if (critRate < 0)
            critRate = 0;

        if (EquippedWeapon.Type == WeaponType.Grimiore)
        {
            if (MagicProfiency[EquippedWeapon.MagicType] == WeaponRank.S)
                critRate += 5;
        }
        else if (WeaponProfiency[EquippedWeapon.Type] == WeaponRank.S)
            critRate += 5;

        return critRate;
    }

    public int DodgeChance(Weapon weapon)
    {
        int luckStat = Stats[UnitStat.Luck].ValueInt;

        return (AttackSpeed(weapon) * 2) + luckStat;
    }

    public int HitRate(Vector2Int targetPosition, Weapon weapon)
    {
        int boxDistance = GridUtility.GetBoxDistance(this.GridPosition, targetPosition);
        int weaponHitStat = weapon.Stats[WeaponStat.Hit].ValueInt;

        int hitRate = (Skill * 2) + Luck + weaponHitStat;
        if (boxDistance >= 2)
            hitRate -= 15;

        hitRate = Mathf.Clamp(hitRate, 0, 100);

        return hitRate;
    }

    public int Accuracy(Unit target, Weapon weapon)
    {
        int boxDistance = GridUtility.GetBoxDistance(this.GridPosition, target.GridPosition);

        if (weapon.Type != WeaponType.Staff && weapon.Type != WeaponType.Grimiore)
        {
            // TODO: Add weapon W△ Weapon triangle effects
            return HitRate(target.GridPosition, weapon) - target.DodgeChance(weapon);
        }
        else
        {
            var accuracy = ((Magic - target.Resistance) * 4) + Skill + 30 - (boxDistance * 2);
            accuracy = Mathf.Clamp(accuracy, 0, 100);

            return accuracy;
        }
    }
    #endregion

    #region AI Decision Making Metrics
    /************************************************************************************************************************/
    //  AI Decision Making Metrics
    /************************************************************************************************************************/
    public virtual Vector2Int FindClosestCellTo(Vector2Int goal)
    {
        var moveRange = GridUtility.GetReachableCells(this);

        if (moveRange.Contains(goal))
            return goal;

        Vector2Int targetCell;

        // Get cells next to the target that are passable and unoccupied
        var targetNeighbors = GridUtility.GetNeighbours(SortingLayerId, goal);
        targetNeighbors.Select((position) => WorldGrid.Instance[position].IsPassable(SortingLayerId, UnitType));

        // Find the nearest neighbor
        var shortestDistance = targetNeighbors.Min((position) => GridUtility.GetBoxDistance(GridPosition, position));
        targetCell = targetNeighbors.First((position) => GridUtility.GetBoxDistance(GridPosition, position) == shortestDistance);

        // Take the nearest neighbor and go to the closest REACHABLE cell
        shortestDistance = moveRange.Min((position) => GridUtility.GetBoxDistance(position, targetCell));
        targetCell = moveRange.First((position) => GridUtility.GetBoxDistance(position, targetCell) == shortestDistance);

        return targetCell;
    }

    // Where int == Player.TeamId
    protected Dictionary<int, List<Unit>> ThreateningUnits()
    {
        Dictionary<int, List<Unit>> unitsByTeam = new Dictionary<int, List<Unit>>();

        foreach (Vector2Int gridPos in ThreatDetectionRange())
        {
            var unit = WorldGrid.Instance[gridPos].Unit;
            if (unit != null && IsEnemy(unit))
            {
                int unitTeam = unit.Player.TeamId;
                if (!unitsByTeam.Keys.Contains(unitTeam))
                    unitsByTeam[unitTeam] = new List<Unit>();

                unitsByTeam[unitTeam].Add(unit);
            }
        }

        return unitsByTeam;
    }

    protected List<Vector2Int> PotentialAttackPoints()
    {
        List<Vector2Int> potentialAtkPoints = new List<Vector2Int>();

        var allThreateningUnits = ThreateningUnits().Values.SelectMany(x => x).ToList();
        foreach (Unit unit in allThreateningUnits)
        {
            var atkPoints = unit.CellsWhereICanAttackFrom();

            if (atkPoints.Count == 0 || !atkPoints.ContainsKey(GridPosition))
                continue;

            foreach (Vector2Int gridPos in atkPoints[GridPosition])
                if (!potentialAtkPoints.Contains(gridPos))
                    potentialAtkPoints.Add(gridPos);
        }

        return potentialAtkPoints;
    }

    protected List<Unit> ReachableAllies()
    {
        List<Unit> units = new List<Unit>();
        var moveRange = GridUtility.GetReachableCells(this, -1);

        foreach (Vector2Int gridPos in moveRange)
        {
            var unit = WorldGrid.Instance[gridPos].Unit;
            if (unit != null && IsAlly(unit))
                units.Add(unit);
        }

        return units;
    }

    protected List<int> ThreatOrder()
    {
        switch (TeamId)
        {
            case Team.LocalPlayerTeamId:
                return new List<int>
                {
                    Team.EnemyTeamId, Team.OtherEnemyTeamId, Team.AllyTeamId, Team.NeutralTeamId
                };
            case Team.EnemyTeamId:
                return new List<int>
                {
                    Team.OtherEnemyTeamId, Team.AllyTeamId, Team.NeutralTeamId, Team.LocalPlayerTeamId
                };
            case Team.OtherEnemyTeamId:
                return new List<int>
                {
                    Team.AllyTeamId, Team.NeutralTeamId, Team.LocalPlayerTeamId, Team.EnemyTeamId
                };
            case Team.AllyTeamId:
                return new List<int>
                {
                    Team.NeutralTeamId, Team.EnemyTeamId, Team.OtherEnemyTeamId, Team.AllyTeamId
                };
            case Team.NeutralTeamId:
                return new List<int>
                {
                    Team.LocalPlayerTeamId, Team.EnemyTeamId, Team.OtherEnemyTeamId, Team.AllyTeamId
                };
        }

        throw new Exception($"Invalid TeamId: {TeamId}");
    }

    protected void SetPotentialThreats()
    {
        // Empty List pf Potential Units
        PotentialThreats = new List<Unit>();

        var potentialThreats = ThreateningUnits();
        var allPotentialAtkPoints = PotentialAttackPoints();

        // Prioritize Units who will attack in the sooner phases
        foreach (int teamId in ThreatOrder())
        {
            if (!potentialThreats.Keys.Contains(teamId))
                continue;

            var threatsOnTeam = potentialThreats[teamId];
            threatsOnTeam.Sort(delegate (Unit unitOne, Unit unitTwo)
            {
                var firstThreateningWeapon = unitOne.AttackableWeapons(false);
                var firstMostDangerousWeapon = firstThreateningWeapon.Keys.FirstOrDefault((weapon) => firstThreateningWeapon[weapon].Contains(GridPosition) == true);

                var secondThreateningWeapon = unitTwo.AttackableWeapons(false);
                var secondMostDangerousWeapon = secondThreateningWeapon.Keys.FirstOrDefault((weapon) => secondThreateningWeapon[weapon].Contains(GridPosition) == true);


                var firstPotentialDamage = firstMostDangerousWeapon != null ? unitOne.PreviewAttack(this, firstMostDangerousWeapon)["ATK_DMG"] : 0;
                var secondPotentialDamage = secondMostDangerousWeapon != null ? unitTwo.PreviewAttack(this, secondMostDangerousWeapon)["ATK_DMG"] : 0;

                return firstPotentialDamage.CompareTo(secondPotentialDamage);
            });

            var unitsToAdd = Mathf.Min(threatsOnTeam.Count, allPotentialAtkPoints.Count - PotentialThreats.Count) - 1;
            for (int i = 0; i < unitsToAdd; i++)
                PotentialThreats.Add(threatsOnTeam[i]);

            if (PotentialThreats.Count == allPotentialAtkPoints.Count)
                break;
        }
    }

    public float ThreatLevel()
    {
        float threatLevel = 0;

        SetPotentialThreats();
        var threatCount = PotentialThreats.Count;

        foreach (Unit unit in PotentialThreats)
        {
            Weapon mostDangerousWeapon = null;
            var threateningWeapons = unit.AttackableWeapons(false);
            var weaponsThatCanHitMe = threateningWeapons.Keys.Where((weapon) => threateningWeapons[weapon].Contains(GridPosition) == true);

            if (weaponsThatCanHitMe.Count() > 0)
                mostDangerousWeapon = weaponsThatCanHitMe.First();


            float accuracyAsDecimal = 0;
            int damage = 0;
            if (mostDangerousWeapon != null)
            {
                var attackPreview = unit.PreviewAttack(this, mostDangerousWeapon);
                accuracyAsDecimal = (float)attackPreview["ACCURACY"] / 100;
                damage = attackPreview["ATK_DMG"];
            }

            var unitThreatLevel = (accuracyAsDecimal * (1 - (CurrentHealth - damage) / CurrentHealth));

            if (this is AIUnit)
            {
                var aiUnit = this as AIUnit;
                if (aiUnit.group != null)
                    unitThreatLevel += (aiUnit.group.Members.Count * .1f);
            }

            threatLevel += unitThreatLevel;
        }

        return Mathf.Clamp(threatLevel, 0, 1f);
    }

    public float ThreatLevelAtPosition(Vector2Int newPosition)
    {
        float threatLevel = 0;

        SetPotentialThreats();
        var threatCount = PotentialThreats.Count;

        foreach (Unit unit in PotentialThreats)
        {
            var threateningWeapons = unit.AttackableWeapons(false);
            var mostDangerousWeapon = threateningWeapons.Keys.FirstOrDefault((weapon) => threateningWeapons[weapon].Contains(newPosition) == true);

            float accuracyAsDecimal = 0;
            int damage = 0;
            if (mostDangerousWeapon != null)
            {
                var attackPreview = unit.PreviewAttack(this, mostDangerousWeapon);
                accuracyAsDecimal = (float)attackPreview["ACCURACY"] / 100;
                damage = attackPreview["ATK_DMG"];
            }

            var unitThreatLevel = (accuracyAsDecimal * (1 - (CurrentHealth - damage) / CurrentHealth));

            if (this is AIUnit)
            {
                var aiUnit = this as AIUnit;
                if (aiUnit.group != null)
                    unitThreatLevel += (aiUnit.group.Members.Count * .1f);
            }

            threatLevel += unitThreatLevel;
        }

        return Mathf.Clamp(threatLevel, 0, 1f);
    }
    #endregion

    #region On Map Combat 
    /************************************************************************************************************************/
    //  On Map Combat
    /************************************************************************************************************************/
    public void AttackOnMap(Unit defender)
    {
        // End Attack if Unit cannot Attack
        if (!CanAttack(defender))
        {
            Debug.Log("Cant attack...");
            UponAttackComplete?.Invoke();
            return;
        }
            

        var directionToLook = DirectionUtility.GetDirection(transform.position, defender.transform.position);
        Rotate(directionToLook);

        directionToLook = DirectionUtility.GetDirection(defender.transform.position, transform.position);
        defender.Rotate(directionToLook);
        defender.SetIdle();

        PlayAttackAnimation(defender);
    }

    public void PlayAttackAnimation(Unit target)
    {
        var lookingDown = _lookDirection.y < 0;
        if (!lookingDown)
            _renderer.sortingOrder = target.OrderInLayer + 1;

        var animations = _attackAnimations.CurrentAnimation();

        var clip = animations.GetClip(_lookDirection);
        var state = _animancer.Play(clip);

        state.Speed = 1.5f;

        _OnAttackLaunched.Set(state, InvokeAttackEvent());
        _OnAttackFinished.Set(state, InvokeFinishAttackingEvent());
    }

    public void TakeDamage(int damage, bool critical = false, bool displayOnly = false)
    {
        PlayAnimation(_Damage);

        _flasher.Flash(Color.red, 1f, 1f, true);

        if ((CurrentHealth - damage) <= 0)
            isDying = true;

        if (displayOnly)
        {
            UponDeath += delegate (Unit deadSelf)
            {
                var controller = GetComponent<SpriteCharacterControllerExt>();
                controller.Die();
            };

            DecreaseHealth(damage);

            return; 
        }

        var currentHealthPercentage = (float)Mathf.Max(CurrentHealth - damage, 0) / MaxHealth;
        _healthBar.OnComplete += delegate ()
        {
            UponDeath += delegate (Unit deadSelf)
            {
                if (Unkillable)
                    PlayAnimation(_Incapacitated);
                else
                    PlayAnimation(_Death);
            };
            DecreaseHealth(damage);

        };
        _healthBar.Tween(currentHealthPercentage);

        if(critical)
            combatText.StartDisplay(CombatText.TextModifier.Critical, string.Format("{0}", damage));
        else
            combatText.StartDisplay(CombatText.TextModifier.Damage, string.Format("{0}", damage));
    }

    public void WaitUntilTargetIsDead(Unit defender) => StartCoroutine(WaitTargetDies(defender));

    private IEnumerator WaitTargetDies(Unit defender)
    {
        yield return new WaitUntil(() => defender.IsDying == false);
    }

    public void WaitForReaction(Unit waitingForUnit, Action actionToTake) => StartCoroutine(WaitingForReaction(waitingForUnit, actionToTake));

    public IEnumerator WaitingForReaction(Unit waitingForUnit, Action actionToTake)
    {
        //Debug.Log("Waiting for reaction...");

        while (waitingForUnit.IsDodging == true || waitingForUnit.IsDying == true || waitingForUnit.IsHitReacting == true)
            yield return new WaitForEndOfFrame();

        actionToTake.Invoke();
    }


    public void DodgeAttack(Unit attacker)
    {
        _facingDirection = DirectionUtility.GetDirection(transform.position, attacker.transform.position);

        _dodgeReturnPosition = transform.position;
        _dodgePosition = transform.position;

        switch (_facingDirection)
        {
            case Direction.Right:
                _dodgePosition.x -= 1f;
                break;
            case Direction.Left:
                _dodgePosition.x += 1f;
                break;
            case Direction.Up:
                _dodgePosition.y -= 1f;
                break;
            case Direction.Down:
                _dodgePosition.y += 1f;
                break;
        }

        isDodging = true;
        combatText.StartDisplay(CombatText.TextModifier.Normal, "Dodge!");
    }

    protected IEnumerator DodgeMovement()
    {
        var supportedDirections = new List<Direction> { Direction.Left, Direction.Down, Direction.Right, Direction.Up };
        if (!supportedDirections.Contains(_facingDirection))
            throw new Exception($"[Unit] Tried to start DodgeMovement, but supplied unsupported facingDirection: {_facingDirection}");

        if (_dodgePosition == _dodgeReturnPosition)
            Debug.Log("I am dodging movement.");

        var reachedTargetPosition = false;

        transform.position = Vector2.Lerp(transform.position, _dodgePosition, 4f * Time.smoothDeltaTime);

        var distanceToDodgePoint = Vector2.Distance(transform.position, _dodgePosition);
        if (distanceToDodgePoint < 0.1f)
            reachedTargetPosition = true;


        bool HasReachTargetPosition() => reachedTargetPosition;

        yield return new WaitUntil(HasReachTargetPosition);

        var hasReturnedToPosition = false;

        transform.position = Vector2.Lerp(_dodgePosition, _dodgeReturnPosition, 4.6f * Time.smoothDeltaTime);
        var distanceToOriginalPosition = Vector2.Distance(transform.position, _dodgeReturnPosition);
        if (distanceToOriginalPosition < 0.99f)
        {
            hasReturnedToPosition = true;
            transform.position = _dodgeReturnPosition;
        }

        bool HasReturned() => hasReturnedToPosition;

        yield return new WaitUntil(HasReturned);

        isDodging = false;

        UponDodgeComplete?.Invoke();
    }

    public void ClearOnMapBattleEvents()
    {
        UponAttackLaunched = null;
        UponAttackAnimationEnd = null;
        UponAttackComplete = null;
        UponDodgeComplete = null;
        UponDamageCalcComplete = null;
    }
    #endregion

    #region Status Effects
    /************************************************************************************************************************/
    //  Status Effects
    /************************************************************************************************************************/

    public virtual void ApplyAuras(Vector2Int origin)
    {
        if (UnitClass.StatusEffects.Count == 0)
            return;
        var maxRadius = _unitClass.StatusEffects.Select(se => se)
            .Where(se => se.Type == StatusEffectType.LeaderAura || se.Type == StatusEffectType.Aura).Max(se => se.Radius);

        CellsWithMyEffects = GridUtility.GetCellsInRadius(GridPosition, maxRadius).ToList();
        CellsWithMyEffects.Remove(origin);

        var worldGrid = WorldGrid.Instance;
        foreach (var cell in CellsWithMyEffects)
        {
            worldGrid[cell].OnEnterCell += ApplyEffect;
            worldGrid[cell].OnExitCell += RemoveEffect;

            if (worldGrid[cell].Unit != null)
                ApplyEffect(worldGrid[cell].Unit, worldGrid[cell].Position);
        }
    }

    protected virtual void RemoveAuras(Vector2Int origin)
    {
        if (_unitClass.StatusEffects.Count == 0)
            return;

        var maxRadius = _unitClass.StatusEffects.Select(se => se)
            .Where(se => se.Type == StatusEffectType.LeaderAura || se.Type == StatusEffectType.Aura).Max(se => se.Radius);

        CellsWithMyEffects = GridUtility.GetCellsInRadius(GridPosition, maxRadius).ToList();
        CellsWithMyEffects.Remove(origin);

        var worldGrid = WorldGrid.Instance;

        foreach (var cell in CellsWithMyEffects)
        {
            worldGrid[cell].OnEnterCell -= ApplyEffect;
            worldGrid[cell].OnExitCell -= RemoveEffect;

            if (worldGrid[cell].Unit != null)
                RemoveEffect(worldGrid[cell].Unit, worldGrid[cell].Position);
        }
    }

    private void ApplyEffect(Unit other, Vector2Int cell)
    {
        if (other == this)
            return;

        var auraEffects = _unitClass.StatusEffects.Select(se => se)
            .Where(se => se.Type == StatusEffectType.LeaderAura || se.Type == StatusEffectType.Aura).ToList();

        foreach (var aura in auraEffects)
        {
            bool inRadius = GridUtility.GetCellsInRadius(GridPosition, aura.Radius).ToList().Contains(cell);
            bool canBeApplied = aura.Type == StatusEffectType.LeaderAura && IsLeader || aura.Type == StatusEffectType.Aura;
            if (canBeApplied && inRadius && IsAlly(other) == aura.ToAlly)
                aura.Add(other);
        }

        //other.morale += IsAlly(other)? .2f : -.2f;
        //Debug.Log(this.Name + "Is applying morale effect on" + other.Name);
    }


    private void RemoveEffect(Unit other, Vector2Int cell)
    {
        if (other == this)
            return;

        var auraEffects = _unitClass.StatusEffects.Select(se => se)
            .Where(se => se.Type == StatusEffectType.LeaderAura || se.Type == StatusEffectType.Aura).ToList();

        foreach (var aura in auraEffects)
        {
            bool inRadius = GridUtility.GetCellsInRadius(GridPosition, aura.Radius).ToList().Contains(cell);
            if (inRadius)
                aura.Remove(other);
        }

        //other.morale += IsAlly(other) ? -.2f : .2f;
        //Debug.Log(this.Name + "No longer apply morale effect on" + other.Name);
    }

    protected virtual List<Vector2Int> ThreatDetectionRange() => throw new Exception("You haven't implemented ThreatLevel for me!");
    private void CellsWithMyEffectsGizmos()
    {
        if (CellsWithMyEffects == null)
            return;

        var worldGrid = WorldGrid.Instance;
        Gizmos.color = new Color(1, 1, 1, .5f);
        foreach (var item in CellsWithMyEffects)
        {
            Gizmos.DrawCube(worldGrid.Grid.GetCellCenterWorld((Vector3Int)item), Vector3.one);
        }
    }

    protected virtual void OnDrawGizmos()
    {
        if (Application.isPlaying)
            CellsWithMyEffectsGizmos();
    }
    #endregion

    #region Animation Event Listeners and Logic
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

    private void LaunchAttack(AnimationEvent animationEvent)
    {
        _OnAttackLaunched.SetFunctionName("LaunchAttack");
        _OnAttackLaunched.HandleEvent(animationEvent);
    }

    private Action<AnimationEvent> InvokeAttackEvent()
    {
        // Uncomment to skip animations

        // return delegate (AnimationEvent animationEvent) { }

        return delegate (AnimationEvent animationEvent) 
        {
            if (UponAttackLaunched != null)
                UponAttackLaunched.Invoke();
            else
                throw new Exception($"[Unit] {gameObject.name} has UponAttackLaunched as NULL!!");
        };
    }

    private void FinishAttacking(AnimationEvent animationEvent)
    {
        _OnAttackFinished.SetFunctionName("FinishAttacking");
        _OnAttackFinished.HandleEvent(animationEvent);
    }

    private Action<AnimationEvent> InvokeFinishAttackingEvent()
    {
        // Uncomment to skip animations

        // return delegate (AnimationEvent animationEvent) { }

        return delegate (AnimationEvent animationEvent) 
        {
            UponAttackAnimationEnd?.Invoke();

            _renderer.sortingOrder = 100;
        };
    }
    #endregion
}