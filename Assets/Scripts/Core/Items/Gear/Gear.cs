using System;
using System.Collections.Generic;
using UnityEngine;


public class Gear : Item
{
    public override string Description => !IsBroken ? DescriptionNormal : DescriptionBroken;
    public readonly string DescriptionNormal, DescriptionBroken;

    public readonly GearRank RequiredRank;
    public readonly GearType Type;
    public readonly Dictionary<GearStat, Stat> Stats = new Dictionary<GearStat, Stat>();
    private readonly Dictionary<GearStat, EditorGearStat> _brokenStats = new Dictionary<GearStat, EditorGearStat>();

    public readonly bool IsUsable;
    private readonly ScriptableGear _source;



    public int Weight { get; protected set; }
    public int MaxDurability { get; protected set; }
    public int CurrentDurability { get; protected set; }

    public bool IsBroken => CurrentDurability == 0;
    public readonly GameObject magicCirclePrefab;
    public readonly MagicEffect magicEffect;
    public readonly string castingSound;


    public Gear(ScriptableGear source) : base(source)
    {
        _source = source;


        DescriptionNormal = source.Description;
        DescriptionBroken = source.DescriptionBroken;

        foreach (var key in source.GearStats.Keys)
        {
            Stats[key] = new Stat(key.ToString(), source.GearStats[key].BaseValue);
            _brokenStats[key] = source.GearStats[key];
        }

        Name = source.Name;
        Icon = source.Icon;
        Weight = source.Weight;
        MaxDurability = source.MaxDurability;
        CurrentDurability = MaxDurability;

        Type = source.Type;
        RequiredRank = source.RequiredRank;

        IsUsable = source.IsUsable;
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

        options.Add(typeof(DropOption));

        return options;
    }

    public override void Drop()
    {
        if (IsEquipped)
            Unit.UnequipWeapon();
    }
}