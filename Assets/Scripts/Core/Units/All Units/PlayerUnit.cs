using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUnit : Unit
{
    protected override Player Player { get; } = Player.LocalPlayer;

    protected override List<Vector2Int> ThreatDetectionRange() => GridUtility.GetReachableCells(this, -1, true).ToList();

}