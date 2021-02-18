using System.Collections.Generic;
using UnityEngine;

public class Weapon : Item
{
    public readonly Dictionary<WeaponStat, Stat> Stats = new Dictionary<WeaponStat, Stat>();
    private readonly Dictionary<WeaponStat, EditorWeaponStat> _brokenStats = new Dictionary<WeaponStat, EditorWeaponStat>();

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

        Stats[WeaponStat.MinRange] = new Stat(WeaponStat.MinRange.ToString(), source.AttackRange.x);
        Stats[WeaponStat.MaxRange] = new Stat(WeaponStat.MinRange.ToString(), source.AttackRange.y);

        this.name = source.Name;
        this.icon = source.Icon;
        this.weight = source.Weight;
        this.maxDurability = source.MaxDurability;
        this.currentDurability = MaxDurability;
        
        Type = source.Type;
        RequiredRank = source.RequiredRank;
    }

    public void Use(int times = 1)
    {
        currentDurability = Mathf.Max(currentDurability - times, 0);
        if (currentDurability == 0)
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

        currentDurability = Mathf.Min(currentDurability + times, MaxDurability);

        foreach (var key in _brokenStats.Keys)
        {
            Stats[key].RawValue = _brokenStats[key].BaseValue;
        }
    }
}