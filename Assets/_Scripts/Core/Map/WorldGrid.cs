using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Tazdraperm.Utility;
using UnityEngine;


[RequireComponent(typeof(Grid))]
[RequireComponent(typeof(WorldGridEditor))]
public class WorldGrid : SerializedMonoBehaviour, IInitializable
{
    [OdinSerialize, HideInInspector] public static WorldGrid Instance;

    // Pixels per cell/unit
    public const int CellSize = 16;

    public float ExtraVisionRangePerHeightUnit = 1;
    [SerializeField] public TileConfiguration _nullConfig;
    [SerializeField] private Material _silhouettesMaterial;

    [HideInInspector] public Grid Grid;
    [HideInInspector] public Vector2Int Size;

    public WorldCellTile NullTile => _nullTile ??= new WorldCellTile(_nullConfig, Vector3.one);

    public Vector2Int Origin { get; private set; }
    public int Width => Size.x;
    public int Height => Size.y;
    public WorldCell this[int x, int y] => _worldGrid[x, y];
    public WorldCell this[Vector2Int pos] => _worldGrid[pos.x, pos.y];

    private WorldCellTile _nullTile;

    /// <summary>
    /// Represents game grid
    /// </summary>
    private WorldCell[,] _worldGrid;

    private static readonly int StencilRef = Shader.PropertyToID("_StencilRef");

    /*
     Instead of Awake, we use custom initialization 
     Every MonoBehaviour that want to initialize something and depends on other classes should implement IInitializable interface and use Init instead of Awake
     Initialization order is specified in EntryPoint GameObject     
     */
    public void Init()
    {
        Instance = this;

        Grid = GetComponent<Grid>();

        var editor = GetComponent<WorldGridEditor>();
        Size = editor.Size;
        Origin = editor.Origin;

        // Translate such that (0, 0) cell will have (0, 0) world position
        var offset = Grid.CellToWorld((Vector3Int) editor.Origin);
        transform.position = offset;
        foreach (Transform child in transform)
            child.position -= offset;

        _worldGrid = editor.WorldGridArray;
        editor.Origin = Vector2Int.zero;

        EnableSilhouettes();

        foreach (var group in editor.ColliderGroups)
        {
            if (group.gameObject.activeInHierarchy)
                group.Apply();
        }
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
    
    public void EnableSilhouettes()
    {
        _silhouettesMaterial.SetInt(StencilRef, 1);
    }

    public void DisableSilhouettes()
    {
        _silhouettesMaterial.SetInt(StencilRef, 3);
    }

    public void PlaceGameObject(GameObject objectToPlace, Vector2Int gridPosition)
    {
        if (!PointInGrid(gridPosition))
            throw new System.Exception($"Given GridPositon: [{gridPosition.x}, {gridPosition.y}] is not a valid point in WorldGrid...");

        var worldCell = this[gridPosition];

        var placementPoint = Grid.GetCellCenterWorld((Vector3Int)gridPosition);

        objectToPlace.transform.position = placementPoint;

        var unit = objectToPlace.GetComponent<Unit>();
        if (unit != null)
            worldCell.Unit = unit;
    }

    public AnimatedDoor FindDoor(Vector2Int gridPosition)
    {
        var worldPosition = Grid.GetCellCenterWorld((Vector3Int)gridPosition);

        var doors = FindObjectsOfType<AnimatedDoor>();

        return doors.OrderBy((door) => Vector2.Distance(door.transform.position, worldPosition)).First();
    }
}
