using System;
using UnityEngine;


public class UnitInventoryMenu : MonoBehaviour, IInputTarget
{
    [SerializeField] private UICursor _cursor;
    [SerializeField] private ItemActionsMenu _itemActionMenu;

    public Action OnMenuClose;
    private ItemSlot[] _itemSlots;

    private Unit _selectedUnit;
    private int _selectedSlotIndex;

    private ItemSlot SelectedSlot => _itemSlots[_selectedSlotIndex];

    private ItemSlot ActiveSlot; // The slot that is has SelectItemSlot called on it. 1 Selected slot at a time.

    private void Awake()
    {
        _itemSlots = GetComponentsInChildren<ItemSlot>();
    }

    private void Start()
    {
        if (_itemSlots.Length > UnitInventory.MaxSize)
            throw new IndexOutOfRangeException("Given more item slots than allowed per user.");

        gameObject.SetActive(false);
    }

    public void ProcessInput(InputData input)
    {
        if (!_cursor.IsMoving)
            MoveSelection(-input.Vertical);

        switch (input.KeyCode)
        {
            case KeyCode.Z:
                if (!SelectedSlot.IsEmpty)
                {
                    SelectItemSlot(SelectedSlot);

                    // TODO: Get Item Derived Class from Base?
                    var weapon = _selectedUnit.Inventory.GetItems<Weapon>()[_selectedSlotIndex];
                    _itemActionMenu.Show(_selectedUnit, weapon, _cursor);
                }
                // TODO: Further logic here
                // Active ItemOptionsMenu(SelectedSlot);
                // Upon open, this menu should read all required data from SelectedSlot.Item and populate UI objects with this data
                // Upon close, this menu should call SelectedSlot.SetDeselected(); to remove outline/highlighting
                // ItemOptionsMenu should implement IInputTarget and process all input inside ProcessInput(InputData);
                // Upon open, should execute UserInput.Instance.InputTarget = this
                // Upon close, should reset InputTarget back to UnitInventoryMenu object
                break;

            case KeyCode.X:
                if (ActiveSlot != null) {
                    ActiveSlot = null;

                } 
                Close();
                break;
        }
    }

    private void MoveSelection(int input)
    {
        _selectedSlotIndex = Mathf.Clamp(_selectedSlotIndex + input, 0, _itemSlots.Length - 1);
        MoveSelectionToOption(_selectedSlotIndex);
    }

    private void MoveSelectionToOption(int index, bool instant = false)
    {
        _cursor.transform.parent = _itemSlots[index].transform;
        _cursor.MoveTo(new Vector2(-300, -15), instant);
    }

    public void Show(Unit unit)
    {
        GridCursor.Instance.ClearAll();
        GridCursor.Instance.SetLockedMode();

        UserInput.Instance.InputTarget = this;

        _selectedUnit = unit;
        
        var items = unit.Inventory.GetItems<Item>();
        for (var i = 0; i < items.Length; i++)
        {
            var itemSlot = _itemSlots[i];
            var inventoryItem = items[i];

            itemSlot.Populate(inventoryItem);
        }

        _selectedSlotIndex = 0;
        MoveSelectionToOption(0, true);

        gameObject.SetActive(true);
    }

    private void Close() {
        gameObject.SetActive(false);
        _cursor.transform.parent = transform;

        GridCursor.Instance.SetFreeMode();
        OnMenuClose.Invoke();
    }

    private void SelectItemSlot(ItemSlot selectedSlot)
    {
        // TODO: Implement ItemOptionsMenu, append to selectedSlot and display relevant options
        selectedSlot.SetSelected();
        ActiveSlot = selectedSlot;
        // TODO: here we should set ItemOptionMenu as InputTarget for UserInput
    }
}
