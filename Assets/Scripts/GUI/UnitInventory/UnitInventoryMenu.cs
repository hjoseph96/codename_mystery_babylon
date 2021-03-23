using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using DarkTonic.MasterAudio;

public class UnitInventoryMenu : Menu
{
    [SerializeField] private UICursor _cursor;
    [SerializeField] private ItemActionsMenu _itemActionMenu;
    [SerializeField] private ItemDetailsView _itemDetailsView;
    [SerializeField] private HeaderDetailsView _header;

    private List<ItemSlot> _itemSlots;

    private Unit _selectedUnit;
    private int _selectedSlotIndex;
    private ItemSlot SelectedItemSlot => _itemSlots[_selectedSlotIndex];


    private void Awake()
    {
        _itemSlots = GetComponentsInChildren<ItemSlot>().ToList();

        if (_itemSlots.Count > UnitInventory.MaxSize)
            throw new IndexOutOfRangeException("Given more item slots than allowed per user.");
    }

    public override MenuOption MoveSelection(Vector2Int input)
    {
        if (!_cursor.IsMoving)
            MoveSelection(-input.y);

        return _itemSlots[_selectedSlotIndex];
    }

    public void Show(Unit unit)
    {
        GridCursor.Instance.ClearAll();

        _selectedUnit = unit;
        
        var items = unit.Inventory.GetItems<Item>();
        for (var i = 0; i < items.Length; i++)
        {
            var itemSlot = _itemSlots[i];
            var inventoryItem = items[i];

            // TODO: Use inventoryItem.Populate(itemSlot), make overloads for each item type to populate slots with correct data
            itemSlot.Populate(inventoryItem);
        }

        for (var i = items.Length; i < UnitInventory.MaxSize; i++)
        {
            var itemSlot = _itemSlots[i];
            itemSlot.Clear();
        }

        _header.Populate(_selectedUnit);
        MoveSelectionToOption(0, true);
        Activate();
        SelectOption(_itemSlots[0]);

        OnSelectionChange += delegate () {
            if (_itemDetailsView.IsActive())
                if (SelectedItemSlot.IsEmpty)
                    _itemDetailsView.Close();
                else
                    _itemDetailsView.Show(SelectedItemSlot.Item, SelectedItemSlot.transform.localPosition);
        };
    }

    public override void ProcessInput(InputData input)
    {
        if (input.MovementVector != Vector2Int.zero)
            SelectOption(MoveSelection(input.MovementVector));

        switch (input.KeyCode)
        {
            case KeyCode.Z:
                SelectedOption.Execute();
                MasterAudio.PlaySound3DFollowTransform(ConfirmSound, CampaignManager.AudioListenerTransform);
                break;

            case KeyCode.X:
                MasterAudio.PlaySound3DFollowTransform(BackSound, CampaignManager.AudioListenerTransform);
                Close();
                break;
            
            case KeyCode.Space:
                if (_itemDetailsView.IsActive())
                    _itemDetailsView.Close();
                else
                    _itemDetailsView.Show(SelectedItemSlot.Item, SelectedItemSlot.transform.localPosition);
                
                break;
        }
    }


    public override void ResetState()
    {
        _cursor.transform.parent = transform;
        _selectedSlotIndex = 0;
    }

    public override void OnClose()
    {
        GridCursor.Instance.SetFreeMode();
    }

    public void SelectItemSlot()
    {
        var selectedSlot = _itemSlots[_selectedSlotIndex];
        _itemActionMenu.Show(_selectedUnit, _selectedUnit.Inventory[_selectedSlotIndex], selectedSlot, _itemSlots);
        _itemActionMenu.PreviousMenu = this;
    }

    public void RemoveSelected()
    {
        for (var i = _selectedSlotIndex + 1; i < _itemSlots.Count; i++)
        {
            if (_itemSlots[i].IsEmpty)
            {
                _itemSlots[i - 1].Clear();
                break;
            }

            _itemSlots[i - 1].Clear();
            _itemSlots[i - 1].Populate(_itemSlots[i].Item);
        }
    }

    private void MoveSelection(int input)
    {
        int newIndex =  Mathf.Clamp(_selectedSlotIndex + input, 0, _itemSlots.Count - 1);;
        
        if (_selectedSlotIndex != newIndex)
            MasterAudio.PlaySound3DFollowTransform(SelectedSound, CampaignManager.AudioListenerTransform);
        
        _selectedSlotIndex = newIndex;
        MoveSelectionToOption(_selectedSlotIndex);    
    }

    private void MoveSelectionToOption(int index, bool instant = false)
    {
        _cursor.transform.parent = _itemSlots[index].transform;
        _cursor.MoveTo(new Vector2(-215, -13), instant);
    }
}
