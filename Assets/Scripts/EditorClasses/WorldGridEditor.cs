using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
 #if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Tilemaps;


[RequireComponent(typeof(Grid))]
[ExecuteAlways]
public class WorldGridEditor : SerializedMonoBehaviour
{
    [PropertyOrder(0)]
    public bool EnableSelection, AutoUpdate = true;

    [FoldoutGroup("Settings"), PropertyOrder(1)]
    public bool SingleTilemapMode = true;

    [FoldoutGroup("Settings"), PropertyOrder(2), ShowIf("SingleTilemapMode")]
    [ValueDropdown("GetTilemaps")]
    public Tilemap ActiveTilemap;

    [FoldoutGroup("Settings"), PropertyOrder(3)]
    public bool ShowConfigurationState;

    [FoldoutGroup("Debug"), PropertyOrder(10)]
    [DistinctUnitType]
    public UnitType UnitType;

    [FoldoutGroup("Debug"), PropertyOrder(11)] 
    public bool DebugPassability;
    [FoldoutGroup("Debug"), PropertyOrder(12)]
    public bool DebugLineOfSight;
    [FoldoutGroup("Debug"), PropertyOrder(13)]
    public bool DebugExitAndEntrance;

    [PropertyOrder(20)]
    public TileConfiguration DefaultConfig;

    [SerializeField, HideInInspector] private TileConfigurationInfluenceZone[] _influenceZones;

    [HideInInspector] public Grid Grid;
    [HideInInspector] public List<Tilemap> Tilemaps = new List<Tilemap>();

    [HideInInspector] public Vector2Int Size, Origin;

    private float _lastUpdateTime;
    private const float UpdateInterval = 0.5f;

    public WorldCell[,] WorldGrid
    {
        get
        {
            if (_worldGrid == null)
            {
                BakeWorldGridData();
            }

            return _worldGrid;
        }
    }

    private WorldCell[,] _worldGrid;


    [Button(ButtonSizes.Large), GUIColor(0, 1, 0), PropertyOrder(30)]
    public void BakeWorldGridData()
    {
        if (!Application.isPlaying)
        {
            UpdateTilemaps();
        }

        var width = Size.x;
        var height = Size.y;

        _worldGrid = new WorldCell[width, height];

        //var t = Time.realtimeSinceStartup;

        var heightMapObject = transform.Find("Heightmap");
        heightMapObject.gameObject.SetActive(false);
        var heightMap = heightMapObject.GetComponent<Tilemap>();

        foreach (var zone in _influenceZones)
        {
            var rect = zone.GetWorldRectInt();
            foreach (var pos in rect.allPositionsWithin)
            {
                var gridPos = pos - Origin;
                var tileHeight = 0;

                if (heightMap != null && heightMap.HasTile((Vector3Int) pos))
                {
                    tileHeight = heightMap.GetTile<NavigationTile>((Vector3Int)pos).Height;
                }

                _worldGrid[gridPos.x, gridPos.y] = new WorldCell(gridPos, zone.Config, tileHeight, Vector3.one);
            }

            if (Application.isPlaying)
                Destroy(zone.gameObject);
        }

        for (var j = 0; j < height; j++)
        {
            for (var i = 0; i < width; i++)
            {
                if (_worldGrid[i, j] != null)
                    continue;

                var pos = new Vector2Int(i, j) + Origin;
                var config = DefaultConfig;
                var tileHeight = 0;
                var scale = Vector3.one;

                var tile = Tilemaps.GetTileAtPosition(pos, out var tilemap);

                if (tile != null)
                {
                    config = tile.Config;
                    scale = tilemap.GetTransformMatrix((Vector3Int) pos).lossyScale;

                    if (heightMap != null && heightMap.HasTile((Vector3Int) pos))
                    {
                        tileHeight = heightMap.GetTile<NavigationTile>((Vector3Int) pos).Height;
                    }
                }
                
                _worldGrid[i, j] = new WorldCell(new Vector2Int(i, j), config, tileHeight, scale);
            }
        }

        //Debug.Log("Baking WorldGrid: " + (Time.realtimeSinceStartup - t) * 1000 + " ms");
    }

    private void Awake()
    {
        Grid = GetComponent<Grid>();
    }


    private void Update()
    {
        if (Application.isPlaying || !AutoUpdate)
            return;

        _influenceZones = GetComponentsInChildren<TileConfigurationInfluenceZone>();
        if (Time.time - _lastUpdateTime < UpdateInterval)
            return;

        _lastUpdateTime = Time.time;
        BakeWorldGridData();
    }


    private IEnumerable<Tilemap> GetTilemaps() => GetComponentsInChildren<Tilemap>();

    private void UpdateTilemaps()
    {
        Tilemaps.Clear();
        Tilemaps.AddRange(GetComponentsInChildren<Tilemap>());
        Tilemaps.SortByRenderingOrder();

        Vector2Int max;
        var min = max = default;
        foreach (var tilemap in Tilemaps)
        {
            tilemap.CompressBounds();

            var localBounds = tilemap.localBounds;
            var cellMin = Grid.WorldToCell(tilemap.transform.TransformPoint(localBounds.min));
            var cellMax = Grid.WorldToCell(tilemap.transform.TransformPoint(localBounds.max));
            min = new Vector2Int(Mathf.Min(min.x, cellMin.x), Mathf.Min(min.y, cellMin.y));
            max = new Vector2Int(Mathf.Max(max.x, cellMax.x), Mathf.Max(max.y, cellMax.y));
        }

        Size = max - min;
        Origin = min;
    }

    #if UNITY_EDITOR
    private void OnEnable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        SceneView.duringSceneGui += OnSceneGUI;
    }
    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }
    private void OnSceneGUI(SceneView sceneView)
    {
        if (ActiveTilemap == null)
        {
            var tilemaps = GetTilemaps().ToArray();
            if (tilemaps.Length > 0)
            {
                ActiveTilemap = tilemaps[0];
            }
            else
            {
                return;
            }
        }

        if (!Application.isPlaying)
        {
            UpdateTilemaps();
        }

        if (SingleTilemapMode)
        {
            Tilemaps.Clear();
            Tilemaps.Add(ActiveTilemap);
        }

        if (!EnableSelection)
        {
            return;
        }

        var ev = Event.current;
        var camera = Camera.current;
        var mouseWorldPosition = 
            camera.ScreenToWorldPoint(new Vector3(ev.mousePosition.x, camera.pixelHeight - ev.mousePosition.y, camera.nearClipPlane));
        var tilemapPosition = ActiveTilemap.WorldToCell(mouseWorldPosition);

        var controlID = GUIUtility.GetControlID(FocusType.Passive);
        switch (ev.GetTypeForControl(controlID))
        {
            case EventType.MouseDown:
                if (ev.button == 0)
                {
                    var currentTile = Tilemaps.GetTileAtPosition((Vector2Int) tilemapPosition, false);
                    if (currentTile != null)
                    {
                        var config = currentTile.Config;
                        if (config != null && !ev.control)
                        {
                            Selection.activeObject = config;
                        }
                        else
                        {
                            Selection.activeObject = currentTile;
                        }

                        GUIUtility.hotControl = controlID;
                        ev.Use();
                    }
                }

                break;

            case EventType.MouseUp:
                if (GUIUtility.hotControl == controlID && ev.button == 0)
                {
                    GUIUtility.hotControl = 0;
                    ev.Use();
                }

                break;
        }

        EditorApplication.QueuePlayerLoopUpdate();
    }

    private void OnDrawGizmos()
    {
        var camera = Camera.current;
        // Fix for UI camera
        if (camera.orthographicSize >= 1000)
        {
            return;
        }
        
        var cameraMinPos = camera.ViewportToWorldPoint(new Vector3(0, 0, 0));
        var cameraMaxPos = camera.ViewportToWorldPoint(new Vector3(1f, 1f, 0));
        var gridMinPos = Grid.WorldToCell(cameraMinPos);
        var gridMaxPos = Grid.WorldToCell(cameraMaxPos);
        var viewSize = gridMaxPos - gridMinPos;

        var ev = Event.current;
        var mouseWorldPosition = 
            camera.ScreenToWorldPoint(new Vector3(ev.mousePosition.x, camera.pixelHeight - ev.mousePosition.y, camera.nearClipPlane));

        // Draw Debug information
        if ((DebugPassability || DebugLineOfSight || DebugExitAndEntrance) && UnitType != UnitType.None)
        {
            var xMin = Mathf.Max(gridMinPos.x, Origin.x);
            var yMin = Mathf.Max(gridMinPos.y, Origin.y);
            var xMax = Mathf.Min(gridMinPos.x + viewSize.x, (!Application.isPlaying? Origin.x : 0) + Size.x - 1);
            var yMax = Mathf.Min(gridMinPos.y + viewSize.y, (!Application.isPlaying ? Origin.y : 0) + Size.y - 1);

            //Debug.Log(xMin + " "  + xMax + " " + yMin + " " + yMax);

            for (var j = yMin; j <= yMax; j++)
            {
                for (var i = xMin; i <= xMax; i++)
                {
                    var pos = new Vector2Int(i, j) + (Application.isPlaying ? Origin : Vector2Int.zero);
                    var tile = Tilemaps.GetTileAtPosition(pos);

                    var gridPos = pos - Origin;
                    var cell = WorldGrid[gridPos.x, gridPos.y];

                    if (tile == null)
                    {
                        continue;
                    }

                    if (DebugPassability)
                    {
                        var isPassable = cell.IsPassable(UnitType);
                        var color = isPassable ? Color.green : Color.red;
                        color.a = isPassable ? 0.3f : 0.4f;
                        Handles.color = color;
                        var center = ActiveTilemap.GetCellCenterWorld((Vector3Int)pos);
                        Handles.DrawSolidRectangleWithOutline(new Rect(center.x - 0.5f, center.y - 0.5f, 1f, 1f),
                            color, color);
                    }

                    if (DebugLineOfSight)
                    {
                        var color = cell.HasLineOfSight ? Color.green : Color.red;
                        color.a = cell.HasLineOfSight ? 0.3f : 0.4f;
                        Handles.color = color;
                        var center = ActiveTilemap.GetCellCenterWorld((Vector3Int)pos);
                        Handles.DrawSolidRectangleWithOutline(new Rect(center.x - 0.5f, center.y - 0.5f, 1f, 1f),
                            color, color);
                    }
                    
                    if (DebugExitAndEntrance)
                    {
                        var color = Color.red;
                        var offset = 0.04f;
                        var width = 0.12f;

                        var leftCell = gridPos.x > 0 ? WorldGrid[gridPos.x - 1, gridPos.y] : null;
                        if (leftCell != null && 
                            (!cell.CanExit(Direction.Left, UnitType) || 
                             !leftCell.CanEnter(Direction.Right, UnitType)))
                        {
                            var cellPos = ActiveTilemap.CellToWorld((Vector3Int)pos);
                            Handles.color = color;
                            Handles.DrawSolidRectangleWithOutline(
                                new Rect(cellPos.x + offset, cellPos.y + offset, width, 1f - 2 * offset),
                                color, color);
                        }

                        var rightCell = gridPos.x < Size.x - 1 ? WorldGrid[gridPos.x + 1, gridPos.y] : null;
                        if (rightCell != null && 
                            (!cell.CanExit(Direction.Right, UnitType) || 
                             !rightCell.CanEnter(Direction.Left, UnitType)))
                        {
                            var cellPos = ActiveTilemap.CellToWorld((Vector3Int)pos);
                            Handles.color = color;
                            Handles.DrawSolidRectangleWithOutline(
                                new Rect(cellPos.x + 1.0f - offset - width, cellPos.y + offset, width, 1f - 2 * offset),
                                color, color);
                        }

                        var upTile = gridPos.y < Size.y - 1 ? WorldGrid[gridPos.x, gridPos.y + 1] : null;
                        if (upTile != null &&
                            (!cell.CanExit(Direction.Up, UnitType) || 
                             !upTile.CanEnter(Direction.Down, UnitType)))
                        {
                            var cellPos = ActiveTilemap.CellToWorld((Vector3Int)pos);
                            Handles.color = color;
                            Handles.DrawSolidRectangleWithOutline(
                                new Rect(cellPos.x + offset, cellPos.y + 1.0f - offset - width, 1f - 2 * offset, width),
                                color, color);
                        }

                        var downTile = gridPos.y > 0 ? WorldGrid[gridPos.x, gridPos.y - 1] : null;
                        if (downTile != null &&
                            (!cell.CanExit(Direction.Down, UnitType) || 
                             !downTile.CanEnter(Direction.Up, UnitType)))
                        {
                            var cellPos = ActiveTilemap.CellToWorld((Vector3Int)pos);
                            Handles.color = color;
                            Handles.DrawSolidRectangleWithOutline(
                                new Rect(cellPos.x + offset, cellPos.y + offset, 1f - 2 * offset, width),
                                color, color);
                        }
                    }
                }
            }
        }

        // Draw selection box and handle selection
        if (EnableSelection)
        {
            if (SingleTilemapMode)
            {
                Tilemaps.Clear();
                Tilemaps.Add(ActiveTilemap);
                ActiveTilemap.CompressBounds();
            }

            if (ShowConfigurationState && SingleTilemapMode)
            {
                for (var j = gridMinPos.y; j <= gridMinPos.y + viewSize.y; j++)
                {
                    for (var i = gridMinPos.x; i <= gridMinPos.x + viewSize.x; i++)
                    {
                        var pos = new Vector3Int(i, j, 0);
                        var tile = ActiveTilemap.GetTile<NavigationTile>(pos);
                        if (tile == null)
                        {
                            continue;
                        }

                        var hasConfig = tile.Config != null;
                        var color = hasConfig ? Color.green : Color.red;
                        color.a = hasConfig ? 0.3f : 0.4f;
                        Handles.color = color;
                        var center = ActiveTilemap.GetCellCenterWorld(pos);
                        Handles.DrawSolidRectangleWithOutline(new Rect(center.x - 0.5f, center.y - 0.5f, 1f, 1f),
                            color, color);
                    }
                }
            }

            var tilemapPosition = ActiveTilemap.WorldToCell(mouseWorldPosition);
            Handles.color = Color.yellow;
            Handles.DrawWireCube(ActiveTilemap.GetCellCenterWorld(tilemapPosition), Vector3.one);
        }
    }
    #endif
}
