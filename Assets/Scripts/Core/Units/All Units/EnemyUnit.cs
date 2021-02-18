public class EnemyUnit : AIUnit
{
    protected override Player Player { get; } = Player.Enemy;
}