using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class TilemapListExtensions
{
    public static void SortByRenderingOrder(this List<Tilemap> list)
    {
        list.Sort((t1, t2) =>
        {
            var r1 = t1.GetComponent<TilemapRenderer>();
            var r2 = t2.GetComponent<TilemapRenderer>();
            return SortingLayer.GetLayerValueFromID(r1.sortingLayerID) >
                   SortingLayer.GetLayerValueFromID(r2.sortingLayerID) ||
                   r1.sortingLayerID == r2.sortingLayerID &&
                   r1.sortingOrder > r2.sortingOrder
                ? -1
                : 1;
        });
    }

    public static NavigationTile GetTileAtPosition(this List<Tilemap> list, Vector2Int position, out Tilemap foundFTilemap, bool withConfigOnly = true)
    {
        foreach (var tilemap in list)
        {
            if (tilemap.HasTile((Vector3Int) position))
            {
                var tile = tilemap.GetTile<NavigationTile>((Vector3Int) position);
                if (tile != null && (!withConfigOnly || tile.Config != null))
                {
                    foundFTilemap = tilemap;
                    return tile;
                }
            }
        }

        foundFTilemap = null;
        return null;
    }

    public static NavigationTile GetTileAtPosition(this List<Tilemap> list, Vector2Int position, bool withConfigOnly = true)
    {
        return GetTileAtPosition(list, position, out _, withConfigOnly);
    }
}