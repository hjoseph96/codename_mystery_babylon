public class AttackOption : ActionMenuOption
{
    public override void Execute()
    {
        Menu.Deactivate();
        Menu.ShowAttackForecast();
    }
}