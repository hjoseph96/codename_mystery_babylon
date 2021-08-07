using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct OriginalTileTravelCost
{
    public UnitType UnitType;
    public int TravelCost;

    public OriginalTileTravelCost(UnitType unitType, int travelCost)
    {
        UnitType = unitType;
        TravelCost = travelCost;
    }
}
