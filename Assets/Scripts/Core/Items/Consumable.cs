using System;
using System.Collections.Generic;

public class Consumable : Item
{
    private readonly ScriptableConsumable _source;

    public Consumable(ScriptableConsumable source) : base(source)
    {
        _source = source;
    }

    public override void UseItem()
    {
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