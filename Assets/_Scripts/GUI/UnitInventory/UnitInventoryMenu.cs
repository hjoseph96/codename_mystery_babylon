using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DarkTonic.MasterAudio;


public class UnitInventoryMenu : Menu
{
    [SerializeField] protected UICursor _cursor;
    [SerializeField] private ItemActionsMenu _itemActionMenu;
    [SerializeField] protected ItemDetailsView _itemDetailsView;
    [SerializeField] private HeaderDetailsView _header;

    private List<ItemSlot> _itemSlots;

    protected Unit _selectedUnit;
    private int _selectedSlotIndex;
    public ItemSlot SelectedItemSlot => _itemSlots[_selectedSlotIndex];


    public override MenuOption MoveSelection(Vector2Int input)
    {
        if (!_cursor.IsMoving)
            MoveSelection(-input.y);

        return _itemSlots[_selectedSlotIndex];
    }

    public virtual void Show(Unit unit)
    {
        _selectedUnit = unit;

        if (_itemSlots == null)
        {
            _itemSlots = GetComponentsInChildren<ItemSlot>().ToList();
            if (_itemSlots.Count > UnitInventory.MaxSize)
                throw new IndexOutOfRangeException("Given more item slots than allowed per user.");
        }

        for (var i = 0; i < UnitInventory.MaxSize; i++)
        {
            var itemSlot = _itemSlots[i];
            itemSlot.Clear();
        }

        var items = unit.Inventory.GetItems<Item>();
        for (var i = 0; i < items.Length; i++)
        {
            var itemSlot = _itemSlots[i];
            var inventoryItem = items[i];

            // TODO: Use inventoryItem.Populate(itemSlot), make overloads for each item type to populate slots with correct data
            itemSlot.Populate(inventoryItem);
        }

        _header.Populate(_selectedUnit);

        if (!gameObject.activeSelf)
        {
            MoveSelectionToOption(0, true);
            Activate();
            SelectOption(_itemSlots[0]);
        }

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
        if (input.MovementVector != Vector2Int.zero)
            SelectOption(MoveSelection(input.MovementVector));

        if (input.KeyState != KeyState.Up)
            return;

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
                if (SelectedItemSlot.IsEmpty) 
                    break;

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
        if (_itemDetailsView.IsActive())
            _itemDetailsView.Close();

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
        var newIndex =  Mathf.Clamp(_selectedSlotIndex + input, 0, _itemSlots.Count - 1);;
        
        if (_selectedSlotIndex != newIndex)
            MasterAudio.PlaySound3DFollowTransform(SelectedSound, CampaignManager.AudioListenerTransform);
        
        _selectedSlotIndex = newIndex;
        MoveSelectionToOption(_selectedSlotIndex);    
    }

    private void MoveSelectionToOption(int index, bool instant = false)
    {
        _cursor.transform.parent = _itemSlots[index].transform;
        _cursor.MoveTo(Vector2.zero, instant);
    }
}
