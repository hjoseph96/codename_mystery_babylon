using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AttackWeakestUnit : AttackBehavior
{
    private bool _checkedAtkPoints = false;

    private Dictionary<Vector2Int, List<Vector2Int>> _attackPoints = new Dictionary<Vector2Int, List<Vector2Int>>();

    public override void Execute() => executionState = AIBehaviorState.Executing;
    

    void Update()
    {
        if (ExecutionState == AIBehaviorState.Executing)
        {
            if (!_checkedAtkPoints)
            {
                _attackPoints = AIAgent.CellsWhereICanAttackFrom();
                _checkedAtkPoints = true;
            }
            var sightedEnemies = AIAgent.EnemiesWithinSight();

            if (_attackPoints.Count > 0)
            {
                var unitWhoWillTakeMostDamage = GetWeakestEnemy(_attackPoints.Keys.ToList());
                var targetAtkPoints = _attackPoints[unitWhoWillTakeMostDamage];

                var targetCell = AIAgent.FindClosestCellTo(targetAtkPoints[0]);

                // Create path to nearest neighnor
                Move(targetCell, () =>
                    {
                        TriggerCombat(unitWhoWillTakeMostDamage);
                        executionState = AIBehaviorState.Complete;
                    });
            }
            else if(sightedEnemies.Count > 0 )
            {
                var weakestEnemyPos = GetWeakestEnemy(sightedEnemies.Select(e => e.GridPosition).ToList());
                Move(weakestEnemyPos, () =>
                {
                    AIAgent.TookAction();
                    _checkedAtkPoints = false;
                    executionState = AIBehaviorState.Complete;
                });
                
            }
            else
            {
                AIAgent.TookAction();
                _checkedAtkPoints = false;
                executionState = AIBehaviorState.Complete;
            }

        }

    }

    private void TriggerCombat(Vector2Int targetGridPosition)
    {
        var targetUnit = WorldGrid.Instance[targetGridPosition].Unit;
        
        if (targetUnit is AIUnit)
        {
            OnMapCombat.OnCombatComplete += delegate ()
            {
                AIAgent.TookAction();
                _checkedAtkPoints = false;
            };
            OnMapCombat.BeginCombat(AIAgent, targetUnit);
        }
        else
            TriggerCombatScene(targetUnit);
    }

    private void TriggerCombatScene(Unit targetUnit)
    {

        // Attacker turns to face defender
        Vector2 defenderPosition = WorldGrid.Instance.Grid.CellToWorld((Vector3Int)targetUnit.GridPosition);
        AIAgent.LookAt(defenderPosition);
        AIAgent.SetIdle();

        // Defender turns to face attacker
        Vector2 attackerPosition = WorldGrid.Instance.Grid.CellToWorld((Vector3Int)AIAgent.GridPosition);
        targetUnit.LookAt(attackerPosition);
        targetUnit.SetIdle();

        CampaignManager.Instance.OnCombatReturn = null;
        CampaignManager.Instance.OnCombatReturn += delegate ()
        {
            // When we're back to the map, mark that the AI took action.
            AIAgent.TookAction();
            _checkedAtkPoints = false;
        };

        // TODO: Put confirm sound in campaign manager
        CampaignManager.Instance.StartCombat(AIAgent, targetUnit, "");
    }

    protected int PreviewRemainingHealth(Unit unit)
    {
        int damageDealt;
        if (AIAgent.EquippedWeapon.Type == WeaponType.Grimiore)
            damageDealt = AIAgent.AttackDamage() - unit.Resistance;
        else
            damageDealt = AIAgent.AttackDamage() - unit.Defense;

        return Mathf.Clamp(unit.CurrentHealth - damageDealt, 0, unit.Class.MaxStats[UnitStat.MaxHealth]);
    }

    protected Vector2Int GetWeakestEnemy(List<Vector2Int> enemies)
    {
        var attackTargets = enemies;


        // TODO: Find out why someitmes, we get attack targets who are null...
        var missingUnits = attackTargets.Where((atkTarget) => WorldGrid.Instance[atkTarget].Unit == null);
        foreach (var unit in missingUnits)
            attackTargets.Remove(unit);


        // If there's more than one attack target, choose the one who will get closer to death
        if (attackTargets.Count > 1)
            attackTargets.Sort(
                delegate (Vector2Int cellOne, Vector2Int cellTwo)
                {
                    var unitOne = WorldGrid.Instance[cellOne].Unit;
                    var unitTwo = WorldGrid.Instance[cellTwo].Unit;

                    return PreviewRemainingHealth(unitOne).CompareTo(PreviewRemainingHealth(unitTwo));
                }
            );

        return attackTargets[0];
    }

    protected void Move(Vector2Int targetCell, Action OnFinishedMoving)
    {
        var movePath = AIAgent.MovePath(targetCell);


        AIAgent.OnFinishedMoving = null;
        // Post Move logic: face each other and trigger combat. attach event to end this AI's turn in the current phase.
        AIAgent.OnFinishedMoving += delegate ()
        {
            OnFinishedMoving?.Invoke();
        };

        if (!AIAgent.MovedThisTurn)
        {
            AIAgent.SetMovedThisTurn();
            
            StartCoroutine(AIAgent.MovementCoroutine(movePath));
        }
    }
}
