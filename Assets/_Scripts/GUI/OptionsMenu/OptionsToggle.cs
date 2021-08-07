using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionsToggle : MenuOption<OptionsMainMenu>
{
    public Toggle toggle;

    public override void Execute()
    {
        toggle.isOn = !toggle.isOn;
    }
}
