using System.Collections.Generic;
using System.Linq;

public static class UnitTypeDictionaryExtensions
{
    public static bool TryGetValueExt<T>(this Dictionary<UnitType, T> dictionary, UnitType key, out T result)
    {
        if (dictionary.Count == 0)
        {
            result = default;
            return false;
        }

        var keys = dictionary.Keys.Where(type => (type & key) == key).ToArray();
        if (keys.Length == 0)
        {
            result = default;
            return false;
        }

        var bestKey = keys.OrderBy(type => (int) type).First();
        result = dictionary[bestKey];
        return true;
    }
}