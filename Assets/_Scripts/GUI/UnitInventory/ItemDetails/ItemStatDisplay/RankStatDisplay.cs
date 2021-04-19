public class RankStatDisplay : ItemStatDisplay
{
    public override void SetText(Weapon weapon)
    {
        var rank = weapon.RequiredRank;
        StatDisplay.SetText(rank.ToString());
    }
}