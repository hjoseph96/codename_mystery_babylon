using System;
using System.Collections.Generic;

public class Consumable : Item
{
    private readonly ScriptableConsumable _source;
    private readonly bool _canHeal;
    public bool CanHeal => _canHeal;


    public Consumable(ScriptableConsumable source) : base(source)
    {
        _source = source;
        _canHeal = source.canHeal;
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