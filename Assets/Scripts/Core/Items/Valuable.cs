using System;
using System.Collections.Generic;

public class Valuable : Item
{
    public readonly uint Cost;

    private readonly ScriptableValuable _source;

    public Valuable(ScriptableValuable source) : base(source)
    {
        _source = source;
        Cost = source.Cost;
    }

    public override IEnumerable<Type> GetUIOptions()
    {
        var options = new List<Type>
        {
            typeof(DropOption)
        };

        return options;
    }
}