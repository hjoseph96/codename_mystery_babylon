using System.Linq;
using System.Collections.Generic;
using UnityEngine;

using Sirenix.Serialization;

public class PreventEscape : MapObjective
{
    [OdinSerialize] private List<Unit> _unitsWhoCannotEscape;
    public List<Unit> UnitsWhoCannotEscape { get => _unitsWhoCannotEscape; }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        objectiveType = ObjectiveType.Loss;
    }

    public override bool CheckConditions()
    {
        return UnitsWhoCannotEscape.Any((escapee) => escapee.HasEscaped);
    }
}
