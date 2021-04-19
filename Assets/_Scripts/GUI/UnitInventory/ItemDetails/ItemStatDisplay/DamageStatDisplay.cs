public class DamageStatDisplay : ItemStatDisplay
{
    public override void SetText(Weapon weapon)
    {
        StatDisplay.SetText($"{weapon.Stats[WeaponStat.Damage].ValueInt}");
    }
}