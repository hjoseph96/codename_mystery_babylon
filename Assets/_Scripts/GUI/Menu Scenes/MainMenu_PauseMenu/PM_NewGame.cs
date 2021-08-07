using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PM_NewGame : PauseMenuOption
{
    public override void Execute()
    {
        // Play new game
        Menu.NewGame();
    }
}
