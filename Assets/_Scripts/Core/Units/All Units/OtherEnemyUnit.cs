using System.Collections.Generic;

public class OtherEnemyUnit : AIUnit
{
    protected override Player Player { get; } = Player.OtherEnemy;

    public override List<Unit> Enemies()
    {
        var enemies = new List<Unit>();

        var campaignManager = CampaignManager.Instance;

        foreach (PlayerUnit player in campaignManager.PlayerUnits())
            enemies.Add(player);

        foreach (AllyUnit ally in campaignManager.AllyUnits())
            enemies.Add(ally);

        foreach (EnemyUnit enemy in campaignManager.EnemyUnits())
            enemies.Add(enemy);

        return enemies;
    }

    public override List<Unit> Allies() => new List<Unit>(CampaignManager.Instance.OtherEnemyUnits());
}