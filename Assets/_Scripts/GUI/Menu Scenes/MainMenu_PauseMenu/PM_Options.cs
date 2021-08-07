using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PM_Options : PauseMenuOption
{
    public override void Execute()
    {
        Menu.OpenOptionsMenu();
    }
}
