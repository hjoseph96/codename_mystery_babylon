using DarkTonic.MasterAudio;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SaveLoadActionMenu : ActionSelectMenu
{
    public new SaveLoadMenu PreviousMenu;
    public bool saving = true;

    public TextMeshProUGUI confirmText;

    public System.Action PerformedChange;
    public System.Action PerformRebuildCheck;

    public void OpenContextMenu()
    {
        AddContextOption(saving ? 0 : 1);
        AddContextOption(2);
        _selectedOptionIndex = 0;
        MoveSelectionToOption(_selectedOptionIndex, true, false);

        confirmText.text = saving ? "Are you sure you want to overwrite save file?" : "You will lose any unsaved progress by loading, continue?";
    }

    public void SelectOption(bool cancel)
    {
        if (!cancel)
        {
            if (saving)
            {
                PreviousMenu.OverwriteExistingSlot();
                PreviousMenu.TakeControl(true);
            }
            else
            {
                PreviousMenu.Load();
            }
        }

        Close();
    }

    protected override void MoveSelectionToOption(int index, bool instant = false, bool playSound = true)
    {
        _cursor.transform.SetParent(_options[index].transform, false);
        _cursor.MoveTo(Vector2.zero, instant);

        if (playSound)
            MasterAudio.PlaySound3DFollowTransform(SelectedSound, CampaignManager.AudioListenerTransform);
    }

    #region Inherited Methods
    public override MenuOption MoveSelection(Vector2Int input)
    {
        if (!_cursor.IsMoving)
            MoveSelection(-input.y);

        return _options[_selectedOptionIndex];
    }

    public override void Close()
    {
        ResetAndHide();
        UserInput.Instance.InputTarget = PreviousMenu;
        ResetState();
    }

    public override void ResetState()
    {
        _selectedOptionIndex = 0;
        _cursor.transform.SetParent(transform, false);
        ClearOptions();
    }

    protected override void ClearOptions()
    {
        foreach (var opt in _options)
            Destroy(opt.gameObject);

        _options.Clear();
    }


    public override void Activate()
    {
        OpenContextMenu();
        base.Activate();
    }

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

    public override void ProcessInput(InputData input)
    {
        HandleDirectionalMovement(input);

        switch (input.KeyCode)
        {
            case KeyCode.Z:
                if (input.KeyState == KeyState.Down)
                {
                    MasterAudio.PlaySound3DFollowTransform(ConfirmSound, CampaignManager.AudioListenerTransform);
                    PressOption(SelectedOption);
                    SelectedOption.SetPressed();
                }
                else
                {
                    SelectedOption.SetNormal();
                    SelectedOption.Execute();
                }

                break;

            case KeyCode.X:
                if (input.KeyState == KeyState.Down)
                    break;

                Close();
                MasterAudio.PlaySound3DFollowTransform(BackSound, CampaignManager.AudioListenerTransform);
                break;
        }
    }
    #endregion

    /// <summary>
    /// Adds the context option from the index set. 0 Save, 1 Load, 2 Cancel
    /// </summary>
    public void AddContextOption(int index)
    {
        var option = Instantiate(_optionsPrefabs[index], _optionsParent, false);
        option.Init(this, _normalSprite, _selectedSprite, _pressedSprite, "Move Item");
        _options.Add(option);

        switch (index)
        {
            case 0: 
                var saveOption = _options[0] as SaveActionOption;
                saveOption.Menu = this;
                break;
            case 1:
                var loadOption = _options[0] as LoadActionOption;
                loadOption.Menu = this;
                break;
            case 2:
                var cancelOption = _options[1] as CancelOption;
                cancelOption.Menu = this;
                break;
        }
    }
}
