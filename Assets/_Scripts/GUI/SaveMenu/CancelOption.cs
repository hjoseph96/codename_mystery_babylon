using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CancelOption : ActionMenuOption
{
    public new SaveLoadActionMenu Menu;
    public override void Execute()
    {
        Menu.SelectOption(true);
    }
}
