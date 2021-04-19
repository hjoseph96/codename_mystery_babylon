using System.Collections.Generic;

public class AllyUnit : AIUnit
{
    protected override Player Player { get; } = Player.Ally;

    public override List<Unit> Enemies()
    {
        var enemies = new List<Unit>();

        var campaignManager = CampaignManager.Instance;

        foreach (OtherEnemyUnit otherEnemy in campaignManager.OtherEnemyUnits())
            enemies.Add(otherEnemy)

        foreach (EnemyUnit enemy in campaignManager.EnemyUnits())
            enemies.Add(enemy);

        return enemies;
    }

    public override List<Unit> Allies()
    {
        var campaignManager = CampaignManager.Instance;

        var allies = new List<Unit>(campaignManager.AllyUnits());

        foreach (PlayerUnit player in campaignManager.PlayerUnits())
            allies.Add(player);

        return allies;
    }
}