using System;
using UnityEngine;

public abstract class Menu : MonoBehaviour, IInputTarget
{
    public Menu PreviousMenu { get; set; }

    public MenuOption SelectedOption { get; private set; }

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
        {
            PreviousMenu.Activate();
        }
    }

    // Reset menu state and hide it. Does NOT call OnClose
    public void ResetAndHide()
    {
        SelectedOption = null;
        ResetState();
        Deactivate();
    }

    // Selects specific option
    public void SelectOption(MenuOption option)
    {
        if ((SelectedOption as object) != null)
            SelectedOption.SetDeselected();

        SelectedOption = option;

        if ((SelectedOption as object) != null)
            SelectedOption.SetSelected();
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

    public void ProcessInput(InputData input)
    {
        if (input.MovementVector != Vector2Int.zero)
            SelectOption(MoveSelection(input.MovementVector));

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