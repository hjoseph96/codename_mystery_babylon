using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles options that appear for pausing the game from the game scenes, (As well as the main menu?) 
/// </summary>
public class PauseMenu : Menu
{
    public GameObject content;
    private bool _isPaused = false;
    private bool _subMenuOpen = false;
    public static Action<bool> OnGamePaused;
    public List<PauseMenuOption> options = new List<PauseMenuOption>();
    public UICursor _cursor;
    private int _selectedOptionIndex;
    public OptionsMainMenu optionsMenu;
    public RosterMenu rosterMenu;
    public AdvancedDisplayMenu advancedDisplay;
    public ConvoyMenu convoyMenu;
    public SaveLoadMenu saveLoadMenu;

    #region Monobehaviour
    private void Start()
    {
        // This needs to be here so that the start is not overridden and deactivated
        for(int i = 0; i < options.Count; i++)
        {
            options[i].Menu = this;
        }
    }

    public void Update()
    {
        //TODO: Change to use the userInput controls instead of hardwiring escape to update Which will also prevent pressing this key while on the start screen
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            if (_subMenuOpen)
                return;

            if (!_isPaused)
            {
                Activate();
            }
            else
            {
                Close();
            }
            _isPaused = !_isPaused;
            OnGamePaused?.Invoke(_isPaused);
        }
    }
    #endregion

    #region Inherited Methods
    public override void Close()
    {
        base.Close();
        content.SetActive(false);
        UserInput.Instance.InputTarget = GridCursor.Instance;
    }


    public override void Activate()
    {
        UserInput.Instance.InputTarget = this;
        content.SetActive(true);
        MoveSelectionToOption(0, true);
        _selectedOptionIndex = 0;
        _subMenuOpen = false;
    }

    public override void Deactivate()
    {
        Debug.Log("Calling deactivate");
    }


    public override void ProcessInput(InputData input)
    {
        HandleDirectionalMovement(input);

        switch (input.KeyCode)
        {
            case KeyCode.Z:
                if (input.KeyState == KeyState.Down)
                {
                    PressOption(SelectedOption);
                    _subMenuOpen = true;
                }
                else
                {
                    SelectedOption.SetNormal();
                    SelectedOption.Execute();
                }

                break;
        }
    }

    public override MenuOption MoveSelection(Vector2Int input)
    {
        if (!_cursor.IsMoving)
            MoveSelection(-input.y);

        return options[_selectedOptionIndex];
    }

    private void MoveSelection(int input)
    {
        if (input == 0)
            return;

        var newIndex = Mathf.Clamp(_selectedOptionIndex + input, 0, options.Count - 1);

        if (_selectedOptionIndex != newIndex)
        {
            _selectedOptionIndex = newIndex;

            MoveSelectionToOption(_selectedOptionIndex);
        }
    }

    private void MoveSelectionToOption(int index, bool instant = false)
    {
        _cursor.transform.SetParent(options[index].transform, false);
        _cursor.MoveTo(new Vector2(-200, 0), instant);

        //MasterAudio.PlaySound3DFollowTransform(SelectedSound, CampaignManager.AudioListenerTransform);
    }
    #endregion

    #region PauseMenu Methods
    public void OpenOptionsMenu()
    {
        optionsMenu.PreviousMenu = this;
        content.SetActive(false);
        optionsMenu.Activate();
    }

    public void OpenRosterMenu()
    {
        rosterMenu.PreviousMenu = this;
        content.SetActive(false);
        rosterMenu.Activate();
    }

    public void OpenConvoyMenu()
    {
        convoyMenu.PreviousMenu = this;
        content.SetActive(false);
        convoyMenu.Activate();
    }

    public void OpenSaveMenu()
    {
        saveLoadMenu.PreviousMenu = this;
        content.SetActive(false);
        saveLoadMenu.saving = true;
        saveLoadMenu.Activate();
    }

    public void OpenLoadMenu()
    {
        saveLoadMenu.PreviousMenu = this;
        content.SetActive(false);
        saveLoadMenu.saving = false;
        saveLoadMenu.Activate();
    }

    public void QuitGame()
    {
        // Confirm yes or no, quit without saving? 

        Application.Quit();
    }

    public void NewGame()
    {
        Debug.Log("New game pressed");
        StartCoroutine(NewGameStart());
    }
    #endregion

    #region Coroutines
    public IEnumerator NewGameStart()
    {
        UserInput.Instance.InputTarget = null;
        yield return StartCoroutine(SceneLoader.Instance.LoadScene("_Carriage_Interior"));
        SceneManager.UnloadSceneAsync("StartScreen");
        yield return null;
    }
    #endregion
}

