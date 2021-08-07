using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PM_Save : PauseMenuOption
{
    public override void Execute()
    {
        Menu.OpenSaveMenu();
    }
}
