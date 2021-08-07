using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PM_Inventory : PauseMenuOption
{
    public override void Execute()
    {
        Menu.OpenConvoyMenu();
    }
}
