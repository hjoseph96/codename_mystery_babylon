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

        var bestKey = UnitType.None;
        var en = dictionary.GetEnumerator();
        while (en.MoveNext())
        {
            var currentKey = en.Current.Key;
            if ((currentKey & key) == key && (bestKey == UnitType.None || (int) currentKey < (int) bestKey))
            {
                bestKey = currentKey;
            }
        }
        en.Dispose();

        if (bestKey == UnitType.None)
        {
            result = default;
            return false;
        }
        else
        {
            result = dictionary[bestKey];
            return true;
        }
    }
}