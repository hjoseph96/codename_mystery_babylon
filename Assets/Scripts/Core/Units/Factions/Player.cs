using System.Collections.Generic;

public class Player
{
    public string Name { get; }
    public int TeamId { get; }


    public static Player LocalPlayer = new Player("Player", Team.LocalPlayerTeamId);
    public static Player Ally = new Player("Ally", Team.AllyTeamId);
    public static Player Enemy = new Player("Enemy", Team.EnemyTeamId);
    public static Player OtherEnemy = new Player("Other Enemy", Team.OtherEnemyTeamId);
    public static Player Neutral = new Player("Neutral", Team.NeutralTeamId);

    private readonly List<Unit> _units = new List<Unit>();

    public Player(string name, int teamId)
    {
        Name = name;
        TeamId = teamId;
    }

    public bool IsAlly(Player player)
    {
        switch(TeamId)
        {
            case Team.AllyTeamId: case Team.LocalPlayerTeamId:
                if (player.TeamId == Team.LocalPlayerTeamId || player.TeamId == Team.AllyTeamId)
                    return true;

                return false;
            default:
                return TeamId == player.TeamId;
        }
    }
}