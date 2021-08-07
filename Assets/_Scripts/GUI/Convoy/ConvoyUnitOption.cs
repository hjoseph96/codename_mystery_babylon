using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConvoyUnitOption : ActionMenuOption
{
    public new ConvoyActionMenu Menu;
    [SerializeField]
    protected Unit _unit;
    public override void Execute()
    {
        Menu.PerformItemMove(_unit);
    }

    public void Init(ActionSelectMenu menu, Sprite normal, Sprite selected, Sprite pressed, Unit unit, string objectText = null)
    {
        base.Init(menu, normal, selected, pressed, objectText);
        _unit = unit;
    }

    public void SetText(string text)
    {
        _objectName.text = text;
    }
}
