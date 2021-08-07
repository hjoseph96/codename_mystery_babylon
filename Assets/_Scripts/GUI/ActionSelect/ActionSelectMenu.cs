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
    private LootingInventoryMenu _sourceLootingInventoryMenu, _destinationLootingInventoryMenu;
    [FoldoutGroup("References")] [SerializeField] 
    protected UICursor _cursor;
    [FoldoutGroup("References")] [SerializeField]
    private List<LootOptionChoice> _lootableOptions = new List<LootOptionChoice>();
    [FoldoutGroup("Options")] [SerializeField]
    protected List<ActionMenuOption> _optionsPrefabs = new List<ActionMenuOption>();
    [FoldoutGroup("Options")] [SerializeField]
    protected Transform _optionsParent;

    [FoldoutGroup("Button sprites")] [SerializeField]
    protected Sprite _normalSprite, _selectedSprite, _pressedSprite;

    protected Unit _selectedUnit;
    public Unit SelectedUnit { get => _selectedUnit; }
    private List<Unit> _lootableBodies = new List<Unit>();


    protected readonly List<ActionMenuOption> _options = new List<ActionMenuOption>();
    protected int _selectedOptionIndex;


    protected RectTransform _rectTransform;

    private SubMenuSelectType _subMenuSelectType = SubMenuSelectType.None;

    #region Monobehaviour
    private void Awake() => _rectTransform = GetComponentInChildren<ContentSizeFitter>().transform as RectTransform;
    #endregion

    #region Overrides
    public override MenuOption MoveSelection(Vector2Int input)
    {
        if (!_cursor.IsMoving)
            MoveSelection(-input.y);

        return _options[_selectedOptionIndex];
    }

    public override void OnClose()
    {
        switch (_subMenuSelectType)
        {
            case SubMenuSelectType.None:
                GridCursor.Instance.ResetUnit(_selectedUnit);
                UserInput.Instance.InputTarget = GridCursor.Instance;
                break;
            case SubMenuSelectType.Looting:
                Show(_selectedUnit);
                break;
            case SubMenuSelectType.Trading:
                Show(_selectedUnit);
                break;
        }
    }

    public override void ResetState()
    {
        _selectedOptionIndex = 0;
        _cursor.transform.SetParent(transform, false);
        ClearOptions();
    }
    #endregion

    #region Navigation Selection
    private void MoveSelection(int input)
    {
        if (input == 0)
            return;

        var newIndex = Mathf.Clamp(_selectedOptionIndex + input, 0, _options.Count - 1);

        if (_selectedOptionIndex != newIndex)
        {
            _selectedOptionIndex = newIndex; 

            MoveSelectionToOption(_selectedOptionIndex);
        }
    }

    public void Show(Unit unit)
    {
        Debug.Log("Showing Root Menu");
        _subMenuSelectType = SubMenuSelectType.None;
        _selectedUnit = unit;

        // Attack Option
        if (_selectedUnit.CanAttack())
            AddOption<AttackOption>();

        if (_selectedUnit.CanJump)
            AddOption<JumpOption>();

        // Items Option
        if(_selectedUnit.CanUseItems())
            AddOption<ItemsOption>();

        // Trade Option
        if (_selectedUnit.CanTrade())
            AddOption<TradeOption>();

        if (_selectedUnit.CanLoot())
            AddOption<LootOption>();

        // Wait Option
        AddOption<WaitOption>();

        MoveSelectionToOption(0, true);
        SelectOption(_options[0]);
        Activate();

        // This is used to rebuild VerticalLayoutGroup. Otherwise UI might not change size!
        LayoutRebuilder.ForceRebuildLayoutImmediate(_rectTransform);
    }

    /// <summary>
    /// Is called wen selecting Items from the main Action menu
    /// </summary>
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

    /// <summary>
    /// Is Called when selecting Trade from the main action menu
    /// </summary>
    public void ShowTradingMenu()
    {
        _subMenuSelectType = SubMenuSelectType.Trading;

        _sourceTradingInventoryMenu.PreviousMenu = this;
        _destinationTradingInventoryMenu.PreviousMenu = this;

        // Other unit needs to be collected from a menu before this one 
        var otherUnit = WorldGrid.Instance[_selectedUnit.AllTradableCells()[0]].Unit;
        _destinationTradingInventoryMenu.Show(otherUnit);

        var tradableCells = _selectedUnit.AllTradableCells();
        GridCursor.Instance.SetRestrictedToListMode(tradableCells, _destinationTradingInventoryMenu.UpdateInventory, BeginTrading, OnExitCallback);
        GridCursor.Instance.MoveInstant(tradableCells[0]);

        UserInput.Instance.InputTarget = GridCursor.Instance;
    }

    /// <summary>
    /// is called when loot is selected from the main action menu
    /// </summary>
    public void ShowLootMenu()
    {
        _subMenuSelectType = SubMenuSelectType.Looting;

        _destinationLootingInventoryMenu.PreviousMenu = this;
        _sourceLootingInventoryMenu.PreviousMenu = this;

        // Collect possible lootable units
        _lootableBodies = new List<Unit>();

        var lootablePoints = _selectedUnit.AllLootableCells();
        for(int i = 0; i < lootablePoints.Count; i++)
        {
            _lootableBodies.AddRange(WorldGrid.Instance[lootablePoints[i]].LootableBodies);
        }

        if (_lootableBodies.Count > 0) 
        {
            ResetState();
            for (int i = 0; i < _lootableBodies.Count; i++)
            {
                AddOption<LootOptionChoice>();

                if (_options.Count - 1 < i)
                    Debug.LogError("_options size is not matching bodies, determine cause of error");

                var option = _options[i] as LootOptionChoice;

                // Reset the unit names counter.
                if (i == 0)
                    option.ResetLists();
                // assign the unit to the button for executable
                option.SetUnit(_lootableBodies[i]);
            }
        }
        // TODO: Determine why this branch prevents backing out of the loot menu when above branch is checking for 1 instead of 0.
        else if(_lootableBodies.Count == 1)
        {
            BeginLooting(_lootableBodies[0]);
        }
    }
    #endregion

    #region Second Level Menus
    /// <summary>
    /// Is called when confirming the loot target, and starting the loot loop
    /// </summary>
    /// <param name="lootTarget"></param>
    public void BeginLooting(Unit lootTarget)
    {
        Debug.Assert(lootTarget != null);

        _sourceLootingInventoryMenu.Show(lootTarget);
        _destinationLootingInventoryMenu.Show(_selectedUnit);

        GridCursor.Instance.ExitRestrictedMode();
        UserInput.Instance.InputTarget = _sourceLootingInventoryMenu;
    }

    /// <summary>
    /// Is called when confirming the trade target, and starting the trade loop
    /// </summary>
    /// <param name="targetPosition"></param>
    public void BeginTrading(Vector2Int targetPosition)
    {
        var otherUnit = WorldGrid.Instance[targetPosition].Unit;
        Debug.Assert(otherUnit != null);
        Debug.Assert(otherUnit.IsAlly(_selectedUnit));

        _sourceTradingInventoryMenu.Show(_selectedUnit);
        GridCursor.Instance.ExitRestrictedMode();
        UserInput.Instance.InputTarget = _sourceTradingInventoryMenu;
    }
    #endregion

    #region Resets and Backwards Navigation
    /// <summary>
    /// Is called when backing out to the root menu option from trading
    /// </summary>
    public void OnExitCallback()
    {
        GridCursor.Instance.ExitRestrictedMode();
        _destinationTradingInventoryMenu.Close();
        if (_selectedUnit.DidTrade)
        {
            ResetState();
            Show(_selectedUnit);
        }
    }

    public void OnExitCallbackLoot()
    {
        GridCursor.Instance.ExitRestrictedMode();
        ClearLootableOptions();
        _sourceTradingInventoryMenu.Close();
    }

    protected virtual void ClearOptions()
    {
        foreach (var opt in _options)
            Destroy(opt.gameObject);

        _options.Clear();
    }

    private void ClearLootableOptions()
    {
        for(int i = 0; i < _lootableOptions.Count; i++)
            Destroy(_lootableOptions[i]);

        _lootableOptions.Clear();
    }
    #endregion

    public void FinishTurnForUnit() => _selectedUnit.TookAction();

    protected void AddOption<T>() where T : ActionMenuOption
    {
        var prefab = _optionsPrefabs.Find(opt => opt is T);
        var option = Instantiate(prefab, _optionsParent, false);
        option.Init(this, _normalSprite, _selectedSprite, _pressedSprite);

        _options.Add(option);
    }

    protected virtual void MoveSelectionToOption(int index, bool instant = false, bool playSound = true)
    {
        _cursor.transform.SetParent(_options[index].transform, false);
        _cursor.MoveTo(Vector2.zero, instant);

        if(playSound)
            MasterAudio.PlaySound3DFollowTransform(SelectedSound, CampaignManager.AudioListenerTransform);
    }

    private enum SubMenuSelectType { None, Trading, Looting}
}
