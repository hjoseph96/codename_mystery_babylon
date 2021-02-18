using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;

public class UnitInventory
{
    public const int MaxSize = 6;

    public Item this[int index] => _items[index];

    private readonly List<Item> _items = new List<Item>(MaxSize);

    public int Size => _items.Count;
    public bool IsFull => Size == MaxSize;

    public bool HasItem(Item item) => _items.Contains(item);

    public T[] GetItems<T>() where T : Item => _items.FilterCast<T>().ToArray();

    public bool AddItem(Item item)
    {
        if (IsFull)
            return false;

        _items.Add(item);
        return true;
    }

    public bool RemoveItem(Item item)
    {
        var index = _items.IndexOf(item);
        if (index >= 0)
        {
            item.Drop();
            _items[index] = null;
            return true;
        }

        return false;
    }

    public bool RemoveItem(int index)
    {
        if (index < 0 || index >= Size)
            return false;

        _items.RemoveAt(index);
        return true;
    }
}