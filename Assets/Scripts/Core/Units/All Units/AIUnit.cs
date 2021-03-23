using System.Linq;
using System.Collections.Generic;

using Sirenix.OdinInspector;
using UnityEngine;

public class AIUnit : Unit
{
    [FoldoutGroup("AI properties")]
    [SerializeField] 
    private int _visionRange = 10;

    [FoldoutGroup("AI properties")] 
    [SerializeField] 
    private bool _showVisionRange;

    [FoldoutGroup("State")]
    private bool _isTakingAction = false;
    [ShowInInspector] public bool IsTakingAction { get { return _isTakingAction; } }

    private int CurrentVisionRange => _visionRange +
                                      Mathf.RoundToInt(WorldGrid.Instance[GridPosition].Height * 
                                                       WorldGrid.Instance.ExtraVisionRangePerHeightUnit);

    private bool _movedThisTurn = false;

    private List<Vector2Int> _debugAtkPoints = new List<Vector2Int>();

    public bool HasVision(Vector2Int position)
    {
        return (GridPosition - position).sqrMagnitude <= CurrentVisionRange * CurrentVisionRange &&
               GridUtility.HasLineOfSight(GridPosition, position);
    }

    public GridPath MovePath(Vector2Int destination) => GridUtility.FindPath(this, GridPosition, destination, this.CurrentMovementPoints);


    public void PerformAction()
    {
        _isTakingAction = true;
    }

    public override void AllowAction()
    {
        base.AllowAction();
        _movedThisTurn = false;
        OnFinishedMoving = null;
    }


    // Override in Team-Specific Classes
    public virtual List<Unit> Enemies()
    {
        var enemies = new List<Unit>();
        return enemies;
    }

    public List<Unit> EnemiesWithinSight()
    {
        var enemies = new List<Unit>();

        foreach (Unit enemy in Enemies())
            if (HasVision(enemy.GridPosition))
                enemies.Add(enemy);

        return enemies;
    }

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

    private void Update()
    {
        // TODO: Setup a modular set of behavior/actions and set their priorities in a list.
        if (IsTakingAction)
        {
            _isTakingAction = false;
            
            var attackPoints = CellsWhereICanAttackFrom();
            var sightedEnemies = EnemiesWithinSight();

            if (attackPoints.Count > 0)
            {
                var attackTargets = attackPoints.Keys.ToList();

                if (attackTargets.Count > 1)
                    attackTargets.Sort(
                        delegate (Vector2Int cellOne, Vector2Int cellTwo)
                        {
                            var unitOne = WorldGrid.Instance[cellOne].Unit;
                            var unitTwo = WorldGrid.Instance[cellTwo].Unit;

                            return PreviewRemainingHealth(unitOne).CompareTo(PreviewRemainingHealth(unitTwo));
                        }
                    );

                var unitWhoWillTakeMostDamage = attackTargets[0];
                var targetAtkPoints = attackPoints[unitWhoWillTakeMostDamage];

                _debugAtkPoints = targetAtkPoints;

                var targetCell = FindClosestCellToTarget(targetAtkPoints[0]);

                // Create path to nearest neighnor
                var movePath = MovePath(targetCell);


                this.OnFinishedMoving = null;
                // Post Move logic: face each other and trigger combat. attach event to end this AI's turn in the current phase.
                this.OnFinishedMoving += delegate ()
                {
                    var targetUnit = WorldGrid.Instance[unitWhoWillTakeMostDamage].Unit;

                    // Attacker turns to face defender
                    Vector2 defenderPosition = WorldGrid.Instance.Grid.CellToWorld((Vector3Int)targetUnit.GridPosition);
                    LookAt(defenderPosition);
                    SetIdle();

                    // Defender turns to face attacker
                    Vector2 attackerPosition = WorldGrid.Instance.Grid.CellToWorld((Vector3Int)GridPosition);
                    targetUnit.LookAt(attackerPosition);
                    targetUnit.SetIdle();

                    CampaignManager.Instance.OnCombatReturn = null;
                    CampaignManager.Instance.OnCombatReturn += delegate ()
                    {
                        // When we're back to the map, mark that the AI took action.
                        TookAction();
                    };

                    // TODO: Put confirm sound in campaign manager
                    CampaignManager.Instance.StartCombat(this, targetUnit, "");
                };

                if (!_movedThisTurn)
                {
                    _movedThisTurn = true;
                    Debug.Log("MOVE PATH LENGTH: " + movePath.Length);
                    StartCoroutine(MovementCoroutine(movePath));
                }
            }
            else if (sightedEnemies.Count > 0)
            {
                if (sightedEnemies.Count > 1)
                    sightedEnemies.Sort(
                        delegate (Unit unitOne, Unit unitTwo)
                        {
                            return PreviewRemainingHealth(unitOne).CompareTo(PreviewRemainingHealth(unitTwo));
                        }
                     );

                var unitMoveToTarget = sightedEnemies[0];

                var targetCell = FindClosestCellToTarget(unitMoveToTarget.GridPosition);
                
                // Create path to nearest neighnor
                var movePath = MovePath(targetCell);

                // Post Move logic: face each other and trigger combat. attach event to end this AI's turn in the current phase.
                this.OnFinishedMoving += delegate ()
                {
                    Vector2 defenderPosition = WorldGrid.Instance.Grid.CellToWorld((Vector3Int)unitMoveToTarget.GridPosition);
                    LookAt(defenderPosition);
                    SetIdle();

                    TookAction();
                };

                if (!_movedThisTurn)
                {
                    _movedThisTurn = true;
                    Debug.Log("MOVE PATH LENGTH: " + movePath.Length);
                    StartCoroutine(MovementCoroutine(movePath));
                }
            }
            else
                TookAction();
        }
    }

    private Vector2Int FindClosestCellToTarget(Vector2Int goal)
    {
        var moveRange = GridUtility.GetReachableCells(this);

        if (moveRange.Contains(goal))
            return goal;

        Vector2Int targetCell;

        // Get cells next to the target that are passable and unoccupied
        var targetNeighbors = GridUtility.GetNeighbours(goal);
        targetNeighbors.Select((position) => WorldGrid.Instance[position].IsPassable(this.UnitType));

        // Find the nearest neighbor
        var shortestDistance = targetNeighbors.Min((position) => GridUtility.GetBoxDistance(this.GridPosition, position));
        targetCell = targetNeighbors.First((position) => GridUtility.GetBoxDistance(this.GridPosition, position) == shortestDistance);

        // Take the nearest neighbor and go to the closest REACHABLE cell
        shortestDistance = moveRange.Min((position) => GridUtility.GetBoxDistance(position, targetCell));
        targetCell = moveRange.First((position) => GridUtility.GetBoxDistance(position, targetCell) == shortestDistance);

        return targetCell;
    }

    private int PreviewRemainingHealth(Unit unit)
    {
        var damageDealt = AttackDamage() - unit.Defense;

        return Mathf.Clamp(unit.CurrentHealth - damageDealt, 0, unit.Class.MaxStats[UnitStat.MaxHealth]);
    }



    // Vision Range Scene Mode Visualizer
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
            return;

        var worldGrid = WorldGrid.Instance;
        Gizmos.DrawCube(worldGrid.Grid.GetCellCenterWorld((Vector3Int) GridPosition), Vector3.one);


        if (_debugAtkPoints.Count > 0)
            foreach(Vector2Int atkPoint in _debugAtkPoints)
                Gizmos.DrawCube(worldGrid.Grid.GetCellCenterWorld((Vector3Int) atkPoint), Vector3.one);


        if (!_showVisionRange)
            return;

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
                    Gizmos.DrawCube(worldGrid.Grid.GetCellCenterWorld((Vector3Int) currentPosition), Vector3.one);
            }
        }
    }
}