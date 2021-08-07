using DarkTonic.MasterAudio;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// This script should always be attached to the inventory of the loot pile
/// </summary>
public class LootingInventoryMenu : UnitInventoryMenu
{
    [SerializeField] private LootingInventoryMenu _otherMenu;
    [SerializeField] private bool _isLootPile; // Is this a lootable inventory?

/*    public void UpdateInventory(Unit target)
    {
        Debug.Assert(target != null);
        Show(target);
        UserInput.Instance.InputTarget = GridCursor.Instance;
    }*/

    public override void Show(Unit unit)
    {
        _selectedUnit = unit;

        if (_itemSlots == null)
        {
            _itemSlots = GetComponentsInChildren<InventoryItemSlot>().ToList();
            if (_itemSlots.Count > UnitInventory.MAX_SIZE)
                throw new IndexOutOfRangeException("Given more item slots than allowed per user.");
        }

        for (var i = 0; i < UnitInventory.MAX_SIZE; i++)
        {
            var itemSlot = _itemSlots[i];
            itemSlot.Clear();
        }

        var items = unit.Inventory.GetItems<Item>();
        for (var i = 0; i < items.Length; i++)
        {
            var itemSlot = _itemSlots[i];
            var inventoryItem = items[i];

            itemSlot.Populate(inventoryItem);
        }

        if (!gameObject.activeSelf)
        {
            MoveSelectionToOption(0, true);
            Activate();
            SelectOption(_itemSlots[0]);
        }

        if (_isLootPile)
        {
            ShowCursor();
            _header.PopulateSmall(_selectedUnit);
        }
        else
            _header.Populate(_selectedUnit);

        OnSelectionChange = delegate
        {
            if (!_itemDetailsView.IsActive()) return;

            if (SelectedItemSlot.IsEmpty)
                _itemDetailsView.Close();
            else
                _itemDetailsView.Show(SelectedItemSlot.Item, SelectedItemSlot.transform.localPosition);
        };
    }

    public override void ProcessInput(InputData input)
    {
        HandleDirectionalMovement(input);

        if (input.KeyState != KeyState.Up)
            return;

        switch (input.KeyCode)
        {
            case KeyCode.Z:
                // From menu base
                if (input.KeyState == KeyState.Down)
                {
                    MasterAudio.PlaySound3DFollowTransform(ConfirmSound, CampaignManager.AudioListenerTransform);
                    PressOption(SelectedOption);
                }
                else
                {
                    SelectedOption.SetNormal();
                    SelectedOption.Execute();
                }

                // this class
                // TODO: Change this feature to be looting menu left right to swap menus, and drop/pickup based on isSource
                if (_isLootPile)
                    SwitchToOtherMenu();
                // We can not trade nothing for nothing
                else if (_otherMenu.SelectedItemSlot.Item != null || SelectedItemSlot.Item != null)
                {
                    var firstUnit = _otherMenu._selectedUnit;
                    var secondUnit = _selectedUnit;
                    //firstUnit.Trade(secondUnit, _otherMenu.SelectedItemSlot.Item, SelectedItemSlot.Item);
                    secondUnit.Trade(firstUnit, SelectedItemSlot.Item, _otherMenu.SelectedItemSlot.Item);

                    Show(_selectedUnit);
                    _otherMenu.Show(_otherMenu._selectedUnit);


                    SwitchToOtherMenu();
                }

                MasterAudio.PlaySound3DFollowTransform(ConfirmSound, CampaignManager.AudioListenerTransform);
                break;

            case KeyCode.X:
                Debug.Log("Closing" + " " + _isLootPile);
                // Close the players inventory and this item list, going back to the last menu
                if (_isLootPile)
                {
                    Close();
                    _otherMenu.Close();
                    var actionMenu = (PreviousMenu as ActionSelectMenu);
                    actionMenu.ShowLootMenu();
                }
                else
                    SwitchToOtherMenu();

                MasterAudio.PlaySound3DFollowTransform(BackSound, CampaignManager.AudioListenerTransform);
                break;

            case KeyCode.Space:
                if (SelectedItemSlot.IsEmpty)
                    break;

                if (_itemDetailsView.IsActive())
                    _itemDetailsView.Close();
                else
                    _itemDetailsView.Show(SelectedItemSlot.Item, SelectedItemSlot.transform.localPosition);

                break;
        }
    }

    public override void OnClose()
    {
        base.OnClose();
        HideCursor();
    }

    public override void SelectItemSlot()
    {
        // Intentionally overriding this to prevent base functionality of selecting the item for use from UnitInventoryMenu.cs
    }

    public void ShowCursor() => _cursor.Show();

    public void HideCursor() => _cursor.Hide();

    private void SwitchToOtherMenu()
    {
        if (_itemDetailsView.IsActive())
            _itemDetailsView.Close();

        UserInput.Instance.InputTarget = _otherMenu;

        _otherMenu.ShowCursor();
        HideCursor();
    }
}
