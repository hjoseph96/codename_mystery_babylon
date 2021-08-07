using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemActionsMenuOption : MenuOption<ItemActionsMenu>
{
    public virtual string Name { get; } = "Error";

    protected Unit Unit;
    protected Item Item;
    protected InventoryItemSlot ItemSlot;
    protected ItemActionsMenu ParentMenu;

    protected int SelectedSlotIndex => ParentMenu.AllItemSlots.IndexOf(ItemSlot); 

    private TextMeshProUGUI _textMeshPro;
    private Image _backgroundImg;

    private readonly Color32 _pressedColor = new Color32(210, 210, 180, 255);
    private readonly Color32 _selectedColor = new Color32(230, 230, 205, 255);
    private readonly Color32 _normalColor = new Color32(255, 255, 255, 255);

    private void Awake()
    {
        _backgroundImg = GetComponent<Image>(); 
        Menu = GetComponentInParent<ItemActionsMenu>();
        _textMeshPro = GetComponentInChildren<TextMeshProUGUI>();
        _textMeshPro.text = Name;
    }

    public void SetData(Unit unit, Item item, InventoryItemSlot itemSlot, ItemActionsMenu parentMenu) 
    {
        Unit = unit;
        Item = item;
        ItemSlot = itemSlot;
        ParentMenu = parentMenu;
    }

    public override void SetPressed()
    {
        _backgroundImg.color = _pressedColor;
    }

    public override void SetSelected()
    {
        _backgroundImg.color = _selectedColor;
    }

    public override void SetNormal()
    {
        _backgroundImg.color = _normalColor;
    }

    public int EquippedWeaponSlotIndex()
    {
        return Unit.Inventory.IndexOf(Unit.EquippedWeapon);
    }
}


public class EquipOption : ItemActionsMenuOption
{
    public override string Name { get; } = "Equip";

    public override void Execute()
    {
        // Remove Equipped Icon from currently Equipped ItemSlot
        if (Unit.HasWeapon)
            ParentMenu.AllItemSlots[EquippedWeaponSlotIndex()].HideEquippedIcon();

        Unit.EquipWeapon(Item as Weapon);
        ItemSlot.ShowEquippedIcon();

        Menu.Close();
    }
}

public class UnequipOption : ItemActionsMenuOption
{
    public override string Name { get; } = "Unequip";

    public override void Execute()
    {
        Unit.UnequipWeapon();
        ItemSlot.HideEquippedIcon();
        Menu.Close();
    }
}

public class UseOption : ItemActionsMenuOption
{
    public override string Name { get; } = "Use";

     public override void Execute()
     {
         Item.UseItem();
     }
}

public class DropOption : ItemActionsMenuOption
{
    public override string Name { get; } = "Drop";

    public override void Execute()
    {
        Unit.Inventory.RemoveItem(Item);
        
        var inventoryMenu = Menu.PreviousMenu as UnitInventoryMenu;
        inventoryMenu.RemoveSelected();
        //ItemSlot.HideEquippedIcon();

        Menu.Close();
    }
}

public class ItemActionsMenu : Menu
{
    public Action OnMenuClose;
    [SerializeField] private GameObject _optionPrefab;
    [SerializeField] private Transform _optionsParent;

    private readonly List<ItemActionsMenuOption> _options = new List<ItemActionsMenuOption>();

    private Unit _selectedUnit;
    private Item _selectedItem;
    private InventoryItemSlot _selectedItemSlot;
    public List<InventoryItemSlot> AllItemSlots { get; private set; }

    private int _selectedOptionIndex;

    // This is used to prevent instant scroll up/down when key is pressed
    private float _lastInputTime;
    private readonly float _inputCooldown = 0.15f;

    public void Show(Unit unit, Item item, InventoryItemSlot itemSlot, List<InventoryItemSlot> allItemSlots)
    {
        ResetState();
        _selectedUnit = unit;
        _selectedItem = item;
        _selectedItemSlot = itemSlot;
        AllItemSlots = allItemSlots;

        // TODO: Mounted units checks If we still want to be able to open the inventory, but just not use the items in the inventory after moving with a mounted unit, we need to expand this code here to allow that
        foreach (var option in item.GetUIOptions())
            AddOption(option);

        SetPositionUnderItemSlot(_selectedItemSlot.transform.localPosition);
        Activate();
        SelectOption(_options[0]);

        // This is used to rebuild VerticalLayoutGroup. Otherwise UI might not change size!
        // TODO: Cache rect transform instead of using GetComponent
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponentInChildren<ContentSizeFitter>().transform as RectTransform);
    }

    public override void ResetState()
    {
        _selectedOptionIndex = 0;
        ClearOptions();
    }

    public override MenuOption MoveSelection(Vector2Int input)
    {
        MoveSelection(-input.y);
        return _options[_selectedOptionIndex];
    }

    private void MoveSelection(int input)
    {
        if (input == 0 || Time.time - _lastInputTime < _inputCooldown)
            return;

        _lastInputTime = Time.time;
        _selectedOptionIndex = Mathf.Clamp(_selectedOptionIndex + input, 0, _options.Count - 1);
    }

    private void ClearOptions()
    {
        foreach (var opt in _options)
            Destroy(opt.gameObject);

        _options.Clear();
    }

    private void AddOption(Type type)
    {
        var go = Instantiate(_optionPrefab, _optionsParent, false);
        var option = go.AddComponent(type) as ItemActionsMenuOption;
        option.SetData(_selectedUnit, _selectedItem, _selectedItemSlot, this);

        _options.Add(option);
    }

    private void SetPositionUnderItemSlot(Vector2 slotLocalPosition) 
    {
        var tr = transform;
        var newPosition = new Vector2(tr.localPosition.x, slotLocalPosition.y - 250);
        tr.localPosition = newPosition;
    } 
}
