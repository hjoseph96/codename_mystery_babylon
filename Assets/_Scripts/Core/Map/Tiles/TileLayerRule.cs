using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct TileLayerRule
{
    public readonly bool IsColliderEnabled;
    public readonly string SortingLayer;

    public readonly List<Collider2D> Obstacles;

    public TileLayerRule (bool isColliderEnabled, string sortingLayer, List<Collider2D> obstacles = null) {
        IsColliderEnabled = isColliderEnabled;
        SortingLayer = sortingLayer;
        Obstacles = obstacles;
    }
}
