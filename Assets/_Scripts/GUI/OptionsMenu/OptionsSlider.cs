using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof (SliderSelection))]
public class OptionsSlider : MenuOption<OptionsMainMenu>
{
    public SliderSelection sliderSelection;

    public override void Execute()
    {
        // Slider needs to at this point be set as the active object, and allow value changing, alternatively, can take the input and set this to do up or down a set amount when input given?
        Debug.Log("Executing on a Slider");
        UserInput.Instance.InputTarget = sliderSelection;
        sliderSelection.PreviousMenu = OptionsMainMenu.Instance;
    }

}
