using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DarkTonic.MasterAudio;
using Sirenix.OdinInspector;


public class ActionSelectMenu : Menu
{
    [FoldoutGroup("References")] [SerializeField] 
    private UnitInventoryMenu _inventoryMenu;
    [FoldoutGroup("References")] [SerializeField]
    private AttackForecastMenu _attackForecastMenu;
    [FoldoutGroup("References")] [SerializeField]
    private TradingInventoryMenu _sourceTradingInventoryMenu, _destinationTradingInventoryMenu;
    [FoldoutGroup("References")] [SerializeField] 
    private UICursor _cursor;

    [FoldoutGroup("Options")] [SerializeField]
    private List<ActionMenuOption> _optionsPrefabs = new List<ActionMenuOption>();

    [FoldoutGroup("Options")] [SerializeField] 
    private Transform _optionsParent;

    [FoldoutGroup("Button sprites")] [SerializeField] 
    private Sprite _normalSprite, _selectedSprite, _pressedSprite;

    private Unit _selectedUnit;

    private readonly List<ActionMenuOption> _options = new List<ActionMenuOption>();
    private int _selectedOptionIndex;


    private RectTransform _rectTransform;

    private void Awake() => _rectTransform = GetComponentInChildren<ContentSizeFitter>().transform as RectTransform;

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
        LayoutRebuilder.ForceRebuildLayoutImmediate(_rectTransform);
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
        GridCursor.Instance.ResetUnit(_selectedUnit);
        UserInput.Instance.InputTarget = GridCursor.Instance;
    }

    public void ShowInventory()
    {
        GridCursor.Instance.ClearAll();
        _inventoryMenu.Show(_selectedUnit);
        _inventoryMenu.PreviousMenu = this;
    }

    public void ShowAttackForecast()
    {
        _attackForecastMenu.Show(_selectedUnit);
        _attackForecastMenu.PreviousMenu = this;
    }

    public void ShowTradingMenu()
    {
        _sourceTradingInventoryMenu.PreviousMenu = this;
        _destinationTradingInventoryMenu.PreviousMenu = this;

        var otherUnit = WorldGrid.Instance[_selectedUnit.AllTradableCells()[0]].Unit;
        _destinationTradingInventoryMenu.Show(otherUnit);

        var tradableCells = _selectedUnit.AllTradableCells();
        GridCursor.Instance.SetRestrictedToListMode(tradableCells, _destinationTradingInventoryMenu.UpdateInventory, BeginTrading, OnExitCallback);
        GridCursor.Instance.MoveInstant(tradableCells[0]);

        UserInput.Instance.InputTarget = GridCursor.Instance;
    }

    public void BeginTrading(Vector2Int targetPosition)
    {
        var otherUnit = WorldGrid.Instance[targetPosition].Unit;
        Debug.Assert(otherUnit != null);
        Debug.Assert(otherUnit.IsAlly(_selectedUnit));

        _sourceTradingInventoryMenu.Show(_selectedUnit);
        GridCursor.Instance.ExitRestrictedMode();
        UserInput.Instance.InputTarget = _sourceTradingInventoryMenu;
    }

    public void OnExitCallback()
    {
        GridCursor.Instance.ExitRestrictedMode();
        _destinationTradingInventoryMenu.Close();
    }

    public void FinishTurnForUnit() => _selectedUnit.TookAction();

    private void ClearOptions()
    {
        foreach (var opt in _options)
            Destroy(opt.gameObject);

        _options.Clear();
    }

    private void AddOption<T>()
        where T : ActionMenuOption
    {
        var prefab = _optionsPrefabs.Find(opt => opt is T);
        var option = Instantiate(prefab, _optionsParent, false);
        option.Init(this, _normalSprite, _selectedSprite, _pressedSprite);

        _options.Add(option);
    }

    private void MoveSelectionToOption(int index, bool instant = false)
    {
        _cursor.transform.parent = _options[index].transform;
        _cursor.MoveTo(Vector2.zero, instant);
        MasterAudio.PlaySound3DFollowTransform(SelectedSound, CampaignManager.AudioListenerTransform);
    }
}
