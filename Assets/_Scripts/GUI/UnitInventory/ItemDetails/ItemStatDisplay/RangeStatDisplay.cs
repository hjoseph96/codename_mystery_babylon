public class RangeStatDisplay : ItemStatDisplay
{
    public override void SetText(Weapon weapon)
    {
        var minRange = weapon.Stats[WeaponStat.MinRange].ValueInt;
        var maxRange = weapon.Stats[WeaponStat.MaxRange].ValueInt;

        var onlyOneRange = minRange == maxRange;
        StatDisplay.SetText(onlyOneRange ? $"{maxRange}" : $"{minRange}-{maxRange}");
    }
}