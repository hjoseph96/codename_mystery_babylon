using System.Collections.Generic;
using Tazdraperm.Utility;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Grid))]
[RequireComponent(typeof(WorldGridEditor))]
public class WorldGrid : MonoBehaviour, IInitializable
{
    public static WorldGrid Instance;

    [SerializeField] private TileConfiguration _defaultConfig;

    [HideInInspector] public Grid Grid;
    [HideInInspector] public Vector2Int Size; //, Origin;
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

        /*Vector2Int max;
        var min = max = default;
        foreach (var tilemap in _tilemaps)
        {
            tilemap.CompressBounds();

            var localBounds = tilemap.localBounds;
            var cellMin = Grid.WorldToCell(tilemap.transform.TransformPoint(localBounds.min));
            var cellMax = Grid.WorldToCell(tilemap.transform.TransformPoint(localBounds.max));
            min = new Vector2Int(Mathf.Min(min.x, cellMin.x), Mathf.Min(min.y, cellMin.y));
            max = new Vector2Int(Mathf.Max(max.x, cellMax.x), Mathf.Max(max.y, cellMax.y));
        }

        Size = max - min;
        Origin = min;*/

        var editor = GetComponent<WorldGridEditor>();
        Size = editor.Size;

        _worldGrid = new WorldCell[Width, Height];

        // Translate such that (0, 0) cell will have (0, 0) world position
        var offset = Grid.CellToWorld((Vector3Int) editor.Origin);
        transform.position = offset;
        foreach (Transform child in transform)
        {
            child.position -= offset;
        }

        //Debug.Log(Origin + " _ " + Size);

        for (var j = 0; j < Height; j++)
        {
            for (var i = 0; i < Width; i++)
            {
                var pos = new Vector2Int(i, j) + editor.Origin;
                var config = _defaultConfig;
                var tile = _tilemaps.GetTileAtPosition(pos);

                if (tile != null)
                {
                    config = tile.Config;
                }

                _worldGrid[i, j] = new WorldCell(new Vector2Int(i, j), config);
            }
        }
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
