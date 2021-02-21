using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(TextMeshProUGUI))]
public class ActionMenuOption : MenuOption<ActionSelectMenu>
{
    public virtual string Name { get; } = "Error";

    private TextMeshProUGUI _textMeshPro;

    private void Awake()
    {
        Menu = GetComponentInParent<ActionSelectMenu>();
        _textMeshPro = GetComponent<TextMeshProUGUI>();
        _textMeshPro.text = Name;
    }
}

public class AttackOption : ActionMenuOption
{
    public override string Name { get; } = "Attack";

    public override void Execute()
    {
        Menu.ShowAttackForecast();
    }
}

public class ItemsOption : ActionMenuOption
{
    public override string Name { get; } = "Items";

    public override void Execute()
    {
        Menu.ShowInventory();
    }
}

public class TradeOption : ActionMenuOption
{
    public override string Name { get; } = "Trade";

    public override void Execute()
    {
        Debug.Log("Trade");
    }
}

public class WaitOption : ActionMenuOption
{
    public override string Name { get; } = "Wait";

    public override void Execute()
    {
        Menu.ResetAndHide();

        GridCursor.Instance.SetFreeMode();
        UserInput.Instance.InputTarget = GridCursor.Instance;
    }
}



public class ActionSelectMenu : Menu
{
    [SerializeField] private UnitInventoryMenu _inventoryMenu;
    [SerializeField] private AttackForecastMenu _attackForecastMenu;
    [SerializeField] private UICursor _cursor;

    [SerializeField] private GameObject _optionPrefab;
    [SerializeField] private Transform _optionsParent;

    private Unit _selectedUnit;

    private readonly List<MenuOption> _options = new List<MenuOption>();
    private int _selectedOptionIndex;

    public void Show(Unit unit)
    {
        _selectedUnit = unit;

        // Attack Option
        if (_selectedUnit.CanAttack())
            AddOption<AttackOption>();

        // Items Option
        AddOption<ItemsOption>();

        // Trade Option
        if (_selectedUnit.CanTrade())
            AddOption<TradeOption>();

        // Wait Option
        AddOption<WaitOption>();

        MoveSelectionToOption(0, true);
        SelectOption(_options[0]);
        Activate();

        // This is used to rebuild VerticalLayoutGroup. Otherwise UI might not change size!
        // TODO: Cache rect transform instead of using GetComponent
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponentInChildren<ContentSizeFitter>().transform as RectTransform);
    }

    public override MenuOption MoveSelection(Vector2Int input)
    {
        if (!_cursor.IsMoving)
            MoveSelection(-input.y);

        return _options[_selectedOptionIndex];
    }

    private void MoveSelection(int input)
    {
        if (input == 0)
            return;

        _selectedOptionIndex = Mathf.Clamp(_selectedOptionIndex + input, 0, _options.Count - 1);
        MoveSelectionToOption(_selectedOptionIndex);
    }

    public override void ResetState()
    {
        _selectedOptionIndex = 0;
        _cursor.transform.parent = transform;
        ClearOptions();
    }

    public override void OnClose()
    {
        GridCursor.Instance.SetRestrictedMode(_selectedUnit);
        UserInput.Instance.InputTarget = GridCursor.Instance;
    }

    public void ShowInventory()
    {
        _inventoryMenu.Show(_selectedUnit);
        _inventoryMenu.PreviousMenu = this;
    }

    public void ShowAttackForecast() {
        _attackForecastMenu.Show(_selectedUnit);
        _attackForecastMenu.PreviousMenu = this;
        ResetAndHide();
    }

    private void ClearOptions()
    {
        foreach (var opt in _options)
            Destroy(opt.gameObject);

        _options.Clear();
    }

    private void AddOption<T>()
        where T : MenuOption
    {
        var go = Instantiate(_optionPrefab, _optionsParent, false);
        var option = go.AddComponent<T>();

        _options.Add(option);
    }

    private void MoveSelectionToOption(int index, bool instant = false)
    {
        _cursor.transform.parent = _options[index].transform;
        _cursor.MoveTo(Vector2.zero, instant);
    }
}
