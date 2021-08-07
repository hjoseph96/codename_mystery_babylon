using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;

/// <summary>
/// Holds both normal (weapons consumables etc) and key items in separated lists with proper checks in place 
/// </summary>
public class UnitInventory
{
    #region Variables and Getters
    private readonly List<Item> _items = new List<Item>(MAX_SIZE);
    private readonly List<Item> _keyItems = new List<Item>();

    public readonly Unit Unit;
    public const int MAX_SIZE = 6;

    public Item this[int index] => _items[index];

    /// <summary>
    /// Returns the size of the normal items inventory, limited by inventory size
    /// </summary>
    public int Size => _items.Count;

    /// <summary>
    /// Returns true if normal inventory item count is currently full
    /// </summary>
    public bool IsFull => Size == MAX_SIZE;

    public T[] GetItems<T>() where T : Item => _items.FilterCast<T>().ToArray(); 

    public T[] GetKeyItems<T>() where T : Item => _keyItems.FilterCast<T>().ToArray(); 

    public UnitInventory(Unit unit) => Unit = unit;
    #endregion

    #region Adding Methods
    /// <summary>
    /// Adds an item to the _items list that is limited by inventory size
    /// </summary>
    private bool AddNormalItem(Item item)
    {
        if (IsFull)
            return false;

        _items.Add(item);
        item.Unit = Unit;

        return true;
    }

    /// <summary>
    /// Adds an item into the _keyItems list that is not hindered by inventory size
    /// </summary>
    private bool AddKeyItem(Item item)
    {
        _keyItems.Add(item);
        item.Unit = Unit;
        return true;
    }
    #endregion

    #region Trading Methods
    /// <summary>
    /// Used when trading items to another inventory
    /// </summary>
    public void MoveItem(Item item, UnitInventory otherInventory, bool usesAction = true)
    {
        RemoveItem(item);
        otherInventory.AddItem(item);
    }

    /// <summary>
    /// Used when trading items to another inventory
    /// </summary>
    public void ExchangeItems(Item item, Item otherItem, UnitInventory otherInventory, bool usesAction = true)
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
    #endregion

    #region Removing Methods
    /// <summary>
    /// Removes an Item at a specific index, used only for the main inventory
    /// </summary>
    private bool RemoveItem(int index)
    {
        if (index < 0 || index >= Size)
            return false;

        _items[index].Drop();
        _items.RemoveAt(index);
        return true;
    }

    /// <summary>
    /// Removes key items from a unit, without modifying inventory size
    /// </summary>
    private bool RemoveKeyItem(int index)
    {
        if (index < 0 || index >= _keyItems.Count)
            return false;

        _keyItems[index].Drop();
        _keyItems.RemoveAt(index);
        return true;
    }
    #endregion

    #region Lambda methods
    /// <summary>
    /// Used when trading items to another inventory
    /// </summary>
    public void SetItem(int index, Item item) => _items[index] = item;

    /// <summary>
    /// Addes an item to the appropriate item pool, based on key item, or otherwise
    /// </summary>
    /// <param name="item"></param>
    /// <returns>if item addition was successful</returns>
    public bool AddItem(Item item) => (item.ItemType == ItemType.KeyItem) ? AddKeyItem(item) : AddNormalItem(item);

    /// <summary>
    /// When removing an item this will remove it from the appropriate list, keyitem or otherwise
    /// </summary>
    /// <param name="item"></param>
    /// <returns>if item removal was successful</returns>
    public bool RemoveItem(Item item) => (item.ItemType == ItemType.KeyItem) ? RemoveKeyItem(_keyItems.IndexOf(item)) : RemoveItem(_items.IndexOf(item));

    /// <summary>
    /// Used to get the index when performing a trade
    /// </summary>
    public int IndexOf(Item item) => _items.IndexOf(item);
    #endregion
}