using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;

public class UnitInventory
{
    public const int MaxSize = 6;

    public Item this[int index] => _items[index];

    private readonly List<Item> _items = new List<Item>(MaxSize);
    public readonly Unit Unit;

    public int Size     => _items.Count;
    public bool IsFull  => Size == MaxSize;

    public bool HasItem(Item item) => _items.Contains(item);

    public T[] GetItems<T>() where T : Item => _items.FilterCast<T>().ToArray();

    public UnitInventory(Unit unit) => Unit = unit;

    public bool AddItem(Item item)
    {
        if (IsFull)
            return false;

        _items.Add(item);
        item.Unit = Unit;

        return true;
    }

    public void SetItem(int index, Item item) => _items[index] = item;

    public void MoveItem(Item item, UnitInventory otherInventory)
    {
        RemoveItem(item);
        otherInventory.AddItem(item);
    }

    public void ExchangeItems(Item item, Item otherItem, UnitInventory otherInventory)
    {
        var ind1 = IndexOf(item);
        var ind2 = otherInventory.IndexOf(otherItem);

        item.Drop();
        otherItem.Drop();

        SetItem(ind1, otherItem);
        otherInventory.SetItem(ind2, item);

        item.Unit       = otherInventory.Unit;
        otherItem.Unit  = Unit;
    }

    public bool RemoveItem(Item item) => RemoveItem(_items.IndexOf(item));

    public bool RemoveItem(int index)
    {
        if (index < 0 || index >= Size)
            return false;

        _items[index].Drop();
        _items.RemoveAt(index);

        return true;
    }

    public int IndexOf(Item item)   => _items.IndexOf(item);
}