using DarkTonic.MasterAudio;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ConvoyActionMenu : ActionSelectMenu
{
    public new ConvoyMenu PreviousMenu;
    public new List<ActionMenuOption> _options = new List<ActionMenuOption>();
    private bool _displayingUnits = false;
    private ConvoyItemSlot _selectedItem = null;

    public System.Action PerformedChange;
    public System.Action<Item, bool, bool> PerformRebuildCheck;

    /// <summary>
    /// Performs the move to the unit selected from the option menu index.
    /// </summary>
    public void PerformItemMove(Unit u)
    {
        // if unit to send to is not null, send item to units inventory
        if(u != null)
        {
            string from = _selectedUnit != null ? _selectedUnit.Name : "Convoy";
            Debug.Log($"Moved item from {from} to {u.Name}");
            // Add item to unit

            // Remove item from original unit and give to destination unit
            if (_selectedUnit != null) 
            {
                _selectedUnit.Inventory.RemoveItem(_selectedItem.Item);
                u.Inventory.AddItem(_selectedItem.Item);
            }
            // Remove item from Convoy, and give to destination unit
            else
            {
                // If Item is consumable or weapon, remove it from the amount on the item, and trigger a new ConvoyItemSlot to be created for the destination unit's inventory
                PreviousMenu.convoy.RemoveItem(_selectedItem.Item);
                if (_selectedItem.Item.ItemType == ItemType.Consumable)
                {
                    if (_selectedItem.Item is Consumable c)
                    {
                        var item = c.CopyData();
                        u.Inventory.AddItem(item);
                        PerformRebuildCheck?.Invoke(item, true, false);
                    }
                }
                else if (_selectedItem.Item.ItemType == ItemType.Weapon)
                {
                    if (_selectedItem.Item is Weapon w)
                    {
                        var item = w.CopyData();
                        u.Inventory.AddItem(item);
                        PerformRebuildCheck?.Invoke(item, true, false);
                    }
                }
                else
                {
                    u.Inventory.AddItem(_selectedItem.Item);
                    PerformRebuildCheck?.Invoke(_selectedItem.Item, true, false);
                }
            }
        }
        // else we're sending it to the convoy
        else
        {
            Debug.Log($"Moved item from {_selectedUnit.Name} to Convoy");
            // Add item to Convoy
            PreviousMenu.convoy.AddItem(_selectedItem.Item);
            // Remove item from unit
            _selectedUnit.Inventory.RemoveItem(_selectedItem.Item);
        }

        _selectedItem.changedOwner = true;
        PerformedChange?.Invoke();
        Close();
    }

    /// <summary>
    /// Opens the units sub menu to allow trading for the selected item
    /// </summary>
    public void OpenUnitMenu()
    {
        ResetState();
        var units = EntityManager.Instance.PlayerUnits();

        // collect all units and add them if they have space for an item
        for (int i = 0; i < units.Count; i++)
        {
            if (!units[i].Inventory.IsFull && units[i] != _selectedUnit)
                AddUnit(units[i]);
        }

        // Add the convoy option if we're not trying to move from the convoy already
        if(_selectedUnit != null)
            AddUnit(null);

        if (_options.Count == 0)
        {
            Debug.LogWarning("No destination available for move item TODO: Play tooltip saying no room, and sound");
            Close();
            return;
        }
        else
        {
            _selectedOptionIndex = 0;
            MoveSelectionToOption(_selectedOptionIndex, true, false);
        }
        _displayingUnits = true;
    }

    public void SetSelectedItem(ConvoyItemSlot item) 
    { 
        _selectedItem = item;
        _selectedUnit = item.Item.Unit;
    }

    public void OpenContextMenu()
    { 
        AddContextOption();
        _selectedOptionIndex = 0;
        MoveSelectionToOption(_selectedOptionIndex, true, false);
    }

    protected override void MoveSelectionToOption(int index, bool instant = false, bool playSound = true)
    {
        _cursor.transform.SetParent(_options[index].transform, false);
        _cursor.MoveTo(Vector2.zero, instant);

        if (playSound)
            MasterAudio.PlaySound3DFollowTransform(SelectedSound, CampaignManager.AudioListenerTransform);
    }

    #region Inherited Methods
    public override MenuOption MoveSelection(Vector2Int input)
    {
        if (!_cursor.IsMoving)
            MoveSelection(-input.y);

        return _options[_selectedOptionIndex];
    }

    public override void Close()
    {
        ResetAndHide();
        OnClose();

        if (PreviousMenu != null)
            PreviousMenu.TakeControl();
    }

    public override void OnClose()
    {
        UserInput.Instance.InputTarget = PreviousMenu;
        _selectedItem = null;
        _selectedUnit = null;
        _displayingUnits = false;
        PreviousMenu.TakeControl();
        ResetState();
    }

    public override void ResetState()
    {
        _selectedOptionIndex = 0;
        _cursor.transform.SetParent(transform, false);
        ClearOptions();
    }

    protected override void ClearOptions()
    {
        foreach (var opt in _options)
            Destroy(opt.gameObject);

        _options.Clear();
    }


    public override void Activate()
    {
        OpenContextMenu();
        base.Activate();
    }

    private void MoveSelection(int input)
    {
        if (input == 0)
            return;

        var newIndex = Mathf.Clamp(_selectedOptionIndex + input, 0, _options.Count - 1);

        if (_selectedOptionIndex != newIndex)
        {
            _selectedOptionIndex = newIndex;

            MoveSelectionToOption(_selectedOptionIndex);
        }
    }

    public override void ProcessInput(InputData input)
    {
        HandleDirectionalMovement(input);

        switch (input.KeyCode)
        {
            case KeyCode.Z:
                if (input.KeyState == KeyState.Down)
                {
                    MasterAudio.PlaySound3DFollowTransform(ConfirmSound, CampaignManager.AudioListenerTransform);
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

                if (_displayingUnits)
                {
                    _displayingUnits = false;
                    ResetState();
                    OpenContextMenu();
                }
                else
                {
                    Close();
                }
                MasterAudio.PlaySound3DFollowTransform(BackSound, CampaignManager.AudioListenerTransform);
                break;
        }
    }
    #endregion

    /// <summary>
    /// Add a destination for moving an item, null to assign a convoy destination
    /// </summary>
    /// <param name="u"></param>
    public void AddUnit(Unit u)
    {
        var prefab = _optionsPrefabs.Find(opt => opt is ConvoyUnitOption);
        var option = Instantiate(prefab, _optionsParent, false).GetComponent<ConvoyUnitOption>();
        option.Init(this, _normalSprite, _selectedSprite, _pressedSprite, u, u != null? u.Name : "Convoy");
        _options.Add(option); 
        var Option = _options.Last() as ConvoyUnitOption;
        Option.Menu = this;
    }

    /// <summary>
    /// Adds a context option when selecting an item, TODO: expand this to account for other features.
    /// </summary>
    public void AddContextOption()
    {
        var prefab = _optionsPrefabs.Find(opt => opt is ConvoyActionOption);
        var option = Instantiate(prefab, _optionsParent, false);
        option.Init(this, _normalSprite, _selectedSprite, _pressedSprite, "Move Item");
        _options.Add(option); 
        var Option = _options[_options.Count - 1] as ConvoyActionOption;
        Option.Menu = this;
    }
}
