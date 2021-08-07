using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicItemSlot : ItemSlot
{
    public AdvancedDisplayMenu menu;

    public override void Execute()
    {
        menu.DisplayInformation();

    }

    private void Start()
    {

    }
}
