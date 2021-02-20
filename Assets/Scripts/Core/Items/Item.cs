using UnityEngine;

public class Item
{
    // TODO: I wonder if we should user an interface? IEquippable, IUseable?
    // TODO: Create Gear, Consumable, and Valuable
    public readonly ItemType ItemType;

    public string Name { get; protected set; }
    public Sprite Icon { get; protected set; }

    public virtual string Description { get; protected set; }

    public int Weight { get; protected set; }
    public int MaxDurability { get; protected set; }
    public int CurrentDurability { get; protected set; }

    public virtual void Equip()
    { }

    public virtual void Drop()
    { }
}