using UnityEditor;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.Tilemaps;


public class NavigationTile : Tile
{
    public TileConfiguration Config;


#if UNITY_EDITOR
    [MenuItem("Assets/Create/Tiles/Navigation Tile")]
    public static void CreateNavigationTile()
    {
        var path =
            UnityEditor.EditorUtility.SaveFilePanelInProject(
                "Save Navigation Tile",
                "New Navigation Tile",
                "asset",
                "Save Navigation Tile",
                "Assets/Tilemap/Tiles");

        if (path == "")
        {
            return;
        }

        AssetDatabase.CreateAsset(CreateInstance<NavigationTile>(), path);
    }

    [CreateTileFromPalette]
    public static TileBase CreateNavigationTile(Sprite sprite)
    {
        var tile = CreateInstance<NavigationTile>();
        tile.sprite = sprite;
        tile.name = sprite.name;
        return tile;
    }
#endif
}
