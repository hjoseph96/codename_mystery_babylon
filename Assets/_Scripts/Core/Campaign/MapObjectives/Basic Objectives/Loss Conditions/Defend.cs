using System.Linq;
using System.Collections.Generic;
using UnityEngine;

using Sirenix.Serialization;

public class Defend : MapObjective
{
    [OdinSerialize] private Dictionary<int, List<Vector2Int>> _positionsToDefend = new Dictionary<int, List<Vector2Int>>();
    public Dictionary<int, List<Vector2Int>> PositionsToDefend { get => _positionsToDefend; }

    [OdinSerialize] private List<Unit> _unitsWhoCannotDie = new List<Unit>();
    //public List<Unit> UnitsWhoCannotDie { get => _unitsWhoCannotDie;  }


    protected override void Start()
    {
        base.Start();

        objectiveType = ObjectiveType.Loss;
    }

    public void AddUnitWhoCannotDie(Unit unit)
    {
        if (!_unitsWhoCannotDie.Contains(unit))
            _unitsWhoCannotDie.Add(unit);
    }

    /// <summary>
    /// Check if all groups of Seize Points have at least on WorldCell that has been seized by a EnemyUnit.
    /// </summary>
    /// <returns></returns>
    public override bool CheckConditions()
    {
        if (_unitsWhoCannotDie.Any((unit) => unit.IsAlive == false))
            return true;

        var enemyTeamIds = new List<int> { Player.Enemy.TeamId, Player.OtherEnemy.TeamId };

        if (_positionsToDefend == null)
            return false;

        if (_positionsToDefend.Count == 0)
            return false;

        var passedObjective = _positionsToDefend.All(delegate (KeyValuePair<int, List<Vector2Int>> entry)
        {
            var anySeizedByEnemy = entry.Value.Any(delegate (Vector2Int seizePoint)
            {
                var cellToCheck = worldGrid[seizePoint];
                var seizedByEnemy = enemyTeamIds.Contains(cellToCheck.Unit.TeamId);

                return seizedByEnemy;
            });

            return anySeizedByEnemy;
        });


        return passedObjective;
    }
}
