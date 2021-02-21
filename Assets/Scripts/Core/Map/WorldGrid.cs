using Tazdraperm.Utility;
using UnityEngine;


[RequireComponent(typeof(Grid))]
[RequireComponent(typeof(WorldGridEditor))]
public class WorldGrid : MonoBehaviour, IInitializable
{
    public static WorldGrid Instance;

    // Pixels per cell/unit
    public const int CellSize = 16;

    public float ExtraVisionRangePerHeightUnit = 1;

    [HideInInspector] public Grid Grid;
    [HideInInspector] public Vector2Int Size;
    public int Width => Size.x;
    public int Height => Size.y;
    public WorldCell this[int x, int y] => _worldGrid[x, y];
    public WorldCell this[Vector2Int pos] => _worldGrid[pos.x, pos.y];

    /// <summary>
    /// Represents game grid
    /// </summary>
    private WorldCell[,] _worldGrid;

    /*
     Instead of Awake, we use custom initialization 
     Every MonoBehaviour that want to initialize something and depends on other classes should implement IInitializable interface and use Init instead of Awake
     Initialization order is specified in EntryPoint GameObject     
     */
    public void Init()
    {
        Instance = this;

        Grid = GetComponent<Grid>();

        //var tilemaps = GetComponentsInChildren<Tilemap>().ToList();
        //tilemaps.SortByRenderingOrder();

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

    /// <summary>
    /// Converts mouse position to grid position
    /// </summary>
    /// <returns></returns>
    public Vector2Int MouseToGrid()
    {
        var pos = CameraUtility.MouseToWorldPosition();
        return (Vector2Int) Grid.WorldToCell(pos);
    }

    /// <summary>
    /// Check if point is inside the grid
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public bool PointInGrid(Vector2Int point)
    {
        return point.x >= 0 &&
               point.y >= 0 &&
               point.x < Width &&
               point.y < Height;
    }
}
