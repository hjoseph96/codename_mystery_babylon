using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DarkTonic.MasterAudio;


public class UnitInventoryMenu : Menu
{
    [Header("Unit Inventory Menu Items")]
    [SerializeField] protected UICursor _cursor;
    [SerializeField] private ItemActionsMenu _itemActionMenu;
    [SerializeField] protected ItemDetailsView _itemDetailsView;
    [SerializeField] protected HeaderDetailsView _header;
    [SerializeField] protected List<InventoryItemSlot> _itemSlots;

    protected Unit _selectedUnit;
    private int _selectedSlotIndex;
    public InventoryItemSlot SelectedItemSlot => _itemSlots[_selectedSlotIndex];


    public override MenuOption MoveSelection(Vector2Int input)
    {
        if (!_cursor.IsMoving)
        {
            var newIndex = Mathf.Clamp(_selectedSlotIndex - input.y, 0, _itemSlots.Count - 1); ;

            if (_selectedSlotIndex != newIndex)
                MasterAudio.PlaySound3DFollowTransform(SelectedSound, CampaignManager.AudioListenerTransform);

            _selectedSlotIndex = newIndex;
            MoveSelectionToOption(_selectedSlotIndex);
        }

        return _itemSlots[_selectedSlotIndex];
    }

    public virtual void Show(Unit unit)
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
        base.ProcessInput(input);

        if (input.KeyState != KeyState.Up)
            return;

        switch (input.KeyCode)
        {
            case KeyCode.Z:
                SelectedOption.Execute();
                break;

            case KeyCode.X:
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
        _cursor.transform.SetParent(transform, false);
        _selectedSlotIndex = 0;
    }

    public override void OnClose()
    {
        if (_itemDetailsView.IsActive())
            _itemDetailsView.Close();

        GridCursor.Instance.SetFreeMode();
    }

    public virtual void SelectItemSlot()
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



    protected void MoveSelectionToOption(int index, bool instant = false)
    {
        _cursor.transform.SetParent(_itemSlots[index].transform, false);

        var cursorPosition = new Vector3(0, 20f, 0);
        _cursor.MoveTo(cursorPosition, instant);
    }
}
