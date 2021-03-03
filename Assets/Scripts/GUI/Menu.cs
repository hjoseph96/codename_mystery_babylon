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
    [SoundGroupAttribute] public string SelectedSound;
    [SoundGroupAttribute] public string ConfirmSound;
    [SoundGroupAttribute] public string BackSound;

    private void Start()
    {
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
        SelectedOption = null;
        ResetState();
        Deactivate();
    }


    protected void Activate()
    {
        gameObject.SetActive(true);
        UserInput.Instance.InputTarget = this;
    }

    protected void Deactivate()
    {
        gameObject.SetActive(false);
        UserInput.Instance.InputTarget = null;
    }
    
    // Selects specific option
    public void SelectOption(MenuOption option)
    {
        if ((SelectedOption as object) != null)
            SelectedOption.SetDeselected();

        SelectedOption = option;
        OnSelectionChange?.Invoke();

        if ((SelectedOption as object) != null)
            SelectedOption.SetSelected();
    }

    public virtual void ProcessInput(InputData input)
    {
        if (input.MovementVector != Vector2Int.zero)
            SelectOption(MoveSelection(input.MovementVector));

        switch (input.KeyCode)
        {
            case KeyCode.Z:
                SelectedOption.Execute();
                MasterAudio.PlaySound3DFollowTransform(ConfirmSound, CampaignManager.AudioListenerTransform);

                break;

            case KeyCode.X:
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