using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemActionsMenuOption : MenuOption<ItemActionsMenu>
{
    public virtual string Name { get; } = "Error";

    protected Unit Unit;
    protected Item Item;
    protected ItemSlot ItemSlot;
    protected ItemActionsMenu ParentMenu;

    protected int SelectedSlotIndex => ParentMenu.AllItemSlots.IndexOf(ItemSlot); 

    private TextMeshProUGUI _textMeshPro;
    private Image _backgroundImg;

    private readonly Color32 _selectedColor = new Color32(241, 238, 238, 255);
    private readonly Color32 _normalColor = new Color32(255, 255, 255, 255);

    private void Awake()
    {
        _backgroundImg = GetComponent<Image>(); 
        Menu = GetComponentInParent<ItemActionsMenu>();
        _textMeshPro = GetComponentInChildren<TextMeshProUGUI>();
        _textMeshPro.text = Name;
    }

    public void SetData(Unit unit, Item item, ItemSlot itemSlot, ItemActionsMenu parentMenu) 
    {
        Unit = unit;
        Item = item;
        ItemSlot = itemSlot;
        ParentMenu = parentMenu;
    }

    public override void SetSelected()
    {
        _backgroundImg.color = _selectedColor;
    }

    public override void SetDeselected()
    {
        _backgroundImg.color = _normalColor;
    }

    public int EquippedWeaponSlotIndex() {
        var playerItems = new List<Item>();
        foreach(var item in Unit.Inventory.GetItems<Item>())
            playerItems.Add(item);

        return playerItems.IndexOf(Unit.EquippedWeapon);
    }
}


public class EquipOption : ItemActionsMenuOption
{
    public override string Name { get; } = "Equip";

    public override void Execute()
    {
        // Remove Equipped Icon from currently Equipped ItemSlot
        var currentlyEquippedSlot = ParentMenu.AllItemSlots[EquippedWeaponSlotIndex()];
        currentlyEquippedSlot.HideEquippedIcon();
        
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
        ItemSlot.HideEquippedIcon();

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
    private ItemSlot _selectedItemSlot;
    public List<ItemSlot> AllItemSlots { get; private set; }

    private int _selectedOptionIndex;

    // This is used to prevent instant scroll up/down when key is pressed
    private float _lastInputTime;
    private readonly float _inputCooldown = 0.15f;

    public void Show(Unit unit, Item item, ItemSlot itemSlot, List<ItemSlot> allItemSlots)
    {
        _selectedUnit = unit;
        _selectedItem = item;
        _selectedItemSlot = itemSlot;
        AllItemSlots = allItemSlots;

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

    private void SetPositionUnderItemSlot(Vector2 slotLocalPosition) {
        var newPosition = new Vector2(
            this.transform.localPosition.x, slotLocalPosition.y - 145
        );
        this.transform.localPosition = newPosition;
    } 
}
