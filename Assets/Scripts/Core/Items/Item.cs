using System;
using System.Collections.Generic;
using UnityEngine;

public class Item
{
    // TODO: Create Gear
    public readonly ItemType ItemType;

    public string Name { get; protected set; }
    public Sprite Icon { get; protected set; }

    public virtual string Description { get; protected set; }

    public Unit Unit { get; set; }

    public Item(ScriptableItem source)
    {
        Name = source.Name;
        Icon = source.Icon;
        Description = source.Description;

    }

    public virtual void UseItem()
    { }

    public virtual void Equip()
    { }

    public virtual void Drop()
    { }

    public virtual IEnumerable<Type> GetUIOptions()
    {
        throw new NotImplementedException();
    }
}