public class WeightStatDisplay : ItemStatDisplay
{
    public override void SetText(Weapon weapon)
    {
        StatDisplay.SetText($"{weapon.Weight}");
    }
}