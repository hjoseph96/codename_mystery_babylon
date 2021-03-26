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

    #region Basic Properties var
    [FoldoutGroup("Basic Properties")]
    [SerializeField] private string _name;
    public string Name { get => _name; }
    
    [FoldoutGroup("Basic Properties")]
    [DistinctUnitType]
    public UnitType UnitType;

    [FoldoutGroup("Basic Properties")] 
    [SerializeField] private float _moveSpeed = 4f;
    
    
    [FoldoutGroup("Basic Properties")] 
    [SerializeField] private float _moveAnimationSpeed = 1.75f;

    [FoldoutGroup("Basic Properties")]
    [SerializeField] private Direction _startingLookDirection;

    [FoldoutGroup("Basic Properties")]
    public Portrait Portrait;

    [FoldoutGroup("Basic Properties")] 
    public GameObject BattlerPrefab;
    #endregion

    #region Audio var
    [FoldoutGroup("Audio")]
    [SoundGroupAttribute] public string dirtFootsteps;
    [FoldoutGroup("Audio")]    
    [SoundGroupAttribute] public string grassFootsteps;
    [FoldoutGroup("Audio")]    
    [SoundGroupAttribute] public string rockFootsteps;
    #endregion

    #region Animation var
    [FoldoutGroup("Animations")] 
    [SerializeField] private DirectionalAnimationSet _idleAnimation;
    
    [FoldoutGroup("Animations")]
    [SerializeField] private DirectionalAnimationSet _walkAnimation;
    #endregion

    #region Base stats var
    public static int MAX_LEVEL = 40;
    [FoldoutGroup("Base Stats")]
    [SerializeField] private int _level;
    public int Level { get => _level; }
    
    [FoldoutGroup("Base Stats")]
    public ScriptableUnitClass UnitClass;

    private UnitClass _unitClass;
    [FoldoutGroup("Base Stats")]
    public UnitClass Class { get => _unitClass; }
    
    [FoldoutGroup("Base Stats")]
    [SerializeField] private int _experience;

    public int Experience { get => _experience; }
    public static int MAX_EXP_AMOUNT = 100;

    public bool IsLeader;
    // TODO: Leadership rank and influence radius
    
    [FoldoutGroup("Base Stats")]
    [UnitStats, OdinSerialize, HideIf("IsPlaying")]
    private Dictionary<UnitStat, EditorStat> _statsDictionary = new Dictionary<UnitStat, EditorStat>();

    [FoldoutGroup("Stats"), ShowIf("IsPlaying"), PropertyOrder(0)]
    [ProgressBar(0, "MaxHealth", ColorGetter = "HealthColor", BackgroundColorGetter = "BackgroundColor", Height = 20)]
    [SerializeField] private int _currentHealth;
    public bool IsAlive => CurrentHealth > 0;
    public int CurrentHealth { get => _currentHealth; }

    [FoldoutGroup("Stats"), ShowIf("IsPlaying"), PropertyOrder(1)]
    [UnitStats]
    public Dictionary<UnitStat, Stat> Stats;

    #endregion


    #region Items var

    [FoldoutGroup("Items")]
    [SerializeField] private ScriptableItem[] _startingItems;

    public UnitInventory Inventory { get; private set; }

    [FoldoutGroup("Items")]
    // Temporary measure until Playerunit have Stats seerialized
    [SerializeField] private WeaponRank _startingRank = WeaponRank.D;


    private Dictionary<WeaponType, WeaponRank> _weaponProfiency = new Dictionary<WeaponType, WeaponRank>();
    [FoldoutGroup("Items")]
    [ShowInInspector] public Dictionary<WeaponType, WeaponRank> WeaponProfiency { get => _weaponProfiency; }

    private Dictionary<MagicType, WeaponRank> _magicProfiency = new Dictionary<MagicType, WeaponRank>();
    [FoldoutGroup("Items")]
    [ShowInInspector] public Dictionary<MagicType, WeaponRank> MagicProfiency { get => _magicProfiency; }
    #endregion

    #region Game Status var

    [FoldoutGroup("Game Status")]
    public int TeamId => Player.TeamId;

    [FoldoutGroup("Game Status")]
    private Weapon _equippedWeapon;
    
    [FoldoutGroup("Game Status")]
    [ShowInInspector] public Weapon EquippedWeapon { get => _equippedWeapon; }   // TODO: Implement Gear. EquippedGear. one 1 Gear equipped per unit (ie. shield)
    public bool HasWeapon => EquippedWeapon != null;

    [FoldoutGroup("Game Status")]
    private Gear _equippedGear;

    [FoldoutGroup("Game Status")]
    [ShowInInspector] public Gear EquippedGear { get => _equippedGear; }   // TODO: Implement Gear. EquippedGear. one 1 Gear equipped per unit (ie. shield)
    public bool HasGear => EquippedGear != null;

    [ShowInInspector] public Dictionary<UnitStat, IEffect> GearEffects = new Dictionary<UnitStat, IEffect>();

    private Vector2Int _gridPosition;
    [FoldoutGroup("Game Status")]
    [ShowInInspector] public Vector2Int GridPosition { get => _gridPosition; }


    // Key == GridPosition && Value == _lookDirection
    private KeyValuePair<Vector2Int, Vector2> _initialGridPosition;
    public KeyValuePair<Vector2Int, Vector2> InitialGridPosition { get => _initialGridPosition; }  // Where Unit began the turn.

    private bool _hasTakenAction;
    [FoldoutGroup("Game Status")]
    [ShowInInspector] public bool HasTakenAction { get => _hasTakenAction; }  // Has the unit moved this turn?
    
    private int _currentMovementPoints;
    [FoldoutGroup("Game Status")]
    [ShowInInspector] public int CurrentMovementPoints { get => _currentMovementPoints; }

    #endregion

    [HideInInspector] public Action OnFinishedMoving;
    [HideInInspector] public Action<Unit> UponDeath;
    [HideInInspector] public Action<Unit> UponLevelUp;


    #region Stat Convenience Methods
    public int Weight {     get => Stats[UnitStat.Weight].ValueInt; }
    public int Strength {   get => Stats[UnitStat.Strength].ValueInt; }
    public int Skill {      get => Stats[UnitStat.Skill].ValueInt; }
    public int Resistance { get => Stats[UnitStat.Resistance].ValueInt; }
    public int MaxHealth {  get => Stats[UnitStat.MaxHealth].ValueInt; }
    public int Magic {      get => Stats[UnitStat.Magic].ValueInt; }
    public int Luck {       get => Stats[UnitStat.Luck].ValueInt; }
    public int Defense {    get => Stats[UnitStat.Defense].ValueInt; }
    public int Constitution { get => Stats[UnitStat.Constitution].ValueInt; }
    public int Speed {      get => Stats[UnitStat.Speed].ValueInt; }

    #endregion
    public bool IsLocalPlayerUnit => Player == Player.LocalPlayer;


    protected virtual Player Player { get; }
    protected Battler Battler;
    protected bool isMoving = false;
    protected List<Unit> PotentialThreats = new List<Unit>();

    private AnimancerComponent _animancer;
    private Vector2 _lookDirection;
    private Dictionary<SurfaceType, string> _footsteps = new Dictionary<SurfaceType, string>();

    
    private Material _allInOneMat;

    #region Unit Initialization
    public virtual void Init()
    {
        _gridPosition = GridUtility.SnapToGrid(this);
        WorldGrid.Instance[GridPosition].Unit = this;

        SetupFootstepSounds();

        _unitClass = UnitClass.GetUnitClass();
        InitStats();

        _animancer = GetComponent<AnimancerComponent>();
        Portrait = GetComponent<Portrait>();

        Rotate(_startingLookDirection);
        _initialGridPosition =  new KeyValuePair<Vector2Int, Vector2>(GridPosition, _lookDirection);
        
        PlayAnimation(_idleAnimation);

        _allInOneMat = GetComponent<Renderer>().material;

        Inventory = new UnitInventory(this);
        foreach (var item in _startingItems)
            Inventory.AddItem(item.GetItem());

        var weapons = Inventory.GetItems<Weapon>();
        if (weapons.Length > 0)
            EquipWeapon(weapons[0]);

        var gears = Inventory.GetItems<Gear>();
        if (gears.Length > 0)
            EquipGear(gears[0]);
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

        _currentMovementPoints = Stats[UnitStat.Movement].ValueInt;
        _currentHealth = MaxHealth;

        foreach (var weaponType in Class.UsableWeapons)
            _weaponProfiency.Add(weaponType, _startingRank);

        foreach (var magicType in Class.UsableMagic)
            _magicProfiency.Add(magicType, _startingRank);
    }


    private void SetupFootstepSounds()
    {
        _footsteps[SurfaceType.Grass]   = grassFootsteps;
        _footsteps[SurfaceType.Dirt]    = dirtFootsteps;
        _footsteps[SurfaceType.Rock]    = rockFootsteps;
    }

    #endregion

    #region Initial Position
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
    #endregion
    public bool IsAlly(Unit unit) => Player.IsAlly(unit.Player);

    public bool IsEnemy(Unit unit) => !IsAlly(unit);

    public void LookAt(Vector2 position)
    {

        _lookDirection = (position - (Vector2)transform.position).normalized;

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
        _animancer.Play(clip).Speed = speed;
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

    #region Weapon
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
    #endregion

    #region Gear 
    public List<Gear> WieldableGear()
    {
        var wieldableGear = new List<Gear>();

        foreach (Gear inventoryWeapon in Inventory.GetItems<Gear>())

            wieldableGear.Add(inventoryWeapon);

        return wieldableGear;
    }

    public void EquipGear(Gear gear)
    {
        var availableGear = new List<Gear>();
        foreach (Gear inventoryGear in Inventory.GetItems<Gear>())
        {
            availableGear.Add(inventoryGear);
        }

        if (availableGear.Contains(gear))
        {
            UnequipGear();
            _equippedGear = gear;

            // add stat from the gear
            foreach (var key in gear.Stats.Keys)
            {
                UnitStat _statType = (UnitStat)Enum.Parse(typeof(UnitStat), key.ToString());
                IEffect _gearEffect = new FlatBonusEffect(gear.Stats[key].ValueInt);
                GearEffects.Add(_statType, _gearEffect);
                Stats[_statType].AddEffect(_gearEffect);
            }
                

            gear.IsEquipped = true;
        }
        else
            throw new Exception($"Provided Weapon: {gear.Name} is not in Unit#{Name}'s Inventory...");
    }

    public void UnequipGear()
    {
        if (EquippedGear != null)
            EquippedGear.IsEquipped = false;
        foreach (var effect in GearEffects)
        {
            Stats[effect.Key].RemoveEffect(effect.Value);
        }
        GearEffects.Clear();
        _equippedGear = null;
    }
    

    #endregion

    #region Health


    public void DecreaseHealth(int amount)
    {
        int damageTaken = amount;

        if (damageTaken >= CurrentHealth)
        {
            damageTaken = CurrentHealth;
            UponDeath.Invoke(this);
        }
        
        _currentHealth -= damageTaken;
    }

    public void IncreaseHealth(int amount)
    {
        int currentHealthAmount = CurrentHealth + amount;
        if (currentHealthAmount > MaxHealth)
            currentHealthAmount = MaxHealth;
        
        _currentHealth = currentHealthAmount;
    }
    #endregion

    #region Experience
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
    #endregion

    public bool CanDefend()
    {
        bool canAttack = false;

        var immediatePositions = GridUtility.GetReachableCells(this, 0);

        var attackableCells = GridUtility.GetAttackableCells(this, immediatePositions, EquippedWeapon);
        if (attackableCells.Count > 0)
            canAttack = true;
        
        return canAttack;
    }
    #region Attack
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
    #endregion

    #region Trade
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

    #endregion

    #region Move

    public IEnumerator MovementCoroutine(GridPath path)
    {
        var nextPathGridPosition = GridPosition;
        var nextPathPosition = transform.position;
        var reachedGoal = false;

        var goal = path.Goal;
        WorldGrid.Instance[GridPosition].Unit = null;

        PlayAnimation(_walkAnimation, _moveAnimationSpeed);

        isMoving = true;
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

                        var footstepSound = _footsteps[WorldGrid.Instance[newGridPosition].SurfaceType];
                        MasterAudio.PlaySound3DFollowTransform(footstepSound, CampaignManager.AudioListenerTransform);



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
    #endregion
    public bool IsPlaying => Application.IsPlaying(this);

    #region Battle Formulas
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
        int weaponWeight = weapon.Weight;
        int burden = weaponWeight - Constitution;

        if (burden < 0)
            burden = 0;
        
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

    #endregion

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

    #region Threat

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
}
#endregion