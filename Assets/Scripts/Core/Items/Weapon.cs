using System;
using System.Collections.Generic;
using UnityEngine;


public class Weapon : Item
{
    public override string Description => !IsBroken ? DescriptionNormal : DescriptionBroken;
    public readonly string DescriptionNormal, DescriptionBroken; 

    public string MeleeSound;

    public readonly WeaponRank RequiredRank;
    public readonly WeaponType Type;
    public readonly Dictionary<WeaponStat, Stat> Stats = new Dictionary<WeaponStat, Stat>();
    private readonly Dictionary<WeaponStat, EditorWeaponStat> _brokenStats = new Dictionary<WeaponStat, EditorWeaponStat>();

    public readonly bool IsUsable;
    private readonly ScriptableWeapon _source;



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

        Name = source.Name;
        Icon = source.Icon;
        Weight = source.Weight;
        MaxDurability = source.MaxDurability;
        CurrentDurability = MaxDurability;
        
        Type = source.Type;
        RequiredRank = source.RequiredRank;

        IsUsable = source.IsUsable;

        if (Type == WeaponType.Grimiore)
        {
            castingSound = source.castingSound;
            magicEffect = source.magicEffect;
            magicCirclePrefab = source.magicCirclePrefab;
        }
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
        var options = new List<Type>
        {
            IsEquipped ? typeof(UnequipOption) : typeof(EquipOption)
        };

        if (IsUsable)
            options.Add(typeof(UseOption));

        options.Add(typeof(DropOption));

        return options;
    }

    public override void Drop()
    {
        if (IsEquipped)
            Unit.UnequipWeapon();
    }

    public override void UseItem()
    {
        if (IsUsable)
            _source.Action.Use(Unit);
    }
}