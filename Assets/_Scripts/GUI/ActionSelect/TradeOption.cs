using UnityEngine;

public class TradeOption : ActionMenuOption
{
    public override void Execute()
    {
        Menu.ShowTradingMenu();
    }
}