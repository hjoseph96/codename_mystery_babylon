using System;
using System.Collections.Generic;

public class Valuable : Item
{
    private readonly ScriptableValuable _source;

    public Valuable(ScriptableValuable source) : base(source)
    {
        _source = source;
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