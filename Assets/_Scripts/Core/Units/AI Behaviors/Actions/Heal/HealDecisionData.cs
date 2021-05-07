using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealDecisionData
{
    public Unit Target;
    public float UrgencyToHeal;
    public Vector2Int PositionToHealAlly;


    public HealDecisionData(Unit _target, float _urgency, Vector2Int _positionToHealFrom)
    {
        Target = _target;
        UrgencyToHeal = _urgency;
        PositionToHealAlly = _positionToHealFrom;

    }

    

}


