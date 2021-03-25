using System.Linq;
using System.Collections.Generic;
using UnityEngine;

using Sirenix.OdinInspector;
using TenPN.DecisionFlex;

public class AIUnit : Unit
{
    private DecisionFlex _decideAction;

    [FoldoutGroup("AI Properties")]
    [SerializeField] private int _visionRange = 10;

    [FoldoutGroup("AI Properties")] 
    [SerializeField] private bool _showVisionRange;

    [FoldoutGroup("AI Properties")]
    [SerializeField] private bool _showMovementRange;

    [FoldoutGroup("State")]
    private bool _isTakingAction = false;
    [ShowInInspector] public bool IsTakingAction { get => _isTakingAction; }

    [ShowInInspector] private int CurrentVisionRange => _visionRange +
                                      Mathf.RoundToInt(WorldGrid.Instance[GridPosition].Height * 
                                                       WorldGrid.Instance.ExtraVisionRangePerHeightUnit);

    private bool _movedThisTurn = false;
    [ShowInInspector] public bool MovedThisTurn { get => _movedThisTurn; }

    private bool _hasDecidedAction = false;

    public override void Init()
    {
        base.Init();

        _decideAction = GetComponentInChildren<DecisionFlex>();
        if (_decideAction == null)
            throw new System.Exception($"No DecisionFlex Utility AI opbject was found on AIUnit: {name}");
    }

    private void Update()
    {
        if (IsTakingAction && !_hasDecidedAction)
        {
            _hasDecidedAction = true;

            _decideAction.PerformAction();
        }
    }

    public bool HasVision(Vector2Int position)
    {
        return (GridPosition - position).sqrMagnitude <= CurrentVisionRange * CurrentVisionRange &&
               GridUtility.HasLineOfSight(GridPosition, position);
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

    // Metrics used in Utility AI to make decisions on AIBehavior
    protected override List<Vector2Int> ThreatDetectionRange()
    {
        var moveRange = GridUtility.GetReachableCells(this);
        var attackableUnitPositions = GridUtility.GetAttackableCells(this, moveRange).ToList();
        return moveRange.ToList().Concat(attackableUnitPositions).ToList();
    }

    public float NeedToHeal() => 1f - (float)CurrentHealth / MaxHealth;

    public float NeedToRetreat()
    {
        float needToRetreat = Mathf.Clamp((ThreatLevel() - 0.2f) + NeedToHeal(), 0, 1f);
        if (!IsArmed())
            needToRetreat = 1f;

        return needToRetreat;
    }

    // Vision Range Scene Mode Visualizer
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
            return;

        // Shows AIUnit's GridPosition as gray square in Play Mode
        var worldGrid = WorldGrid.Instance;
        Gizmos.DrawCube(worldGrid.Grid.GetCellCenterWorld((Vector3Int) GridPosition), Vector3.one);


        if (_showMovementRange)
            foreach(Vector2Int gridPosition in GridUtility.GetReachableCells(this))
                Gizmos.DrawCube(worldGrid.Grid.GetCellCenterWorld((Vector3Int)gridPosition), Vector3.one);

        if (_showVisionRange)
            foreach(Vector2Int gridPosition in VisionRange())
                Gizmos.DrawCube(worldGrid.Grid.GetCellCenterWorld((Vector3Int) gridPosition), Vector3.one);
    }
}