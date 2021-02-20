using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemActionsMenuOption : MonoBehaviour
{
    public virtual string Name { get; } = "Error";


    protected ItemActionsMenu Menu;
    protected Unit playerUnit;
    protected Weapon weapon;
    // protected Consumable _consumable;
    // protected TextGenerationSettings _gear;
    // protected Valuable _valuable;

    private TextMeshProUGUI _textMeshPro;
    private Image _backgroundImg;
    private Color32 _selectedColor = new Color32(241, 238, 238, 255);
    private Color32 _normalColor = new Color32(255, 255, 255, 255);

    private void Awake()
    {
        _backgroundImg = GetComponent<Image>(); 
        Menu = GetComponentInParent<ItemActionsMenu>();
        _textMeshPro = GetComponentInChildren<TextMeshProUGUI>();
        _textMeshPro.text = Name;
    }

    

    public virtual void Execute() {}
    
    public void SetItem(Weapon givenWeapon) {
        weapon = givenWeapon;
    }

    public void SetSelected()
    {
        _backgroundImg.color = _selectedColor;
    }

    public void ResetColor()
    {
        _backgroundImg.color = _normalColor;
    }
}


public class EquipOption : ItemActionsMenuOption
{
    public override string Name { get; } = "Equip";

    public override void Execute()
    {
        playerUnit.EquipWeapon(weapon);
    }

    
    // public override void Execute()
    // {
    //     playerUnit.EquipGear(gear);
    // }
}

public class UnequipOption : ItemActionsMenuOption
{
    public override string Name { get; } = "Unequip";

    public override void Execute()
    {
        playerUnit.UnequipWeapon();
    }

    
    // public override void Execute()
    // {
    //     playerUnit.EquipGear(gear);
    // }
}

public class UseOption : ItemActionsMenuOption
{
    public override string Name { get; } = "Use";

    // public override void Execute()
    // {
    //     // consumable.Use(playerUnit) TODO
    // }
}

public class DropOption : ItemActionsMenuOption
{
    public override string Name { get; } = "Drop";

    public override void Execute()
    {
        playerUnit.Inventory.RemoveItem(weapon);
    }
}

public class ItemActionsMenu : MonoBehaviour, IInputTarget
{
    public Action OnMenuClose;
    [SerializeField] private GameObject _optionPrefab;
    [SerializeField] private Transform _optionsParent;
    private readonly List<ItemActionsMenuOption> _options = new List<ItemActionsMenuOption>();
    public Unit SelectedUnit { get; private set; }

    private UICursor _cursor;
    private int _selectedOptionIndex;
    private ItemActionsMenuOption SelectedOption => _options[_selectedOptionIndex];
    private Weapon _weapon;
    // private Consumable _consumable;
    // private TextGenerationSettings _gear;
    // private Valuable _valuable;

    
    // TODO: Show() for every item type: Weapon, Gear, Consumable, Valuable
    public void Show(Unit unit, Weapon weapon, UICursor cursor) {
        UserInput.Instance.InputTarget = this;
        SelectedUnit = unit;
        _weapon = weapon;
        _cursor = cursor;

        // Attack Option
        if (unit.EquippedWeapon == _weapon) {
            AddOption<EquipOption>();
        } else {
            AddOption<UnequipOption>();
        }

        // TODO: If Weapon has Use Ability
        // if (weapon.HasUseAbility())
        //     AddOption<UseOption>();

        // Items Option
        AddOption<DropOption>();

        _selectedOptionIndex = 0;
        MoveSelectionToOption();


        gameObject.SetActive(true);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ProcessInput(InputData input)
    {
        if (!_cursor.IsMoving)
            MoveSelection(-input.Vertical);

        switch (input.KeyCode)
        {
            case KeyCode.Z:
                SelectedOption.Execute();
                break;

            case KeyCode.X:
                Close();
                break;
        }
    }
    
    private void MoveSelection(int input)
    {
        if (input == 0)
            return;

        SelectedOption.ResetColor();
        _selectedOptionIndex = Mathf.Clamp(_selectedOptionIndex + input, 0, _options.Count - 1);
        MoveSelectionToOption();
    }

    private void MoveSelectionToOption(bool instant = false)
    {
        SelectedOption.SetSelected();
        _cursor.transform.parent = SelectedOption.transform;
        _cursor.MoveTo(Vector2.zero, instant);
    }

    private void AddOption<T>()
        where T : ItemActionsMenuOption
    {
        var go = Instantiate(_optionPrefab, _optionsParent, false);
        var option = go.AddComponent<T>();
        option.SetItem(_weapon);

        _options.Add(option);
    }

    private void Close()
    {

    }
}
