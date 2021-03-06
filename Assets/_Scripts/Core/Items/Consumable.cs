using System;
using System.Collections.Generic;

public class Consumable : Item
{
    private readonly ScriptableConsumable _source;
    private readonly bool _canHeal;
    public bool CanHeal => _canHeal;

    public int MaxDurability { get; protected set; }
    public int CurrentDurability { get; protected set; }


    public Consumable(ScriptableConsumable source) : base(source)
    {
        _source     = source;
        _canHeal    = source.canHeal;
        MaxDurability       = source.MaxDurability;
        CurrentDurability   = source.CurrentDurability;
    }

    /// <summary>
    /// Needed to return copies of items for replacement from convoy when stacked
    /// </summary>
    /// <returns></returns>
    public Consumable CopyData()
    {
        var consumable = new Consumable(_source);
        consumable.CurrentDurability = CurrentDurability;
        consumable.amount = 1;
        return consumable;
    }

    public override void UseItem()
    {
        if (CurrentDurability == 1)
            Unit.Inventory.RemoveItem(this);
        else
            CurrentDurability -= 1;

        _source.Action.Use(Unit);
    }

    public override IEnumerable<Type> GetUIOptions()
    {
        var options = new List<Type>
        {
            typeof(UseOption), typeof(DropOption)
        };

        return options;
    }
}