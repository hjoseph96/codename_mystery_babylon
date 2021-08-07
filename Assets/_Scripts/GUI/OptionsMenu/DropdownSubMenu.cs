using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DropdownSubMenu : Menu
{
    public TMP_Dropdown dropdown;
    public OptionsDropDown optionsDropdown;
    public UICursor cursor;
    private int _selectedOptionIndex;
    public Transform dropdownContent;


    public override void ProcessInput(InputData input)
    {
        HandleDirectionalMovement(input);

        switch (input.KeyCode)
        {
            case KeyCode.Z:
                if (input.KeyState == KeyState.Up)
                {
                    //MasterAudio.PlaySound3DFollowTransform(ConfirmSound, CampaignManager.AudioListenerTransform);
                    dropdown.onValueChanged?.Invoke(_selectedOptionIndex);
                    dropdown.Hide();
                    dropdown.value = _selectedOptionIndex;
                    //dropdown.RefreshShownValue();
                    UserInput.Instance.InputTarget = PreviousMenu;
                }
                break;

            case KeyCode.X:
                if (input.KeyState == KeyState.Up)
                {
                    dropdown.Hide();
                    UserInput.Instance.InputTarget = PreviousMenu;
                }
                break;
        }
    }

    protected override void HandleDirectionalMovement(InputData input)
    {
        if (input.MovementVector != Vector2Int.zero)
        {
            _currentTime += Time.deltaTime;
            if (_currentTime >= _currentLoopSpeed)
            {
                _currentTime -= _currentLoopSpeed;
                _loopedInput = true;

                if (_currentLoopSpeed > _maxLoopSpeed)
                {
                    _currentLoopSpeed -= _timeToIncreasePerLoop;
                }
            }
        }
        if (_zeroed || _loopedInput)
        {
            MoveSelection(-input.MovementVector.y);
            _zeroed = false;
            _loopedInput = false;
        }

        if (!_zeroed && input.MovementVector == Vector2Int.zero)
        {
            _zeroed = true;
            _currentLoopSpeed = _defaultLoopSpeed;
            _currentTime = 0f;
        }
    }

    private void MoveSelection(int input)
    {
        if (input == 0)
            return;

        var newIndex = Mathf.Clamp(_selectedOptionIndex + input, 0, dropdown.options.Count - 1);

        if (_selectedOptionIndex != newIndex)
        {
            _selectedOptionIndex = newIndex;

            MoveSelectionToOption(_selectedOptionIndex);
        }
    }

    private void MoveSelectionToOption(int index, bool instant = false)
    {
        Debug.Log("Selected index " + index);
        //cursor.transform.SetParent(dropdownContent.GetChild(index), false);
        cursor.MoveTo(new Vector2(0, 10 + 10*index), instant);

        //MasterAudio.PlaySound3DFollowTransform(SelectedSound, CampaignManager.AudioListenerTransform);
    }

    public override void Activate()
    {

    }

    public override void Deactivate()
    {

    }
}
