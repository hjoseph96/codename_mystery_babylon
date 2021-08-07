using System;
using UnityEngine;
using Sirenix.OdinInspector;
using DarkTonic.MasterAudio;


public abstract class Menu : SerializedMonoBehaviour, IInputTarget
{
    public Menu PreviousMenu { get; set; }

    public MenuOption SelectedOption { get; private set; }

    [HideInInspector] public Action OnSelectionChange;
    
    [Header("Audio")]
    [SoundGroup] public string SelectedSound;
    [SoundGroup] public string ConfirmSound;
    [SoundGroup] public string BackSound;

    protected bool _isInitialized;
    protected bool _zeroed = true;
    protected bool _loopedInput = false;
    protected const float _defaultLoopSpeed = .2f;
    protected float _currentLoopSpeed = _defaultLoopSpeed;
    protected float _maxLoopSpeed = .01f;
    protected float _timeToIncreasePerLoop = .03f;
    protected float _currentTime = 0f;

    private void Start()
    {
        if (!_isInitialized)
            gameObject.SetActive(false);
    }

    // Reset menu state and hide, call OnClose, show PreviousMenu if it is set
    public virtual void Close()
    {
        ResetAndHide();
        OnClose();

        if (PreviousMenu != null)
            PreviousMenu.Activate();
    }

    // Reset menu state and hide it. Does NOT call OnClose
    public void ResetAndHide()
    {
        if ((SelectedOption as object) != null)
            SelectedOption.SetNormal();

        SelectedOption = null;
        ResetState();
        Deactivate();
    }

    public virtual void Activate()
    {
        _isInitialized = true;
        gameObject.SetActive(true);
        UserInput.Instance.InputTarget = this;
    }

    public virtual void Deactivate()
    {
        gameObject.SetActive(false);
        UserInput.Instance.InputTarget = null;
    }
    
    // Selects specific option
    public void SelectOption(MenuOption option)
    {
        // Skip doing anything if we are still on the same option.
        if (option == SelectedOption)
            return;
        if (SelectedOption != null)
            SelectedOption.SetNormal();

        SelectedOption = option;
        OnSelectionChange?.Invoke();

        if ((SelectedOption as object) != null)
            SelectedOption.SetSelected();
    }

    protected virtual void PressOption(MenuOption option)
    {
        if ((SelectedOption as object) != null)
            SelectedOption.SetPressed();
    }

    /// <summary>
    /// When overriding this input from Menu, ensure the _zeroed section is utilized
    /// <br>Either from calling base.ProcessInput() or taking the first if statements using _zeroed</br>
    /// </summary>
    /// <param name="input"></param>
    public virtual void ProcessInput(InputData input)
    {
        HandleDirectionalMovement(input);

        switch (input.KeyCode)
        {
            case KeyCode.RightArrow:
            case KeyCode.Z:
                if (input.KeyState == KeyState.Down)
                {
                    MasterAudio.PlaySound3DFollowTransform(ConfirmSound, CampaignManager.AudioListenerTransform);
                    PressOption(SelectedOption);
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

                MasterAudio.PlaySound3DFollowTransform(BackSound, CampaignManager.AudioListenerTransform);
                Close();
                break;
        }
    }

    protected virtual void HandleDirectionalMovement(InputData input)
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
            SelectOption(MoveSelection(input.MovementVector));
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

    // Processes user directional input (e.g. arrow keys)
    public virtual MenuOption MoveSelection(Vector2Int input)
    {
        throw new NotImplementedException();
    }

    // Resets menu state. You should reset all internal variables here, clear lists, etc
    public virtual void ResetState()
    { }

    // Called when menu is closed by X button or with Menu.Close
    // Override it if you need to perform some actions when menu is closed
    public virtual void OnClose()
    { }
}