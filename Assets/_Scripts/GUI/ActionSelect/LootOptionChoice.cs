using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Used to display the lootable bodies when on the ground, keeps track of count of identical names and assigns an additional number if more than 1 exist. 
/// </summary>
public class LootOptionChoice : ActionMenuOption
{
    public TextMeshProUGUI nameText; 
    
    private Unit unit;
    private static List<string> unitNames = new List<string>();
    private const string nameformat = "{0} {1}";

    public override void Execute()
    {
        Menu.BeginLooting(unit);
    }

    public void SetUnit(Unit unit)
    {
        this.unit = unit;
        RunNameCheck();
    }

    private void RunNameCheck()
    {
        int count = 1;
        for(int i = 0; i < unitNames.Count; i++)
        {
            if (unitNames[i].Equals(unit.Name))
                count++;
        }
        if (count > 1)
            nameText.text = string.Format(nameformat, unit.Name, count);
        else
            nameText.text = unit.Name;

        unitNames.Add(unit.Name);
    }

    public void ResetLists()
    {
        unitNames.Clear();
        unitNames = new List<string>();
    }
}
