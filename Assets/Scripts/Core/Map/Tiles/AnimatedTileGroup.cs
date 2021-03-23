using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatedTileGroup : MonoBehaviour
{
    public string Name;
    public List<AnimatedTileConfigurationInfluenceZone> InfluenceZones;

    public void Awake()
    {
        foreach (var zone in InfluenceZones)
            zone.SetInvisible();
    }
}
