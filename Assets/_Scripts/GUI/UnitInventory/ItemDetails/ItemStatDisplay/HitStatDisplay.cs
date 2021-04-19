public class HitStatDisplay : ItemStatDisplay
{
    public override void SetText(Weapon weapon)
    {
        StatDisplay.SetText($"{weapon.Stats[WeaponStat.Hit].ValueInt}%");
    }
}