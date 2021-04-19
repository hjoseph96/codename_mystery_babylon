public class WaitOption : ActionMenuOption
{
    public override void Execute()
    {
        Menu.ResetAndHide();
        Menu.FinishTurnForUnit();

        GridCursor.Instance.ClearAll();
        GridCursor.Instance.SetFreeMode();
        UserInput.Instance.InputTarget = GridCursor.Instance;
    }
}