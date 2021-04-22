using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Animancer;
using DarkTonic.MasterAudio;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
public class Unit : SerializedMonoBehaviour, IInitializable
{
    [FoldoutGroup("Basic Properties")]
    [SerializeField] private string _name;
    public string Name => _name;

    [FoldoutGroup("Basic Properties")]
    [DistinctUnitType]
    public UnitType UnitType;

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
    public Portrait Portrait;

    [FoldoutGroup("Basic Properties")] 
    public GameObject BattlerPrefab;

    [FoldoutGroup("Animations")] 
    [SerializeField] private DirectionalAnimationSet _idleAnimation;
    
    [FoldoutGroup("Animations")]
    [SerializeField] private DirectionalAnimationSet _walkAnimation;

    [FoldoutGroup("Animations")]
    [SerializeField] private DirectionalAnimationSet _runAnimation;

    [FoldoutGroup("Animations")]
    [SerializeField] private DirectionalAnimationSet _drinkPotionAnimation;


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
    [UnitStats, OdinSerialize, HideIf("IsPlaying")]
    private Dictionary<UnitStat, EditorStat> _statsDictionary = new Dictionary<UnitStat, EditorStat>();
    public Dictionary<UnitStat, EditorStat> EditorStats { get => _statsDictionary; }

    [FoldoutGroup("Stats"), ShowIf("IsPlaying"), PropertyOrder(0)]
    [ProgressBar(0, "MaxHealth", ColorGetter = "HealthColor", BackgroundColorGetter = "BackgroundColor", Height = 20)]
    [SerializeField] protected int currentHealth;
    public bool IsAlive => CurrentHealth > 0;
    public int CurrentHealth => currentHealth;

    [FoldoutGroup("Stats"), ShowIf("IsPlaying"), PropertyOrder(1)]
    [UnitStats]
    public Dictionary<UnitStat, Stat> Stats;
    



    
    
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


    [FoldoutGroup("Game Status")]
    public int TeamId => Player.TeamId;

    [FoldoutGroup("Game Status")]
    private Weapon _equippedWeapon;
    
    [FoldoutGroup("Game Status")]
    [ShowInInspector] public Weapon EquippedWeapon => _equippedWeapon; // TODO: Implement Gear. EquippedGear. one 1 Gear equipped per unit (ie. shield)
    public bool HasWeapon => EquippedWeapon != null;

    private Vector2Int _gridPosition;
    [FoldoutGroup("Game Status")]
    [ShowInInspector] public Vector2Int GridPosition => _gridPosition;


    // Key == GridPosition && Value == _lookDirection
    private KeyValuePair<Vector2Int, Vector2> _initialGridPosition;
    public KeyValuePair<Vector2Int, Vector2> InitialGridPosition => _initialGridPosition; // Where Unit began the turn.

    private bool _hasTakenAction;
    [FoldoutGroup("Game Status")]
    [ShowInInspector] public bool HasTakenAction => _hasTakenAction; // Has the unit moved this turn?
    
    protected int currentMovementPoints;
    [FoldoutGroup("Game Status")]
    [ShowInInspector] public int CurrentMovementPoints => currentMovementPoints;


    [HideInInspector] public Action OnFinishedMoving;
    [HideInInspector] public Action<Unit> UponDeath;
    [HideInInspector] public Action<Unit> UponLevelUp;


    // Stat Convenience Methods
    public int Weight       => Stats[UnitStat.Weight].ValueInt;
    public int Strength     => Stats[UnitStat.Strength].ValueInt;
    public int Skill        => Stats[UnitStat.Skill].ValueInt;
    public int Resistance   => Stats[UnitStat.Resistance].ValueInt;
    public int MaxHealth    => Stats[UnitStat.MaxHealth].ValueInt;
    public int Magic        => Stats[UnitStat.Magic].ValueInt;
    public int Luck         => Stats[UnitStat.Luck].ValueInt;
    public int Defense      => Stats[UnitStat.Defense].ValueInt;
    public int Constitution => Stats[UnitStat.Constitution].ValueInt;
    public int Speed        => Stats[UnitStat.Speed].ValueInt;
    public int MaxMoveRange => Stats[UnitStat.Movement].ValueInt;


    public bool IsLocalPlayerUnit => Player == Player.LocalPlayer;

    public int SortingLayerId => _renderer.sortingLayerID;


    protected virtual Player Player { get; }
    protected Battler Battler;
    protected bool isMoving;
    protected List<Unit> PotentialThreats = new List<Unit>();

    private AnimancerComponent _animancer;
    private Vector2 _lookDirection;

    private FootstepController _footstepController;

    // Mecanim AnimationEvent Listeners
    private AnimationEventReceiver _OnPlayFootsteps;

    private Renderer _renderer;

    private Material _allInOneMat;
    private static readonly int StencilRef = Shader.PropertyToID("_StencilRef");

    public virtual void Init()
    {
        _gridPosition = GridUtility.SnapToGrid(this);
        WorldGrid.Instance[GridPosition].Unit = this;

        _unitClass = UnitClass.GetUnitClass();
        InitStats();

        _renderer           = GetComponent<Renderer>();
        _animancer          = GetComponent<AnimancerComponent>();
        _footstepController = GetComponent<FootstepController>();
        Portrait            = GetComponent<Portrait>();

        Rotate(_startingLookDirection);
        _initialGridPosition =  new KeyValuePair<Vector2Int, Vector2>(GridPosition, _lookDirection);
        
        PlayAnimation(_idleAnimation);

        _allInOneMat = GetComponent<Renderer>().material;

        // TODO: load from serialization
        Inventory = new UnitInventory(this);
        foreach (var item in _startingItems)
            Inventory.AddItem(item.GetItem());

        var weapons = Inventory.GetItems<Weapon>();
        if (weapons.Length > 0)
            EquipWeapon(weapons[0]);

        EnableSilhouette();
    }

    public void EnableSilhouette()
    {
        _allInOneMat.SetInt(StencilRef, 1);
    }

    public void DisableSilhouette()
    {
        _allInOneMat.SetInt(StencilRef, 2);
    }

    private void InitStats()
    {
        // TODO: Setup serialization of Unit Stats for PlayerUnits
        Stats = new Dictionary<UnitStat, Stat>();

        // Copy base stats
        foreach (var stat in _statsDictionary.Keys)
        {
            var value   = _statsDictionary[stat];
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

    public void SetInitialPosition() => _initialGridPosition = new KeyValuePair<Vector2Int, Vector2>(GridPosition, _lookDirection);

    public void ResetToInitialPosition()
    {
        // Move back to Start instantly
        this.transform.position = WorldGrid.Instance.Grid.GetCellCenterWorld((Vector3Int)InitialGridPosition.Key);
        
        // Reset Original Look Direction
        LookAt(InitialGridPosition.Value);
        SetIdle();

        // Change Grid Position locally and within WorldGrid's WorldCell.
        SetGridPosition(InitialGridPosition.Key);
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

    public void SetIdle() => PlayAnimation(_idleAnimation);

    private void SetGridPosition(Vector2Int newGridPosition)
    {
        WorldGrid.Instance[GridPosition].Unit = null;

        _gridPosition = newGridPosition;
        WorldGrid.Instance[GridPosition].Unit = this;
    }

    private void PlayAnimation(DirectionalAnimationSet animations, float speed = 1f)
    {
        var clip = animations.GetClip(_lookDirection);
        
        var state = _animancer.Play(clip);
        state.Speed = speed;

        if (animations == _walkAnimation || animations == _runAnimation)
            _OnPlayFootsteps.Set(state, PlayFootstepSound());
    }

    public virtual void TookAction() 
    {
        _hasTakenAction = true;
        SetInactiveShader();
    }

    public virtual void AllowAction()
    {
        _hasTakenAction = false;
        RemoveInactiveShader();
    }

    private void SetInactiveShader() => _allInOneMat.EnableKeyword("HSV_ON");    
    private void RemoveInactiveShader() => _allInOneMat.DisableKeyword("HSV_ON");

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
        foreach(Weapon inventoryWeapon in Inventory.GetItems<Weapon>())
        {
            inventoryWeapon.CanWield = CanWield(inventoryWeapon);
            availableWeapons.Add(inventoryWeapon);
        }

        if (availableWeapons.Contains(weapon) && weapon.CanWield)
        {
            UnequipWeapon();
            _equippedWeapon = weapon;
            weapon.IsEquipped = true;
        } else 
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


    public void DecreaseHealth(int amount)
    {
        int damageTaken = amount;

        if (damageTaken >= CurrentHealth)
        {
            damageTaken = CurrentHealth;
            UponDeath.Invoke(this);
        }
        
        currentHealth -= damageTaken;
    }

    public void IncreaseHealth(int amount)
    {
        int currentHealthAmount = CurrentHealth + amount;
        if (currentHealthAmount > MaxHealth)
            currentHealthAmount = MaxHealth;
        
        currentHealth = currentHealthAmount;
    }


    public void GainExperience(int amount)
    {
        int currentExpAmount = _experience + amount;
        
        if (currentExpAmount > MAX_EXP_AMOUNT)
        {
            currentExpAmount -= MAX_EXP_AMOUNT;
            
            if (UponLevelUp != null)
                UponLevelUp.Invoke(this);
        }
        
        _experience = currentExpAmount;
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

    public Dictionary<Weapon, List<Vector2Int>> AttackableWeapons(bool currentPositionOnly = true) 
    {
        var attackableWeapons = new Dictionary<Weapon, List<Vector2Int>>();
        
        var moveCost = -1;
        if (currentPositionOnly)
            moveCost = 0;
        var reachableCells = GridUtility.GetReachableCells(this, moveCost);

        foreach(var weapon in Inventory.GetItems<Weapon>()) {
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
            {
                allTradableCells.Add(pos);
            }
        }

        return allTradableCells;
    }

    public List<Vector2Int> AllAttackableCells(bool currentPositionOnly = true) 
    {
        var allAttackableCells = new List<Vector2Int>();

        var moveCost = -1;
        if (currentPositionOnly)
            moveCost = 0;
        var reachableCells = GridUtility.GetReachableCells(this, moveCost);

        foreach(var weapon in Inventory.GetItems<Weapon>()) {
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
            foreach(Vector2Int pos in attackableCells)
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

        // Set radius based on weapon range's max range
        var radius = EquippedWeapon.Stats[WeaponStat.MaxRange].ValueInt;

        foreach(Vector2Int atkTarget in attackableCellsByEquippedWeapon)
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
        foreach(KeyValuePair<Vector2Int, List<Vector2Int>> entry in cellsToNavigateToForAnAttack)
            entry.Value.Sort(delegate(Vector2Int atkPoint1, Vector2Int atkPoint2) {
                var firstDistance = GridUtility.GetBoxDistance(this.GridPosition, atkPoint1);
                var secondDistance = GridUtility.GetBoxDistance(this.GridPosition, atkPoint2);
                
                return firstDistance.CompareTo(secondDistance);
            });

        return cellsToNavigateToForAnAttack;
    }


    public bool CanAttack() => AttackableWeapons().Count > 0;

    public bool CanAttack(Unit targetUnit) 
    {
        var weaponsThatCanHit = AttackableWeapons();

        if (weaponsThatCanHit.Count == 0 || !weaponsThatCanHit.Keys.Contains<Weapon>(EquippedWeapon))
            return false;

        return weaponsThatCanHit[EquippedWeapon].Contains(targetUnit.GridPosition);
    }

    public bool CanTrade()
    {
        return GridUtility.DefaultNeighboursOffsets.Any(offset =>
        {
            if (!WorldGrid.Instance.PointInGrid(GridPosition + offset))
                return false;

            var unit = WorldGrid.Instance[GridPosition + offset].Unit;
            return unit != null && unit.IsLocalPlayerUnit;
        });
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
    }

    public IEnumerator MovementCoroutine(GridPath path)
    {
        var nextPathGridPosition = GridPosition;
        var nextPathPosition = transform.position;
        var reachedGoal = false;

        var goal = path.Goal;
        WorldGrid.Instance[GridPosition].Unit = null;

        var speed = _walkSpeed * Time.deltaTime;
        DirectionalAnimationSet moveAnimation = _walkAnimation;

        if (this is PlayerUnit && Input.GetButton("Fire3")) // Left Shift by default.
        {
            moveAnimation = _runAnimation;
            speed = _runSpeed * Time.deltaTime;
        }

        PlayAnimation(moveAnimation, _moveAnimationSpeed);

        isMoving = true;
        while (!reachedGoal)
        {
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

        _gridPosition = goal;
        WorldGrid.Instance[GridPosition].Unit = this;

        isMoving = false;

        if (OnFinishedMoving != null)
            OnFinishedMoving.Invoke();
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

    public bool IsPlaying => Application.IsPlaying(this);


    // =====================
    // || Battle Formulas ||
    // =====================

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

        int hitRate = (Skill * 2) + Luck  + weaponHitStat;
        if (boxDistance >= 2)
            hitRate -= 15;
        
        hitRate = Mathf.Clamp(hitRate, 0, 100);

        return hitRate;
    }

    public int Accuracy(Unit target, Weapon weapon)
    {
        int boxDistance = GridUtility.GetBoxDistance(this.GridPosition, target.GridPosition);

        if (weapon.Type != WeaponType.Staff) {
            // TODO: Add weapon W△ Weapon triangle effects
            return HitRate(target.GridPosition, weapon) - target.DodgeChance(weapon);
        } else {
            var accuracy = (Magic - Resistance) + Skill + 30 - (boxDistance * 2);
            accuracy = Mathf.Clamp(accuracy, 0, 100);

            return accuracy;
        }
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
        foreach(Unit unit in allThreateningUnits)
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
        var moveRange = GridUtility.GetReachableCells(this, -1, true);

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
        switch(TeamId)
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
        foreach(int teamId in ThreatOrder())
        {
            if (!potentialThreats.Keys.Contains(teamId))
                continue;

            var threatsOnTeam = potentialThreats[teamId];
            threatsOnTeam.Sort(delegate (Unit unitOne, Unit unitTwo)
            {
                var firstThreateningWeapon = unitOne.AttackableWeapons(false);
                var firstMostDangerousWeapon = firstThreateningWeapon.Keys.First((weapon) => firstThreateningWeapon[weapon].Contains(GridPosition) == true);

                var secondThreateningWeapon = unitTwo.AttackableWeapons(false);
                var secondMostDangerousWeapon = secondThreateningWeapon.Keys.First((weapon) => secondThreateningWeapon[weapon].Contains(GridPosition) == true);

                var firstPreview = unitOne.PreviewAttack(this, firstMostDangerousWeapon);
                var firstPotentialDamage = firstPreview["ATK_DMG"];

                var secondPreview = unitTwo.PreviewAttack(this, secondMostDangerousWeapon);
                var secondPotentialDamage = secondPreview["ATK_DMG"];

                return firstPotentialDamage.CompareTo(secondPotentialDamage);
            });

            var unitsToAdd = Mathf.Min(threatsOnTeam.Count, allPotentialAtkPoints.Count - PotentialThreats.Count) - 1;
            for(int i = 0; i < unitsToAdd; i++)
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
            var threateningWeapons  = unit.AttackableWeapons(false);
            var mostDangerousWeapon = threateningWeapons.Keys.First((weapon) => threateningWeapons[weapon].Contains(GridPosition) == true);
            var attackPreview       = unit.PreviewAttack(this, mostDangerousWeapon);
            var accuracyAsDecimal   = (float)attackPreview["ACCURACY"] / 100;

            threatLevel += (accuracyAsDecimal * (1 - (CurrentHealth - attackPreview["ATK_DMG"]) / CurrentHealth));
        }

        return Mathf.Clamp(threatLevel, 0, 1f);
    }

    protected virtual List<Vector2Int> ThreatDetectionRange() => throw new Exception("You haven't implemented ThreatLevel for me!");

    /************************************************************************************************************************/
    //  Animation Event Listeners and Logic
    /************************************************************************************************************************/

    private void PlayFootsteps(AnimationEvent animationEvent)
    {
        _OnPlayFootsteps.SetFunctionName("PlayFootsteps");
        _OnPlayFootsteps.HandleEvent(animationEvent);
    }

    private System.Action<AnimationEvent> PlayFootstepSound()
    {
        return delegate (AnimationEvent animationEvent)
        {
            var currentSortingLayer = _renderer.sortingLayerID;
            var worldCell = WorldGrid.Instance[GridPosition];
            var walkingOnSurface = worldCell.TileAtSortingLayer(currentSortingLayer).SurfaceType;

            _footstepController.PlaySound(walkingOnSurface);
        };
    }
}
