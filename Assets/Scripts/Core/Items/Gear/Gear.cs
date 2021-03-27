using System;
using System.Collections.Generic;
using UnityEngine;


public class Gear : Item
{
    public override string Description => DescriptionNormal;
    public readonly string DescriptionNormal;

    
    public readonly GearType Type;
    public readonly Dictionary<GearStat, Stat> Stats = new Dictionary<GearStat, Stat>();
   

    public readonly bool IsUsable;
    private readonly ScriptableGear _source;



    public int Weight { get; protected set; }


    public Gear(ScriptableGear source) : base(source)
    {
        _source = source;


        DescriptionNormal = source.Description;


        foreach (var key in source.GearStats.Keys)
        {
            Stats[key] = new Stat(key.ToString(), source.GearStats[key].BaseValue);
           
        }

        Name = source.Name;
        Icon = source.Icon;
        Weight = source.Weight;


        Type = source.Type;

        IsUsable = source.IsUsable;
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