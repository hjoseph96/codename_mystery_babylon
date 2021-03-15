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
    public string Name { get { return _name; } }
    
    [FoldoutGroup("Basic Properties")]
    [DistinctUnitType]
    public UnitType UnitType;

    [FoldoutGroup("Basic Properties")] 
    [SerializeField] private float _moveSpeed = 4f;
    
    
    [FoldoutGroup("Basic Properties")] 
    [SerializeField] private float _moveAnimationSpeed = 1.75f;
    
    [FoldoutGroup("Basic Properties")] 
    public GameObject BattlerPrefab;


    [FoldoutGroup("Audio")]
    [SoundGroupAttribute] public string dirtFootsteps;
    [FoldoutGroup("Audio")]    
    [SoundGroupAttribute] public string grassFootsteps;
    [FoldoutGroup("Audio")]    
    [SoundGroupAttribute] public string rockFootsteps;





    [FoldoutGroup("Animations")] 
    [SerializeField] private DirectionalAnimationSet _idleAnimation;
    
    [FoldoutGroup("Animations")]
    [SerializeField] private DirectionalAnimationSet _walkAnimation;


    public static int MAX_LEVEL = 40;
    [FoldoutGroup("Base Stats")]
    [SerializeField] private int _level;
    public int Level { get { return _level; } }
    
    [FoldoutGroup("Base Stats")]
    public ScriptableUnitClass UnitClass;

    private UnitClass _unitClass;
    [FoldoutGroup("Base Stats")]
    public UnitClass Class { get { return _unitClass; } }
    
    [FoldoutGroup("Base Stats")]
    [SerializeField] private int _experience;
    public int Experience { get { return _experience; } }
    public static int MAX_EXP_AMOUNT = 100;
    
    [FoldoutGroup("Base Stats")]
    [UnitStats, OdinSerialize, HideIf("IsPlaying")]
    private Dictionary<UnitStat, EditorStat> _statsDictionary = new Dictionary<UnitStat, EditorStat>();

    [FoldoutGroup("Stats"), ShowIf("IsPlaying"), PropertyOrder(0)]
    [ProgressBar(0, "MaxHealth", ColorGetter = "HealthColor", BackgroundColorGetter = "BackgroundColor", Height = 20)]
    [SerializeField] private int _currentHealth;
    public bool IsAlive => CurrentHealth > 0;
    public int CurrentHealth {
        get { return _currentHealth; }
    }
    public int MaxHealth => Stats[UnitStat.MaxHealth].ValueInt;

    [FoldoutGroup("Stats"), ShowIf("IsPlaying"), PropertyOrder(1)]
    [UnitStats]
    public Dictionary<UnitStat, Stat> Stats;
    



    
    
    [FoldoutGroup("Items")]
    [SerializeField] private ScriptableItem[] _startingItems;

    // TODO: Implement WeaponRanks (equippable weapons and rank with each)
    // TODO: Create custom odin drawer to assign weapons wieldable to unit
    // And only allow selection of ScriptableWeapons created from JSON

    public UnitInventory Inventory { get; private set; }

    
    [FoldoutGroup("Game Status")]
    public int TeamId => Player.TeamId;

    [FoldoutGroup("Game Status")]
    private Weapon _equippedWeapon;
    
    [FoldoutGroup("Game Status")]
    [ShowInInspector] public Weapon EquippedWeapon { get { return _equippedWeapon; } }   // TODO: Implement Gear. EquippedGear. one 1 Gear equipped per unit (ie. shield)
    public bool HasWeapon => EquippedWeapon != null;

    private Vector2Int _gridPosition;
    [FoldoutGroup("Game Status")]
    [ShowInInspector] public Vector2Int GridPosition { get { return _gridPosition; } }

    private Vector2Int _initialGridPosition;
    public Vector2Int InitialGridPosition { get { return _initialGridPosition; } }  // Where Unit began the turn.

    private bool _hasTakenAction;
    [FoldoutGroup("Game Status")]
    [ShowInInspector] public bool HasTakenAction { get { return _hasTakenAction; } }  // Has the unit moved this turn?
    
    private int _currentMovementPoints;
    [FoldoutGroup("Game Status")]
    [ShowInInspector] public int CurrentMovementPoints { get { return _currentMovementPoints; } }



    [FoldoutGroup("Events")]
    public Action<Unit> UponDeath;

    [FoldoutGroup("Events")]
    public Action<Unit> UponLevelUp;


    public bool IsLocalPlayerUnit => Player == Player.LocalPlayer;

    protected virtual Player Player { get; }
    protected Battler Battler;

    private AnimancerComponent _animancer;
    private Vector2 _lookDirection;
    private Dictionary<SurfaceType, string> _footsteps = new Dictionary<SurfaceType, string>();

    
    private Material _allInOneMat; 

    public void Init()
    {
        _gridPosition = GridUtility.SnapToGrid(this);
        WorldGrid.Instance[GridPosition].Unit = this;

        _initialGridPosition = GridPosition;

        SetupFootstepSounds();

        Player.AddUnit(this);

        _unitClass = UnitClass.GetUnitClass();
        InitStats();

        _animancer = GetComponent<AnimancerComponent>();
        Rotate(Direction.Down);
        PlayAnimation(_idleAnimation);

        _allInOneMat = GetComponent<Renderer>().material;

        Inventory = new UnitInventory(this);
        foreach (var item in _startingItems)
            Inventory.AddItem(item.GetItem());

        var weapons = Inventory.GetItems<Weapon>();
        if (weapons.Length > 0)
            EquipWeapon(weapons[0]);
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

        _currentMovementPoints = Stats[UnitStat.Movement].ValueInt;
        _currentHealth = MaxHealth;
    }


    private void SetupFootstepSounds()
    {
        _footsteps[SurfaceType.Grass] = grassFootsteps;
        _footsteps[SurfaceType.Dirt] = dirtFootsteps;
        _footsteps[SurfaceType.Rock] = rockFootsteps;
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

    private void PlayAnimation(DirectionalAnimationSet animations, float speed = 1f)
    {
        var clip = animations.GetClip(_lookDirection);
        _animancer.Play(clip).Speed = speed;
    }

    public void TookAction() 
    {
        _hasTakenAction = true;
        SetInactiveShader();
    }

    private void SetInactiveShader() => _allInOneMat.EnableKeyword("HSV_ON");    
    private void RemoveInactiveShader() => _allInOneMat.EnableKeyword("HSV_ON");    

    public void AllowAction()
    {
        _hasTakenAction = false;
        RemoveInactiveShader();
    }

    public void EquipWeapon(Weapon weapon)
    {
        var availableWeapons = new List<Weapon>();
        foreach(Weapon inventoryWeapon in Inventory.GetItems<Weapon>())
            availableWeapons.Add(inventoryWeapon);

        if (availableWeapons.Contains(weapon))
        {
            UnequipWeapon();
            _equippedWeapon = weapon;
            weapon.IsEquipped = true;
        } else 
            throw new System.Exception($"Provided Weapon: {weapon.Name} is not in Unit#{Name}'s Inventory...");
    }

    public void UnequipWeapon()
    {
        if (EquippedWeapon != null)
            EquippedWeapon.IsEquipped = false;

        _equippedWeapon = null;
    }


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


    public void GainExperience(int amount)
    {
        int currentExpAmount = _experience + amount;
        
        if (currentExpAmount > MAX_EXP_AMOUNT)
        {
            currentExpAmount -= MAX_EXP_AMOUNT;
            UponLevelUp.Invoke(this);
        }
        
        _experience = currentExpAmount;
    }

    public bool CanDefend(Vector2Int attackerPosition)
    {
        bool canAttack = false;

        var immediatePositions = GridUtility.GetReachableCells(this, 0);

        var attackableCells = GridUtility.GetAttackableCells(this, immediatePositions, EquippedWeapon);
        if (attackableCells.Count > 0)
            canAttack = true;
        
        return canAttack;
    }

    public Dictionary<Weapon, List<Vector2Int>> AttackableWeapons() 
    {
        var attackableWeapons = new Dictionary<Weapon, List<Vector2Int>>();

        var immediatePositions = GridUtility.GetReachableCells(this, 0);

        foreach(var weapon in Inventory.GetItems<Weapon>()) {
            var attackableCells = GridUtility.GetAttackableCells(this, immediatePositions, weapon);
            if (attackableCells.Count > 0)
                attackableWeapons[weapon] = attackableCells;
        }

        return attackableWeapons;
    }

    public List<Vector2Int> AllAttackableCells() 
    {
        var allAttackableCells = new List<Vector2Int>();

        var immediatePositions = GridUtility.GetReachableCells(this, 0);

        foreach(var weapon in Inventory.GetItems<Weapon>()) {
            var maxRange = weapon.Stats[WeaponStat.MaxRange].ValueInt;
            var attackableCells = GridUtility.GetAttackableCells(this, immediatePositions, weapon);
            
            if (attackableCells.Count > 0)
                foreach(Vector2Int pos in attackableCells)
                    if (!allAttackableCells.Contains(pos))
                        allAttackableCells.Add(pos);

        }

        return allAttackableCells;
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

    private static bool IsPlaying => Application.isPlaying;
    

    // =====================
    // || Battle Formulas ||
    // =====================

    public int AttackDamage(Weapon weapon)
    {
        int weaponDamage = weapon.Stats[WeaponStat.Damage].ValueInt;
        
        if (weapon.Type == WeaponType.Grimiore) { // MAGIC USER
            int magicStat = Stats[UnitStat.Magic].ValueInt;
            
            return magicStat + weaponDamage;
        } else {
            int strengthStat = Stats[UnitStat.Strength].ValueInt;

            return strengthStat + weaponDamage;
        }
    }

    public int AttackDamage()
    {
        int weaponDamage = EquippedWeapon.Stats[WeaponStat.Damage].ValueInt;
        
        if (EquippedWeapon.Type == WeaponType.Grimiore) { // MAGIC USER
            int magicStat = Stats[UnitStat.Magic].ValueInt;
            
            return magicStat + weaponDamage;
        } else {
            int strengthStat = Stats[UnitStat.Strength].ValueInt;

            return strengthStat + weaponDamage;
        }
    }

    public int AttackSpeed(Weapon weapon)
    {
        int weaponWeight = weapon.Weight;
        int constitutionStat = Stats[UnitStat.Constitution].ValueInt;
        int burden = weaponWeight - constitutionStat;

        if (burden < 0)
            burden = 0;
        
        return Stats[UnitStat.Speed].ValueInt - burden;
    }

    
    public int AttackSpeed()
    {
        int weaponWeight = EquippedWeapon.Weight;
        int constitutionStat = Stats[UnitStat.Constitution].ValueInt;
        int burden = weaponWeight - constitutionStat;

        if (burden < 0)
            burden = 0;
        
        return Stats[UnitStat.Speed].ValueInt - burden;
    }

    public Dictionary<string, int> PreviewAttack(Unit defender, Weapon weapon)
    {
        Dictionary<string, int> battleStats = new Dictionary<string, int>();

        if (!CanAttack(defender))
        {   // -1 == Cannot Attack
            battleStats["ATK_DMG"] = -1;
            battleStats["ACCURACY"] = -1;
            battleStats["CRIT_RATE"] = -1;

            return battleStats;
        }

        int atkDmg;
        if (weapon.Type == WeaponType.Grimiore)
            atkDmg = AttackDamage(weapon) - defender.Stats[UnitStat.Resistance].ValueInt;
        else
            atkDmg = AttackDamage(weapon) - defender.Stats[UnitStat.Defense].ValueInt;

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
        int skill = Stats[UnitStat.Skill].ValueInt;
        int weaponCritChance = weapon.Stats[WeaponStat.CriticalHit].ValueInt;

        int critRate = ((skill / 2) + weaponCritChance) - target.Stats[UnitStat.Luck].ValueInt;
        if (critRate < 0)
            critRate = 0;
        
        // if (WEAPON_RANKS[EquippedWeapon.Type] == Weapon.RANKS["S"])
        //     critRate += 5;

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
        int luckStat = Stats[UnitStat.Luck].ValueInt;
        int skillStat = Stats[UnitStat.Skill].ValueInt;
        int weaponHitStat = weapon.Stats[WeaponStat.Hit].ValueInt;

        int hitRate = (skillStat * 2) + luckStat  + weaponHitStat;
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
            int magicStat = Stats[UnitStat.Magic].ValueInt;
            int resistanceStat = Stats[UnitStat.Resistance].ValueInt;
            int skillStat = Stats[UnitStat.Skill].ValueInt;
            
            var accuracy = (magicStat - resistanceStat) + skillStat + 30 - (boxDistance * 2);
            accuracy = Mathf.Clamp(accuracy, 0, 100);

            return accuracy;
        }
    }
}
