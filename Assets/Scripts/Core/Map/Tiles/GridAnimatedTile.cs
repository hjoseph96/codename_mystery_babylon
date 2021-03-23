using System.Linq;
using System.Collections.Generic;
using UnityEngine;

using Sirenix.OdinInspector;

public class GridAnimatedTile : SerializedMonoBehaviour
{
    private Dictionary<string, List<AnimatedTileConfigurationInfluenceZone>>_tileStates = new Dictionary<string, List<AnimatedTileConfigurationInfluenceZone>>();


    private WorldGrid _worldGrid;

    // Start is called before the first frame update
    void Start()
    {
        _worldGrid = WorldGrid.Instance;

        foreach (AnimatedTileGroup tileGroup in GetComponentsInChildren<AnimatedTileGroup>())
            _tileStates[tileGroup.Name] = tileGroup.InfluenceZones;

        SetTileInfluences(_tileStates.Keys.ToList()[0]);
    }


    void SetTileInfluences(string stateName)
    {
        var influenceZones = _tileStates[stateName];
        foreach (var zone in influenceZones)
        {
            var rect = zone.GetWorldRectInt();
            foreach (var pos in rect.allPositionsWithin)
                _worldGrid[pos - _worldGrid.Origin].SetTileConfig(zone.Config);
        }
    }

    // To be call from Animation Event
    void SetTileState(string stateName) => SetTileInfluences(stateName);
}
