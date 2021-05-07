using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUnit : Unit
{
    protected override Player Player { get; } = Player.LocalPlayer;

    protected override List<Vector2Int> ThreatDetectionRange() => GridUtility.GetReachableCells(this, -1, true).ToList();

    public override Vector2Int PreferredDestination { get => CampaignManager.Instance.PlayerDestination; }

    public override void Init()
    {
        base.Init();
    }

    public List<AIUnit> Enemies()
    {
        var enemies = new List<AIUnit>();

        var campaignManager = CampaignManager.Instance;

        foreach (EnemyUnit enemy in campaignManager.EnemyUnits())
            enemies.Add(enemy);

        foreach (OtherEnemyUnit otherEnemy in campaignManager.OtherEnemyUnits())
            enemies.Add(otherEnemy);

        return enemies;
    }

    public List<AIUnit> Allies() => new List<AIUnit>(CampaignManager.Instance.AllyUnits());

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
    }

}