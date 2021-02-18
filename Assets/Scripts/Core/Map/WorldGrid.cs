using System.Collections.Generic;
using Tazdraperm.Utility;
using UnityEngine;
using UnityEngine.Tilemaps;


[RequireComponent(typeof(Grid))]
[RequireComponent(typeof(WorldGridEditor))]
public class WorldGrid : MonoBehaviour, IInitializable
{
    public static WorldGrid Instance;
    public const int CellSize = 16;

    [HideInInspector] public Grid Grid;
    [HideInInspector] public Vector2Int Size;
    public int Width => Size.x;
    public int Height => Size.y;
    public WorldCell this[int x, int y] => _worldGrid[x, y];
    public WorldCell this[Vector2Int pos] => _worldGrid[pos.x, pos.y];

    private WorldCell[,] _worldGrid;
    private readonly List<Tilemap> _tilemaps = new List<Tilemap>();

    public void Init()
    {
        Instance = this;

        Grid = GetComponent<Grid>();
        _tilemaps.AddRange(GetComponentsInChildren<Tilemap>());
        _tilemaps.SortByRenderingOrder();

        var editor = GetComponent<WorldGridEditor>();
        Size = editor.Size;

        // Translate such that (0, 0) cell will have (0, 0) world position
        var offset = Grid.CellToWorld((Vector3Int) editor.Origin);
        transform.position = offset;
        foreach (Transform child in transform)
        {
            child.position -= offset;
        }

        _worldGrid = editor.WorldGrid;
    }

    public Vector2Int MouseToGrid()
    {
        var pos = CameraUtility.MouseToWorldPosition();
        return (Vector2Int) Grid.WorldToCell(pos);
    }

    public bool PointInGrid(Vector2Int point)
    {
        return point.x >= 0 &&
               point.y >= 0 &&
               point.x < Width &&
               point.y < Height;
    }
}
