using System.Linq;
using System.Collections.Generic;
using UnityEngine;

using Sirenix.OdinInspector;
using TenPN.DecisionFlex;

public class AIUnit : Unit
{
    private DecisionFlex _decideAction;

    [FoldoutGroup("AI Properties")]
    [SerializeField] AIUnitTrait _trait;
    [FoldoutGroup("AI Properties")]
    [SerializeField] private int _visionRange = 21;

    [FoldoutGroup("AI Properties")]
    [SerializeField] private bool _showVisionRange;

    [FoldoutGroup("AI Properties")]
    [SerializeField] private bool _showMovementRange;

    [FoldoutGroup("State")]
    private bool _isTakingAction = false;
    [ShowInInspector] public bool IsTakingAction { get => _isTakingAction; }

    private int CurrentVisionRange => _visionRange +
                                      Mathf.RoundToInt(WorldGrid.Instance[GridPosition].Height *
                                                       WorldGrid.Instance.ExtraVisionRangePerHeightUnit);


    private bool _movedThisTurn = false;
    [ShowInInspector] public bool MovedThisTurn { get => _movedThisTurn; }

    private bool _hasDecidedAction = false;

    public AIGroup group;
    [ShowInInspector]
    public int positionInGroup;

    public override Vector2Int PreferredDestination { get => group.PreferredGroupPosition.Position; }

    public override void Init()
    {
        base.Init();

        TraitUtility.ApplyTrait(this, _trait);
        gameObject.name += "_" + _trait.ToString();

        // Stats come from assigned Class for non-recruitable AIUnits
        // TODO: Autolevel and grow stats for AIUnits
        Stats = Class.Stats;
        currentHealth = MaxHealth;
        currentMovementPoints = MaxMoveRange;

        _decideAction = GetComponentInChildren<DecisionFlex>();
        if (_decideAction == null)
            throw new System.Exception($"No DecisionFlex Utility AI opbject was found on AIUnit: {name}");
    }

    private void Update()
    {
        if (IsTakingAction && !_hasDecidedAction)
        {
            _hasDecidedAction = true;

            //group.PreferredGroupPosition()

            _decideAction.PerformAction();
        }
    }

    public Vector2Int MyCellInFormation()
    {
        AIFormation formationGrid = group.CurrentFormation;

        for (int i = 0; i < formationGrid.Width; i++)
        {
            for (int j = 0; j < formationGrid.Height; j++)
            {
                if (formationGrid[i, j] == positionInGroup)
                {
                    return new Vector2Int(i, j) - group.CurrentFormation.Pivot;
                }
            }
        }

        return Vector2Int.zero;
    }

    public bool HasVision(Vector2Int position)
    {
        return (GridPosition - position).sqrMagnitude <= CurrentVisionRange * CurrentVisionRange &&
               GridUtility.HasLineOfSight(SortingLayerId, GridPosition, position);
    }

    public List<Consumable> HealingItems()
    {
        List<Consumable> healingItems = new List<Consumable>();

        var consumables = Inventory.GetItems<Consumable>();
        if (consumables.Length == 0)
            return healingItems;

        for (int i = 0; i < consumables.Length; i++)
            if (consumables[i].CanHeal)
                healingItems.Add(consumables[i]);

        return healingItems;
    }

    public int Priority()
    {
        int priority = 30;

        if (CanAttackLongRange())
            priority += 50;

        if (IsLeader)
            priority += 70;

        return priority;
    }

    public GridPath MovePath(Vector2Int destination) => GridUtility.FindPath(this, GridPosition, destination, this.CurrentMovementPoints);

    public void SetMovedThisTurn() => _movedThisTurn = true;

    public void PerformAction() => _isTakingAction = true;

    public override void AllowAction()
    {
        base.AllowAction();
        _movedThisTurn = false;
        OnFinishedMoving = null;
    }

    public override void TookAction()
    {
        base.TookAction();
        _isTakingAction = false;
        _hasDecidedAction = false;

    }


    // Override in Team-Specific Classes
    public virtual List<Unit> Enemies() => throw new System.Exception("You did not implement Enemies() this AIUnit...");

    // Override in Team-Specific Classes
    public virtual List<Unit> Allies() => throw new System.Exception("You did not implement Allies() this AIUnit...");


    public List<Unit> EnemiesWithinSight()
    {
        var enemies = new List<Unit>();

        foreach (Unit enemy in Enemies())
            if (HasVision(enemy.GridPosition))
                enemies.Add(enemy);

        return enemies;
    }

    public List<Unit> AlliesWithinSight()
    {
        var allies = new List<Unit>();

        foreach (Unit ally in Allies())
            if (HasVision(ally.GridPosition))
                allies.Add(ally);

        return allies;
    }

    public List<T> ReachableAllies<T>() where T : Unit
    {
        var allies = new List<T>();

        foreach (Unit ally in Allies())
            if (ally != this && GridUtility.GetBoxDistance(FindClosestCellTo(ally.GridPosition), ally.GridPosition) <= 1)
                allies.Add(ally as T);

        return allies;
    }

    public bool CanAttackLeader()
    {
        var sightedEnemies = EnemiesWithinSight();
        if (!sightedEnemies.Any((enemy) => enemy.IsLeader))
            return false;

        var attackableLeaders = EnemiesWithinSight().Where((enemy) => enemy.IsLeader == true && CanAttack(enemy));

        if (attackableLeaders.Count() > 0)
            return true;
        else
            return false;
    }

    public List<Vector2Int> VisionRange()
    {
        List<Vector2Int> visionRange = new List<Vector2Int>();

        var worldGrid = WorldGrid.Instance;
        var range = CurrentVisionRange;

        var minX = Mathf.Max(0, GridPosition.x - range);
        var maxX = Mathf.Min(worldGrid.Width - 1, GridPosition.x + range);
        var minY = Mathf.Max(0, GridPosition.y - range);
        var maxY = Mathf.Min(worldGrid.Height - 1, GridPosition.y + range);

        var color = Color.yellow;
        color.a = 0.3f;
        Gizmos.color = color;

        for (var i = minX; i <= maxX; i++)
        {
            for (var j = minY; j <= maxY; j++)
            {
                var currentPosition = new Vector2Int(i, j);
                if (HasVision(currentPosition))
                    visionRange.Add(currentPosition);
            }
        }

        return visionRange;
    }


    public Vector2Int FindClosestCellTo(Vector2Int goal)
    {
        var moveRange = GridUtility.GetReachableCells(this);

        if (moveRange.Contains(goal))
            return goal;

        Vector2Int targetCell;

        // Get cells next to the target that are passable and unoccupied
        var targetNeighbors = GridUtility.GetNeighbours(SortingLayerId, goal);
        targetNeighbors.Select((position) => WorldGrid.Instance[position].IsPassable(this));

        // Find the nearest neighbor
        var shortestDistance = targetNeighbors.Min((position) => GridUtility.GetBoxDistance(GridPosition, position));
        targetCell = targetNeighbors.First((position) => GridUtility.GetBoxDistance(GridPosition, position) == shortestDistance);

        // Take the nearest neighbor and go to the closest REACHABLE cell
        shortestDistance = moveRange.Min((position) => GridUtility.GetBoxDistance(position, targetCell));
        targetCell = moveRange.First((position) => GridUtility.GetBoxDistance(position, targetCell) == shortestDistance);

        return targetCell;
    }

    // Metrics used in Utility AI to make decisions on AIBehavior
    protected override List<Vector2Int> ThreatDetectionRange()
    {
        var moveRange = GridUtility.GetReachableCells(this);
        var attackableUnitPositions = GridUtility.GetAttackableCells(this, moveRange).ToList();
        return moveRange.ToList().Concat(attackableUnitPositions).ToList();
    }

    public float NeedToHeal()
    {
        return 1f - (float)CurrentHealth / MaxHealth;
    }

    public float NeedToHealAlly()
    {
        float healAllyModifier = 1f;
        float urgencyToHealAlly = 0;

        if (_trait == AIUnitTrait.Medic)
        {
            var allyToHeal = ReachableAllies<AIUnit>().Select(ally => ally).Where(ally => ally)
                .OrderByDescending(ally => ally.NeedToHeal()).FirstOrDefault();

            healAllyModifier = allyToHeal != null ? 1.2f : 0;
            urgencyToHealAlly = allyToHeal != null ? allyToHeal.NeedToHeal() * healAllyModifier : 0;
        }

        return urgencyToHealAlly;
    }


    public float NeedToRetreat()
    {
        float moraleModifier = 2 - Morale / 100f;
        float retreatScore = ((ThreatLevel() - 0.2f) + Mathf.Clamp01(NeedToHeal() * moraleModifier)) / 2;
        float needToRetreat = Mathf.Clamp(retreatScore, 0, 1f);
        if (!IsArmed())
            needToRetreat = 1f;

        return needToRetreat;
    }

    public float NeedToBeWithGroup()
    {
        var closestEnemy = EnemiesWithinSight()
            .OrderBy((enemy) => GridUtility.GetBoxDistance(GridPosition, enemy.GridPosition)).FirstOrDefault();

        if (closestEnemy != null)
        {
            float distanceToGroup = GridUtility.GetBoxDistance(GridPosition, group.PreferredGroupPosition.Position);
            float distanceToClosestEnemy = GridUtility.GetBoxDistance(GridPosition, closestEnemy.GridPosition);

            var shouldAttack = Mathf.Clamp01((distanceToClosestEnemy / distanceToGroup) / 2);
            return (NeedToHeal() + (1 - shouldAttack)) / 2;
        }
        else
            return NeedToRetreat();
    }

    public float NeedToResortToDefault()
    {
        return 1f;
    }

    // Vision Range Scene Mode Visualizer
    protected override void OnDrawGizmos()
    {
        if (!Application.isPlaying)
            return;

        // Shows AIUnit's GridPosition as gray square in Play Mode
        var worldGrid = WorldGrid.Instance;
        Gizmos.DrawCube(worldGrid.Grid.GetCellCenterWorld((Vector3Int)GridPosition), Vector3.one);


        if (_showMovementRange)
            foreach (Vector2Int gridPosition in GridUtility.GetReachableCells(this))
                Gizmos.DrawCube(worldGrid.Grid.GetCellCenterWorld((Vector3Int)gridPosition), Vector3.one);

        if (_showVisionRange)
            foreach (Vector2Int gridPosition in VisionRange())
                Gizmos.DrawCube(worldGrid.Grid.GetCellCenterWorld((Vector3Int)gridPosition), Vector3.one);

       // base.OnDrawGizmos();
    }
}