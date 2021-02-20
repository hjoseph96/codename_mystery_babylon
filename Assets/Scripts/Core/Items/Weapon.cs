using System.Collections.Generic;
using UnityEngine;

public class Weapon : Item
{
    public readonly Dictionary<WeaponStat, Stat> Stats = new Dictionary<WeaponStat, Stat>();
    private readonly Dictionary<WeaponStat, EditorWeaponStat> _brokenStats = new Dictionary<WeaponStat, EditorWeaponStat>();

    public bool IsBroken => CurrentDurability == 0;

    public override string Description => IsBroken ? DescriptionNormal : DescriptionBroken;
    public readonly string DescriptionNormal, DescriptionBroken; 

    public readonly WeaponRank RequiredRank;
    public readonly WeaponType Type;

    private ScriptableWeapon _source;

    public Weapon(ScriptableWeapon source)
    {
        _source = source;

        foreach (var key in source.WeaponStats.Keys)
        {
            Stats[key] = new Stat(key.ToString(), source.WeaponStats[key].BaseValue);
            _brokenStats[key] = source.WeaponStats[key];
        }

        DescriptionNormal = source.Description;
        DescriptionBroken = source.DescriptionBroken;

        Stats[WeaponStat.MinRange] = new Stat(WeaponStat.MinRange.ToString(), source.AttackRange.x);
        Stats[WeaponStat.MaxRange] = new Stat(WeaponStat.MinRange.ToString(), source.AttackRange.y);

        Name = source.Name;
        Icon = source.Icon;
        Weight = source.Weight;
        MaxDurability = source.MaxDurability;
        CurrentDurability = MaxDurability;
        
        Type = source.Type;
        RequiredRank = source.RequiredRank;
    }

    // TODO: This has to do with a weapon being used in battle. Typically, the use only counts if the hit lands
    // Unless it is a ranged weapon like bows, throwable lances/axes, and magic
    // TODO: Some Weapons have a Use option like consumables. So maybe a UseAbility() function or something.
    public void Use(int times = 1)
    {
        CurrentDurability = Mathf.Max(CurrentDurability - times, 0);
        if (CurrentDurability == 0)
            Break();
    }

    public void Break()
    {
        foreach (var key in _brokenStats.Keys)
        {
            Stats[key].RawValue = _brokenStats[key].BrokenValue;
        }
    }

    public void Repair(int times = -1)
    {
        // Negative value = full repair
        if (times < 0)
            times = MaxDurability;

        CurrentDurability = Mathf.Min(CurrentDurability + times, MaxDurability);

        foreach (var key in _brokenStats.Keys)
        {
            Stats[key].RawValue = _brokenStats[key].BaseValue;
        }
    }
}