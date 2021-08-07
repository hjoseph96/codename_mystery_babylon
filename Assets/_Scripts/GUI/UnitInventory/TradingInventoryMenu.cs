using DarkTonic.MasterAudio;
using UnityEngine;

public class TradingInventoryMenu : UnitInventoryMenu
{
    [SerializeField] private TradingInventoryMenu _otherMenu;
    [SerializeField] private bool _isSource; // Is this an inventory we move item FROM?

    public void UpdateInventory(Vector2Int targetPosition)
    {
        var otherUnit = WorldGrid.Instance[targetPosition].Unit;
        Debug.Assert(otherUnit != null);
        Debug.Assert(otherUnit.IsAlly(_selectedUnit));

        Show(otherUnit);
        UserInput.Instance.InputTarget = GridCursor.Instance;
    }

    public void UpdateInventory(Unit target)
    {
        Debug.Assert(target != null);
        Show(target);
        UserInput.Instance.InputTarget = GridCursor.Instance;
    }

    public override void ProcessInput(InputData input)
    {
        HandleDirectionalMovement(input);

        if (input.KeyState != KeyState.Up)
            return;

        switch (input.KeyCode)
        {
            case KeyCode.Z:
                if (_isSource)
                    SwitchToOtherMenu();
                // We can not trade nothing for nothing
                else if (_otherMenu.SelectedItemSlot.Item != null || SelectedItemSlot.Item != null)
                {
                    var firstUnit = _otherMenu._selectedUnit;
                    var secondUnit = _selectedUnit;
                    firstUnit.Trade(secondUnit, _otherMenu.SelectedItemSlot.Item, SelectedItemSlot.Item);

                    Show(_selectedUnit);
                    _otherMenu.Show(_otherMenu._selectedUnit);

     
                    SwitchToOtherMenu();

                    // TODO: bool _didTrade
                }

                MasterAudio.PlaySound3DFollowTransform(ConfirmSound, CampaignManager.AudioListenerTransform);
                break;

            case KeyCode.X:
                if (_isSource)
                {
                    Close();
                    _otherMenu.Close();
                    var actionMenu = (PreviousMenu as ActionSelectMenu);
                    actionMenu.ShowTradingMenu();
                }
                else
                    SwitchToOtherMenu();

                MasterAudio.PlaySound3DFollowTransform(BackSound, CampaignManager.AudioListenerTransform);
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

    public override void OnClose()
    {
        base.OnClose();

        if (!_isSource)
            HideCursor();
        else
            ShowCursor();
    }

    public void ShowCursor() => _cursor.Show();

    public void HideCursor() => _cursor.Hide();

    private void SwitchToOtherMenu()
    {
        if (_itemDetailsView.IsActive())
            _itemDetailsView.Close();

        UserInput.Instance.InputTarget = _otherMenu;

        _otherMenu.ShowCursor();
        HideCursor();
    }
}
