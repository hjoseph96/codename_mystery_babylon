using UnityEngine;

public class Item
{
    protected string name;
    public string Name { get { return name; } }

    protected Sprite icon;
    public Sprite Icon { get { return icon; } }
    

    protected int weight, maxDurability, currentDurability;
    public int Weight { get { return weight; } }
    public int MaxDurability { get { return maxDurability; } }
    public int CurrentDurability { get {return currentDurability; } }

    public virtual void Equip()
    { }

    public virtual void Drop()
    { }
}