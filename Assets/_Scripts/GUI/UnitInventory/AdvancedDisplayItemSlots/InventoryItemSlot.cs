using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryItemSlot : ItemSlot
{
    public UnitInventoryMenu unitInventoryMenu;
    public AdvancedDisplayMenu advancedDisplayMenu;

    public override void Populate(Item item)
    {
        base.Populate(item);
        // TODO: Use overloads, this is a TEMPORARY solution!
        if (item is Weapon weapon)
        {
            if (weapon.CurrentDurability < 0)
                _durability.text = "---";
            else
                _durability.SetText("{0}/{1}", weapon.CurrentDurability, weapon.MaxDurability);
        }
        else
            _durability.SetText("None");
    }

    public override void Execute()
    {
        if (!IsEmpty)
        {
            if(unitInventoryMenu)
                unitInventoryMenu.SelectItemSlot();
            else if(advancedDisplayMenu)
                advancedDisplayMenu.DisplayInformation();



        }
    }
}
