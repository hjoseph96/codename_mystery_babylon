using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PM_Load : PauseMenuOption
{
    public override void Execute()
    {
        Menu.OpenLoadMenu();
    }
}
