using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConvoyActionOption : ActionMenuOption
{
    public new ConvoyActionMenu Menu;
    public override void Execute()
    {
        Menu.OpenUnitMenu();
    }

    public void SetText(string text)
    {
        _objectName.text = text;
    }
}
