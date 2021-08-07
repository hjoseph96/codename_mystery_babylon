using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(DropdownSubMenu))]
public class OptionsDropDown : MenuOption<OptionsMainMenu>
{
    public DropdownSubMenu subMenu;

    private void Start()
    {
            
    }

    public override void Execute()
    {
        // similar to the slider, determine how to select the options with the input target system
        Debug.Log("Executing on a dropdown");
        subMenu.PreviousMenu = OptionsMainMenu.Instance;
        UserInput.Instance.InputTarget = null;
        UserInput.Instance.InputTarget = subMenu;
        subMenu.dropdown.Show();
    }
}
