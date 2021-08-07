using System;
using System.Collections.Generic;
using System.Diagnostics;

public class EnemyUnit : AIUnit
{
    protected override Player Player { get; } = Player.Enemy;

    public override List<Unit> Enemies()
    {
        var enemies = new List<Unit>();

        var campaignManager = CampaignManager.Instance;

        foreach (PlayerUnit player in campaignManager.PlayerUnits())
            enemies.Add(player);

        foreach (AllyUnit ally in campaignManager.AllyUnits())
            enemies.Add(ally);

        foreach (OtherEnemyUnit otherEnemy in campaignManager.OtherEnemyUnits())
            enemies.Add(otherEnemy);

        return enemies;
    }

    public override List<Unit> Allies() => new List<Unit>(CampaignManager.Instance.EnemyUnits());
}