using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Contains the information for a reinforcement group, including spawn points, units
/// </summary>
[Serializable]
public class ReinforcementGroup
{
    [Tooltip("Used to search for this group when calling to spawn reinforcements, ensure it's unique from others in the list to be able to call it")]
    public int id;
    [Tooltip("If ticked, will not remove from the list of possible reinforcements, for those maps with infinite reinforcements")]
    public bool repeatable = false;
    [Tooltip("Put prefabs in place to have them load in when the reinforcement group is spawned")]
    public List<Unit> unitsToSpawn = new List<Unit>();
    [Tooltip("This value should always match the amount of units to spawn, alternatively, further code can be added to look for adjacent tiles if not supplied")]
    public List<Vector2Int> gridPositionsToSpawn = new List<Vector2Int>();
    // WorldGrid.Instance[GridPosition] to call the points for spawning
}
