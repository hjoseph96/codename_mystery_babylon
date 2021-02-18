using System.Collections.Generic;

public class Player
{
    public string Name { get; }
    public int TeamId { get; }

    public IEnumerable<Unit> Units => _units;

    public static Player LocalPlayer = new Player("Player", Team.LocalPlayerTeamId);
    public static Player Ally = new Player("Ally", Team.LocalPlayerTeamId);
    public static Player Enemy = new Player("Enemy", Team.EnemyTeamId);
    public static Player OtherEnemy = new Player("Other Enemy", Team.OtherEnemyTeamId);
    public static Player Other = new Player("Other", Team.OtherTeamId);

    private readonly List<Unit> _units = new List<Unit>();

    public Player(string name, int teamId)
    {
        Name = name;
        TeamId = teamId;
    }

    public void AddUnit(Unit unit)
    {
        _units.Add(unit);
    }

    public void RemoveUnit(Unit unit)
    {
        _units.Remove(unit);
    }

    public bool IsAlly(Player player) => TeamId == player.TeamId;
}