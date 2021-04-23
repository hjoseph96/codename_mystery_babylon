using System;
using System.Collections.Generic;
using UnityEngine;

public class Item
{
    // TODO: Create Gear
    public readonly ItemType ItemType;

    public string Name { get; protected set; }
    public Sprite Icon { get; protected set; }

    public uint Cost { get; protected set; }
    public uint SaleValue { get; protected set; }


    public virtual string Description { get; protected set; }

    public Unit Unit { get; set; }
    public bool IsEquipped { get; set; }

    // Only relevant to Weapons... I should move it there and just so `Item as Weapon`
    public bool CanWield { get; set; }


    public Item(ScriptableItem source)
    {
        Name        = source.Name;
        ItemType    = source.ItemType;
        Icon        = source.Icon;
        Cost        = source.Cost;
        SaleValue   = source.SaleValue;
        Description = source.Description;
    }

    public virtual void UseItem()
    { }

    public virtual void Equip()
    { }

    public virtual void Drop()
    { }

    public virtual IEnumerable<Type> GetUIOptions() => throw new NotImplementedException();
}