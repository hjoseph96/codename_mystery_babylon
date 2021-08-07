using Sirenix.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConvoyMenu : Menu
{

    public GameObject content;
    public Transform prefabParent;
    public ConvoyItemSlot prefab;
    public ScrollRect scroll;

    public List<ConvoyItemSlot> options = new List<ConvoyItemSlot>();
    public UICursor _cursor;
    private int _selectedOptionIndex;

    public TextMeshProUGUI quantityText;
    public ConvoyActionMenu actionMenu;

    public System.Action OnSlotsChanged;

    public Convoy convoy;

    /// <summary>
    /// Loads items from the Player Units on Campaign Manager, and from the convoy, and then loads the item into the list
    /// </summary>
    public void CreateItemsList()
    {
        var playerUnits = EntityManager.Instance.PlayerUnits();        
        
        for(int i = 0; i < playerUnits.Count; i++)
        {
            var unitItems = playerUnits[i].Inventory.GetItems<Item>().ToList();
            unitItems.AddRange(playerUnits[i].Inventory.GetKeyItems<KeyItem>());

            for(int j = 0; j < unitItems.Count(); j++)
            {
                CreateConvoyItemSlot(unitItems[j]);
            }
        }

        var convoyItems = convoy.GetInventoryItems();
        List<Consumable> consumables = new List<Consumable>();
        List<Weapon> weapons = new List<Weapon>();
        
        // Go over the items, and add non stacking items as new ConvoyItemSlots immediately, store weapons and consumables for checking for amounts
        for(int i = 0; i < convoyItems.Count; i++)
        {
            if (convoyItems[i].ItemType == ItemType.Weapon)
            {
                if(convoyItems[i] is Weapon w)
                {
                    var storedWeapon = weapons.Find(weap => weap.Name == w.Name && weap.CurrentDurability == w.CurrentDurability);
                    if (storedWeapon != null)
                    {
                        Debug.Log("Found a convoy weapon that matches, adding 1 to the amount");
                        storedWeapon.amount++;
                    }
                    else
                    {
                        Debug.Log("Adding a new weapon to the convoyItemSlots list for display");
                        weapons.Add(convoyItems[i] as Weapon);
                    }
                }
            }
            else if (convoyItems[i].ItemType == ItemType.Consumable)
            {
                if (convoyItems[i] is Consumable c)
                {
                    var storedConsumable = consumables.Find(consum => consum.Name == c.Name && consum.CurrentDurability == c.CurrentDurability);
                    if (storedConsumable != null)
                    {
                        Debug.Log("Found a convoy consumable that matches, adding 1 to the amount");
                        storedConsumable.amount++;
                    }
                    else
                    {
                        Debug.Log("Adding a new consumable to the convoyItemSlots list for display");
                        consumables.Add(convoyItems[i] as Consumable);
                    }
                }
            }
            else
            {
                CreateConvoyItemSlot(convoyItems[i]);
            }
        }
        
        // Go Over weapons and consumables now and add them to the convoyItemSlot after they have been tallied together.
        for(int i = 0; i < consumables.Count; i++)
        {
            CreateConvoyItemSlot(consumables[i]);
        }
        for (int i = 0; i < weapons.Count; i++)
        {
            CreateConvoyItemSlot(weapons[i]);
        }

        quantityText.text = string.Format("{0}/400", convoy.ItemCount());
    }

    /// <summary>
    /// When selecting an item, activate the context menu, and pass the item to it
    /// </summary>
    /// <param name="itemSlot"></param>
    public void OnConvoyItemSelected(ConvoyItemSlot itemSlot) 
    {
        actionMenu.PreviousMenu = this;
        actionMenu.SetSelectedItem(itemSlot);
        actionMenu.Activate();
    }

    public void Start() 
    {
        actionMenu.PerformedChange += delegate
        {
            Debug.Log("Refreshing slots");
            OnSlotsChanged?.Invoke();
            scroll.content.position = Vector3.zero;
        };

        actionMenu.PerformRebuildCheck += RefreshItems;
        convoy.itemRemovedFromList += RemoveItemFromList;
    }


    private void OnDestroy()
    {
        convoy.itemRemovedFromList -= RemoveItemFromList;
        actionMenu.PerformRebuildCheck -= RefreshItems;
    }

    private void RemoveItemFromList(Item item, bool refreshAll)
    {
        for (int i = 0; i < options.Count; i++)
        {
            if (options[i].Item == item)
            {
                var option = options[i];
                options.RemoveAt(i);
                RefreshItems(allItems: refreshAll);
                Destroy(option.gameObject);
            }
        }
    }

    private void RefreshItems(Item itemToAdd = null, bool newItem = false, bool allItems = false)
    {
        if (newItem)
            CreateConvoyItemSlot(itemToAdd);
        options = options.OrderBy(item => (item.Item.Unit != null) ? item.Item.Unit.Name : "ZZZ").ToList();
        for (int i = 0; i < options.Count; i++)
        {
            if (allItems)
                options[i].changedOwner = true;
            options[i].transform.SetSiblingIndex(i);
        }
        TakeControl();
        quantityText.text = string.Format("{0}/400", convoy.ItemCount());

    }

    public void CreateConvoyItemSlot(Item Item)
    {
        ConvoyItemSlot item = GameObject.Instantiate(prefab, prefabParent).GetComponent<ConvoyItemSlot>();
        item.Populate(Item, this);
        options.Add(item);
    }

    #region Inherited Methods
    /// <summary>
    /// Recreates item list, and assigns control to this item
    /// </summary>
    public override void Activate()
    {
        CreateItemsList();
        content.SetActive(true);
        TakeControl();
    }

    /// <summary>
    /// Assign control to this object for the input manager used from the Convoy context menu and Through Activate
    /// </summary>
    public void TakeControl()
    {
        UserInput.Instance.InputTarget = this;
        _selectedOptionIndex = 0;
        MoveSelectionToOption(_selectedOptionIndex, true);
        scroll.content.position = Vector3.zero;
    }

    public override void Close() { }

    /// <summary>
    /// This is Main method to close Convoy Menu
    /// <br></br>Destroys the items list, allowing refreshing of them after changing data.
    /// </summary>
    public override void Deactivate()
    {

        UserInput.Instance.InputTarget = PreviousMenu;
        PreviousMenu.Activate();
        content.SetActive(false);
        _cursor.transform.SetParent(transform, false);
        for (int i = 0; i < options.Count; i++)
        {
            Destroy(options[i].gameObject);
        }
        options.Clear();
    }

    public override MenuOption MoveSelection(Vector2Int input)
    {
        if (!_cursor.IsMoving)
            MoveSelection(-input.y);

        return options[_selectedOptionIndex];
    }

    private void MoveSelection(int input)
    {
        if (input == 0)
            return;

        var newIndex = Mathf.Clamp(_selectedOptionIndex + input, 0, options.Count - 1);


        if (_selectedOptionIndex != newIndex)
        {
            _selectedOptionIndex = newIndex;

            MoveSelectionToOption(_selectedOptionIndex);
        }
    }

    private void MoveSelectionToOption(int index, bool instant = false)
    {
        _cursor.transform.SetParent(options[index].transform, false);
        _cursor.MoveTo(new Vector2(-200, 0), instant);
        SnapTo(options[index].GetComponent<RectTransform>());
    }

    public void SnapTo(RectTransform child)
    {
        var contentPos = (Vector2)scroll.transform.InverseTransformPoint(scroll.content.position);
        var childPos = (Vector2)scroll.transform.InverseTransformPoint(child.position);
        var endPos = contentPos - childPos;
        // If no horizontal scroll, then don't change contentPos.x
        if (!scroll.horizontal) endPos.x = contentPos.x;
        // If no vertical scroll, then don't change contentPos.y
        if (!scroll.vertical) endPos.y = contentPos.y;
        scroll.content.anchoredPosition = endPos;
    }

    public override void ProcessInput(InputData input)
    {
        HandleDirectionalMovement(input);

        switch (input.KeyCode)
        {
            case KeyCode.Z:
                if (input.KeyState == KeyState.Down)
                {
                    PressOption(SelectedOption);
                    SelectedOption.SetPressed();
                }
                else
                {
                    SelectedOption.SetNormal();
                    SelectedOption.Execute();
                }

                break;

            case KeyCode.X:
                if (input.KeyState == KeyState.Down)
                    break;

                Deactivate();
                break;
        }
    }
    #endregion
}
