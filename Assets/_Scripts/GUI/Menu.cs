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

    private bool _isInitialized;

    private void Start()
    {
        if (!_isInitialized)
            gameObject.SetActive(false);
    }

    // Reset menu state and hide, call OnClose, show PreviousMenu if it is set
    public void Close()
    {
        ResetAndHide();
        OnClose();

        if ((PreviousMenu as object) != null)
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

    public void Activate()
    {
        _isInitialized = true;
        gameObject.SetActive(true);
        UserInput.Instance.InputTarget = this;
    }

    public void Deactivate()
    {
        gameObject.SetActive(false);
        UserInput.Instance.InputTarget = null;
    }
    
    // Selects specific option
    public void SelectOption(MenuOption option)
    {
        if ((SelectedOption as object) != null)
            SelectedOption.SetNormal();

        SelectedOption = option;
        OnSelectionChange?.Invoke();

        if ((SelectedOption as object) != null)
            SelectedOption.SetSelected();
    }

    public void PressOption(MenuOption option)
    {
        if ((SelectedOption as object) != null)
            SelectedOption.SetPressed();
    }

    public virtual void ProcessInput(InputData input)
    {
        if (input.MovementVector != Vector2Int.zero)
            SelectOption(MoveSelection(input.MovementVector));

        switch (input.KeyCode)
        {
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