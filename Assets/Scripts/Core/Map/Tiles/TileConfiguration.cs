using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public enum StairsOrientation
{
    None,
    LeftToRight,
    RightToLeft
}

[CreateAssetMenu(fileName = "NewTileConfiguration", menuName = "ScriptableObjects/TileConfiguration", order = 1)]
public class TileConfiguration : SerializedScriptableObject
{
    [Header("Properties")]
    [DictionaryDrawerSettings(KeyLabel = "Unit Type", ValueLabel = "Travel Cost")]
    public Dictionary<UnitType, int> TravelCost = new Dictionary<UnitType, int>();

    public bool HasLineOfSight = true;
    public bool IsStairs;
    [ShowIf("IsStairs")]
    public StairsOrientation StairsOrientation = StairsOrientation.None;

    [HorizontalGroup("Group 1")] public Dictionary<Direction, UnitType> BlockExit = new Dictionary<Direction, UnitType>();

    [HorizontalGroup("Group 2")]
    public Dictionary<Direction, UnitType> BlockEntrance = new Dictionary<Direction, UnitType>();

    [Space, Header("Preview")]
    [DistinctUnitType]
    public UnitType UnitType;

    [Button("Sync")]
    [HorizontalGroup("Group 1", 0.25f)]
    private void SyncExitWithEntrance()
    {
        BlockExit.Clear();
        foreach (var keyValuePair in BlockEntrance)
        {
            BlockExit.Add(keyValuePair.Key, keyValuePair.Value);
        }
    }

    [Button("Sync")]
    [HorizontalGroup("Group 2", 0.25f)]
    private void SyncEntranceWithExit()
    {
        BlockEntrance.Clear();
        foreach (var keyValuePair in BlockExit)
        {
            BlockEntrance.Add(keyValuePair.Key, keyValuePair.Value);
        }
    }
}