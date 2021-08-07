using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootOption : ActionMenuOption
{
    public override void Execute()
    {
        Menu.ShowLootMenu();
    }
}
