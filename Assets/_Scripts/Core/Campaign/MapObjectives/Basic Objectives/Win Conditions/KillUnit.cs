using System.Linq;
using System.Collections.Generic;
using UnityEngine;

using Sirenix.Serialization;

public class KillUnit : MapObjective
{
    [OdinSerialize] private List<Unit> _unitsWhoMustDie;
    public List<Unit> UnitsWhoMustDie { get => _unitsWhoMustDie; }
    
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        objectiveType = ObjectiveType.Win;
    }

    public override bool CheckConditions() => UnitsWhoMustDie.Any((unit) => !unit.IsAlive);
}
