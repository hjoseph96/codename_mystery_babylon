using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PM_Roster : PauseMenuOption
{
    public override void Execute()
    {
        Menu.OpenRosterMenu();
    }
}
