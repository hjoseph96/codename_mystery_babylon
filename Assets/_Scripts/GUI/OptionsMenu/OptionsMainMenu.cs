using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Main menu options selection, could be used to do in game menu option selection calls as well
/// <br>The functions inside this script are called via Unity Events directly on each of the objects that have the names associated with the method names</br>
/// <br>Refer to the Options Menu Prefab in assets for modification of these values, or references. </br>
/// </summary>
public class OptionsMainMenu : Menu
{

    #region Variables
    public GameObject content;
    public List<MenuOption> _options = new List<MenuOption>();
    public UICursor _cursor;

    private int _selectedOptionIndex;

    //TODO: Remove static values and instead use some other reference point
    public static OptionsMainMenu Instance; 
    #endregion

    #region Monobehaviour
    private void Awake()
    {
        _isInitialized = true;
        Instance = this;
    }


    #endregion

    //With sound settings We can either feed directly to the manager if it's always active, or trigger an action that is called which sets the sound levels for objects
    #region Sound Levels
    public void OnSFXVolumeChanged(float sliderValue)
    {
        //mixer.SetFloat("MusicVol", Mathf.Log10(sliderValue) * 20);
        PlayerPrefs.SetFloat("SoundVolume", sliderValue);
    }

    public void OnMusicVolumeChanged(float sliderValue)
    {
        //mixer.SetFloat("MusicVol", Mathf.Log10(sliderValue) * 20);
        PlayerPrefs.SetFloat("MusicVolume", sliderValue);
    }
    #endregion

    #region UI Settings
    /// <summary>
    /// Takes in the input for typewriter and changes how fast that displays text to the ui by default. 
    /// </summary>
    /// <param name="value"></param>
    public void OnTypewriterSpeedChanged(float value)
    {
        PlayerPrefs.SetFloat("TypewriterSpeed", value);
    }
    #endregion

    #region Visual Settings
    public void OnFullscreenToggled(bool fullscreen)
    {
        PlayerPrefs.SetInt("Fullscreen", fullscreen ? 1 : 0);
    }

    public void OnResolutionChanged(int resolutionIndex)
    {
        PlayerPrefs.SetInt("Resolution", resolutionIndex);
    }
    #endregion

    #region Game Settings
    public void OnShowBattleCinematicsPressed(bool showBattleCinematics)
    {
        PlayerPrefs.SetInt("BattleCinematicsActive", showBattleCinematics ? 1 : 0);
    }
    #endregion

    #region Saving, Loading and Other
    /// <summary>
    /// Saves modified preferences to Disk. Can be changed to call this inside each of the change functions, or just when leaving the options menu. 
    /// </summary>
    public void Save()
    {
        PlayerPrefs.Save();
    }
    #endregion

    #region Inherited
    public override void Activate()
    {
        content.SetActive(true);
        UserInput.Instance.InputTarget = this;
        MoveSelectionToOption(0, true);
        _selectedOptionIndex = 0;
    }

    public override void Deactivate()
    {
/*        Debug.Log("What's calling me");
        content.SetActive(false);
        UserInput.Instance.InputTarget = null;*/
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

        var newIndex = Mathf.Clamp(_selectedOptionIndex + input, 0, _options.Count - 1);

        if (_selectedOptionIndex != newIndex)
        {
            _selectedOptionIndex = newIndex;

            MoveSelectionToOption(_selectedOptionIndex);
        }
    }

    private void MoveSelectionToOption(int index, bool instant = false)
    {
        _cursor.transform.SetParent(_options[index].transform, false);
        _cursor.MoveTo(Vector2.zero, instant);

        //MasterAudio.PlaySound3DFollowTransform(SelectedSound, CampaignManager.AudioListenerTransform);
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
                content.SetActive(false);
                PreviousMenu.Activate();
                break;
        }
    }
    #endregion
}
