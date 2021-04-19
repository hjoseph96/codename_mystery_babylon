public class CritStatDisplay : ItemStatDisplay
{
    public override void SetText(Weapon weapon)
    {
        StatDisplay.SetText($"{weapon.Stats[WeaponStat.CriticalHit].ValueInt}%");
    }
}