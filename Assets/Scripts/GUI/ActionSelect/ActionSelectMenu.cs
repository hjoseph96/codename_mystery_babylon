using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


[RequireComponent(typeof(TextMeshProUGUI))]
public class ActionMenuOption : MonoBehaviour
{
    public virtual string Name { get; } = "Error";

    protected ActionSelectMenu Menu;
    private TextMeshProUGUI _textMeshPro;

    private void Awake()
    {
        Menu = GetComponentInParent<ActionSelectMenu>();
        _textMeshPro = GetComponent<TextMeshProUGUI>();
        _textMeshPro.text = Name;
    }

    public virtual void Execute()
    { }
}

public class AttackOption : ActionMenuOption
{
    public override string Name { get; } = "Attack";

    public override void Execute()
    {
        Debug.Log("Attack");
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
        Menu.Close();
    }
}

public class ActionSelectMenu : MonoBehaviour, IInputTarget
{
    [SerializeField] private UnitInventoryMenu _inventoryMenu;
    [SerializeField] private UICursor _cursor;

    [SerializeField] private GameObject _optionPrefab;
    [SerializeField] private Transform _optionsParent;

    public Action OnMenuClose;
    public Unit SelectedUnit { get; private set; }


    private readonly List<ActionMenuOption> _options = new List<ActionMenuOption>();
    private int _selectedOptionIndex;

    private ActionMenuOption SelectedOption => _options[_selectedOptionIndex];

    private void Start()
    {
        gameObject.SetActive(false);
    }

    public void Show(Unit unit)
    {
        UserInput.Instance.InputTarget = this;
        SelectedUnit = unit;

        // Attack Option
        if (SelectedUnit.CanAttack())
            AddOption<AttackOption>();

        // Items Option
        AddOption<ItemsOption>();

        // Trade Option
        if (SelectedUnit.CanTrade())
            AddOption<TradeOption>();

        // Wait Option
        AddOption<WaitOption>();

        _selectedOptionIndex = 0;
        MoveSelectionToOption(0, true);

        gameObject.SetActive(true);
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

    public void Close()
    {
        gameObject.SetActive(false);
        _cursor.transform.parent = transform;

        UserInput.Instance.InputTarget = null;
        ClearOptions();

        OnMenuClose?.Invoke();
    }

    public void ShowInventory()
    {
        _inventoryMenu.Show(SelectedUnit);
        _inventoryMenu.OnMenuClose = () =>
        {
            UserInput.Instance.InputTarget = this;
        };

        //Close();
    }

    private void ClearOptions()
    {
        foreach (var opt in _options)
            Destroy(opt.gameObject);

        _options.Clear();
    }

    private void AddOption<T>()
        where T : ActionMenuOption
    {
        var go = Instantiate(_optionPrefab, _optionsParent, false);
        var option = go.AddComponent<T>();

        _options.Add(option);
    }

    private void MoveSelection(int input)
    {
        if (input == 0)
            return;

        _selectedOptionIndex = Mathf.Clamp(_selectedOptionIndex + input, 0, _options.Count - 1);
        MoveSelectionToOption(_selectedOptionIndex);
    }

    private void MoveSelectionToOption(int index, bool instant = false)
    {
        _cursor.transform.parent = _options[index].transform;
        _cursor.MoveTo(Vector2.zero, instant);
    }
}
