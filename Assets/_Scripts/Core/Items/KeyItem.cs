using System;
using System.Collections.Generic;

public class KeyItem : Item
{
    private readonly ScriptableKeyItem _source;

    public KeyItem(ScriptableKeyItem source) : base(source)
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
