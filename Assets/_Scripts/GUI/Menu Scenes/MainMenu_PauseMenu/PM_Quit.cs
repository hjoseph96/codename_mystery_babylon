using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PM_Quit : PauseMenuOption
{
    public override void Execute()
    {
        Menu.QuitGame();
    }
}
