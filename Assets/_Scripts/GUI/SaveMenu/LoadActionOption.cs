using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadActionOption : ActionMenuOption
{
    public new SaveLoadActionMenu Menu;
    public override void Execute()
    {
        Menu.SelectOption(false);
    }
}
