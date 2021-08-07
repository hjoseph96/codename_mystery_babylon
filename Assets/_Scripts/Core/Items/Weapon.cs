using System;
using System.Collections.Generic;
using UnityEngine;


public class Weapon : Item
{
    public override string Description => !IsBroken ? DescriptionNormal : DescriptionBroken;
    public readonly string DescriptionNormal, DescriptionBroken; 

    public readonly string MeleeSound;

    public readonly WeaponRank RequiredRank;
    public readonly WeaponType Type;
    public readonly MagicType MagicType;
    public readonly Dictionary<WeaponStat, Stat> Stats = new Dictionary<WeaponStat, Stat>();
    private readonly Dictionary<WeaponStat, EditorWeaponStat> _brokenStats = new Dictionary<WeaponStat, EditorWeaponStat>();

    public readonly bool IsUsable;
    private readonly ScriptableWeapon _source;


    public int Damage    => Stats[WeaponStat.Damage].ValueInt;
    public int CritRate  => Stats[WeaponStat.CriticalHit].ValueInt;
    public int HitChance => Stats[WeaponStat.Hit].ValueInt;
    public int MaxRange  => Stats[WeaponStat.MaxRange].ValueInt;
    public int MinRange  => Stats[WeaponStat.MinRange].ValueInt;

    public int Weight { get; protected set; }
    public int MaxDurability { get; protected set; }
    public int CurrentDurability { get; protected set; }



    public bool IsBroken => CurrentDurability == 0;
    public readonly GameObject magicCirclePrefab;
    public readonly MagicEffect magicEffect;
    public readonly string castingSound; 


    public Weapon(ScriptableWeapon source) : base(source)
    {
        _source = source;


        MeleeSound = source.meleeSound;
        DescriptionNormal = source.Description;
        DescriptionBroken = source.DescriptionBroken;

        foreach (var key in source.WeaponStats.Keys)
        {
            Stats[key] = new Stat(key.ToString(), source.WeaponStats[key].BaseValue);
            _brokenStats[key] = source.WeaponStats[key];
        }

        Stats[WeaponStat.MinRange] = new Stat(WeaponStat.MinRange.ToString(), source.AttackRange.x);
        Stats[WeaponStat.MaxRange] = new Stat(WeaponStat.MinRange.ToString(), source.AttackRange.y);

        Name    = source.Name;
        Icon    = source.Icon;
        Weight  = source.Weight;
        MaxDurability = source.MaxDurability;
        CurrentDurability = MaxDurability;
        
        Type            = source.Type;
        RequiredRank    = source.RequiredRank;

        IsUsable = source.IsUsable;

        if (Type == WeaponType.Grimiore)
        {
            MagicType           = source.MagicType;
            castingSound        = source.castingSound;
            magicEffect         = source.magicEffect;
            magicCirclePrefab   = source.magicCirclePrefab;
        }
    }

    /// <summary>
    /// Needed to return copies of items for replacement from convoy when stacked
    /// </summary>
    /// <returns></returns>
    public Weapon CopyData()
    {
        var weapon = new Weapon(_source);
        weapon.CurrentDurability = CurrentDurability;
        weapon.amount = 1;
        if (weapon.CurrentDurability == 0)
            weapon.Break();
        return weapon;
    }

    public void Use(int times = 1)
    {
        CurrentDurability = Mathf.Max(CurrentDurability - times, 0);
        if (CurrentDurability == 0)
            Break();
    }

    public void Break()
    {
        foreach (var key in _brokenStats.Keys)
            Stats[key].RawValue = _brokenStats[key].BrokenValue;
    }

    public void Repair(int times = -1)
    {
        // Negative value = full repair
        if (times < 0)
            times = MaxDurability;

        CurrentDurability = Mathf.Min(CurrentDurability + times, MaxDurability);

        foreach (var key in _brokenStats.Keys)
            Stats[key].RawValue = _brokenStats[key].BaseValue;
    }

    public override IEnumerable<Type> GetUIOptions()
    {
        var options = new List<Type>();

        if (CanWield)
            options.Add(IsEquipped ? typeof(UnequipOption) : typeof(EquipOption));

        if (IsUsable)
            options.Add(typeof(UseOption));

        options.Add(typeof(DropOption));

        return options;
    }

    public override void Drop()
    {
        if (IsEquipped)
            Unit.UnequipWeapon();
        
        base.Drop();
    }

    public override void UseItem()
    {
        if (IsUsable)
            _source.Action.Use(Unit);
    }
}