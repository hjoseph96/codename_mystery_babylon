using Articy.Unity;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// A storage for the items the player uses throughout the game, storing the excess items the player units cannot hold
/// </summary>
public class Convoy : MonoBehaviour
{
    #region Variables
    private List<Item> _items = new List<Item>();
    private int _maxInventorySize = 400;
    public List<ScriptableItem> itemstoAdd = new List<ScriptableItem>();
    public System.Action<Item, bool> itemRemovedFromList;

    #endregion

    private void Start()
    {
        for(int i = 0; i < itemstoAdd.Count; i++)
        {
            _items.Add(itemstoAdd[i].GetItem());
        }
    }

    public bool IsConvoyFull()
    {
        int amount = 0;
        for(int i = 0; i < _items.Count; i++)
        {
            amount += _items[i].amount;
        }
        return amount >= _maxInventorySize;
    }

    public int ItemCount()
    {
        int amount = 0;
        for (int i = 0; i < _items.Count; i++)
        {
            amount += _items[i].amount;
        }
        return amount;
    }

    #region Set and Get Inventory
    /// <summary>
    /// Load the items from the save file **Perhaps pull this out and put it into the load/save manager to find this script or assign the data**
    /// </summary>
    public void LoadItems(int saveFileId)
    {

    }

    /// <summary>
    /// Should be used to collect the items for saving purposes ** Could be changed to do the work here and return that value? **
    /// </summary>
    /// <returns></returns>
    public List<Item> GetInventoryItems()
    {
        return _items;
    }
    #endregion


    #region Add and Remove Items
    /// <summary>
    /// Adds an item to the inventory if it's not full, otherwise, will not complete the move. Use IsConvoyFull outside to determine availability for moving
    /// </summary>
    /// <param name="item"></param>
    public void AddItem(Item item)
    {
        if (!IsConvoyFull())
        {
            bool add = true;
            // If item is stackable check to see if a stackable item exists, and if so stack it, otherwise, add it manually
            if(item.ItemType == ItemType.Weapon && item is Weapon w)
            {
                for(int i = 0; i < _items.Count; i++)
                {
                    if (_items[i] is Weapon stored)
                    {
                        if (stored.CurrentDurability == w.CurrentDurability && stored.Name == w.Name)
                        {
                            Debug.Log("Found a stored weapon matching, adding ++ to the amount");
                            stored.amount++;
                            add = false;
                            itemRemovedFromList?.Invoke(item, true);
                            break;
                        }
                    }
                }
            }
            else if(item.ItemType == ItemType.Consumable && item is Consumable c)
            {
                for(int i = 0; i < _items.Count; i++)
                {
                    if(_items[i] is Consumable stored)
                    {
                        if (stored.CurrentDurability == c.CurrentDurability && stored.Name == c.Name)
                        {
                            Debug.Log("Found a stored consumable matching, adding ++ to the amount");
                            stored.amount++;
                            add = false;
                            itemRemovedFromList?.Invoke(item, true);
                            break;
                        }
                    }
                }
            }
            
            if(add)
                _items.Add(item);
        }
        else
            Debug.LogWarning("Inventory is full, not adding item to the convoy");
    }

    /// <summary>
    /// Removes an item at the specified index, Calls RemoveItem(Item item) after getting item
    /// <br>Unlikely to be used, delete this method if no calls are made to it at any point</br>
    /// </summary>
    /// <param name="index"></param>
    public void RemoveItem(int index)
    {
        if (index >= 0 && _items.Count - 1 < index)
            RemoveItem(_items[index]);
        else
            Debug.LogWarning("Item index is outside the parameters of the inventory, not removing anything");
    }

    /// <summary>
    /// Removes an item specified by item
    /// </summary>
    /// <param name="item"></param>
    public void RemoveItem(Item item)
    {
        bool found = false;

        if(item.ItemType == ItemType.Weapon)
        {
            if (item is Weapon w) 
            {
                var foundItems = _items.Where(stored => stored.ItemType == ItemType.Weapon && stored.Name == w.Name).ToList();
                for(int i = 0; i < foundItems.Count; i++)
                {
                    if(foundItems[i] is Weapon weapon)
                    {
                        if(weapon.CurrentDurability == w.CurrentDurability)
                        {
                            Debug.Log("removing weapon from convoy matching criteria");
                            found = true;
                            weapon.amount--;
                            if (weapon.amount == 0)
                            {
                                _items.Remove(item);
                                itemRemovedFromList?.Invoke(item, false);
                            }
                            break;
                        }
                    }
                }
            }
        }
        else if(item.ItemType == ItemType.Consumable)
        {
            if (item is Consumable c)
            {
                var foundItems = _items.Where(stored => stored.ItemType == ItemType.Consumable && stored.Name == c.Name).ToList();
                for (int i = 0; i < foundItems.Count; i++)
                {
                    if (foundItems[i] is Consumable consumable)
                    {
                        if (consumable.CurrentDurability == c.CurrentDurability)
                        {
                            Debug.Log("removing consumable from convoy matching criteria");
                            found = true;
                            consumable.amount--;
                            if (consumable.amount == 0)
                            {
                                _items.Remove(item);
                                itemRemovedFromList?.Invoke(item, false);
                            }
                            break;
                        }
                    }
                }
            }
        }
        // if item is not one of the above types, we can simply remove it if we find it
        else if (_items.Contains(item))
        {
            _items.Remove(item);
            itemRemovedFromList?.Invoke(item, false);
            found = true;
        }

        if (!found)
            Debug.LogWarning("Cannot find item in inventory, ensure item removal is set up correctly");

    }
    #endregion

}
