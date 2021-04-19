using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Tilemaps;

#if UNITY_EDITOR
using UnityEditor;
#endif


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

    [FoldoutGroup("Debug"), PropertyOrder(9), ShowIf("EnableSelection"), SerializeField, ReadOnly]
    private Vector2Int _selectedGridPosition;

    [FoldoutGroup("Debug"), PropertyOrder(10)]
    [DistinctUnitType]
    public UnitType UnitType;
    [FoldoutGroup("Debug"), PropertyOrder(11)]
    [SortingLayer]
    public string SortingLayerName;

    [FoldoutGroup("Debug"), PropertyOrder(12)] 
    public bool DebugPassability;
    [FoldoutGroup("Debug"), PropertyOrder(13)]
    public bool DebugLineOfSight;
    [FoldoutGroup("Debug"), PropertyOrder(14)]
    public bool DebugExitAndEntrance;
    [FoldoutGroup("Debug"), PropertyOrder(15)]
    public bool DebugCollisions;

    [FoldoutGroup("Colliders"), PropertyOrder(20)]
    public bool ConsiderCollidersInEditor = true;
    [FoldoutGroup("Colliders"), PropertyOrder(21)]
    public List<ColliderGroup> ColliderGroups;

    [PropertyOrder(50)]
    public TileConfiguration DefaultConfig;

    //[SerializeField, HideInInspector] private TileConfigurationInfluenceZone[] _influenceZones;

    [HideInInspector] public Grid Grid;
    [HideInInspector] public List<Tilemap> Tilemaps = new List<Tilemap>();

    [HideInInspector] public Vector2Int Size, Origin;

    private float _lastUpdateTime;
    private const float UpdateInterval = 0.5f;

    public WorldCell[,] WorldGridArray
    {
        get
        {
            if (_worldGrid == null)
                BakeWorldGridData();

            return _worldGrid;
        }
    }

    private WorldCell[,] _worldGrid;


    [Button(ButtonSizes.Large), GUIColor(0, 1, 0), PropertyOrder(30)]
    public void BakeWorldGridData()
    {
        if (!Application.isPlaying)
            UpdateTilemaps();

        var width = Size.x;
        var height = Size.y;

        _worldGrid = new WorldCell[width, height];

        //var t = Time.realtimeSinceStartup;

        var heightMapObject = transform.Find("Heightmap");
        heightMapObject.gameObject.SetActive(false);
        var heightMap = heightMapObject.GetComponent<Tilemap>();

        for (var j = 0; j < height; j++)
        {
            for (var i = 0; i < width; i++)
            {
                if (_worldGrid[i, j] != null)
                    continue;

                var pos = new Vector2Int(i, j) + Origin;
                var tileHeight = 0;
                if (heightMap != null && heightMap.HasTile((Vector3Int) pos))
                    tileHeight = heightMap.GetTile<NavigationTile>((Vector3Int) pos).Height;

                _worldGrid[i, j] = new WorldCell(new Vector2Int(i, j), tileHeight);

                var tilemapsByLayer = Tilemaps.GetTilemapsBySortingLayer(pos);
                foreach (var sortingLayerId in tilemapsByLayer.Keys)
                {
                    var currentTilemap = tilemapsByLayer[sortingLayerId];
                    if (currentTilemap.HasTile((Vector3Int) pos))
                    {
                        var tile = currentTilemap.GetTile<NavigationTile>((Vector3Int) pos);
                        if (tile.Config != null)
                        {
                            var scale = currentTilemap.GetTransformMatrix((Vector3Int) pos).lossyScale;
                            _worldGrid[i, j].AddTile(sortingLayerId, tile.Config, scale);
                        }
                        else if (sortingLayerId == 0)
                        {
                            Debug.LogError("No tile config at Default layer!");
                        }
                    }
                    else if (sortingLayerId == 0)
                    {
                        Debug.LogError("No tile at Default layer!");
                    }
                }
            }
        }

        if (ConsiderCollidersInEditor)
        {
            foreach (var group in ColliderGroups)
            {
                if (group.gameObject.activeInHierarchy)
                    group.Apply();
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
        if (Application.isPlaying)
            return;

        if (WorldGrid.Instance == null)
            WorldGrid.Instance = GetComponent<WorldGrid>();

        if (!AutoUpdate)
            return;

        //_influenceZones = GetComponentsInChildren<TileConfigurationInfluenceZone>();
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
        //if (_influenceZones.Any((zone) => zone == null))
        //    _influenceZones = _influenceZones.Where((zone) => zone != null).ToArray();
                

        if (ActiveTilemap == null)
        {
            var tilemaps = GetTilemaps().ToArray();
            if (tilemaps.Length > 0)
                ActiveTilemap = tilemaps[0];
            else
                return;
        }

        if (!Application.isPlaying)
            UpdateTilemaps();

        if (SingleTilemapMode)
        {
            Tilemaps.Clear();
            Tilemaps.Add(ActiveTilemap);
        }

        if (!EnableSelection)
            return;

        var ev = Event.current;
        var camera = Camera.current;
        var mouseWorldPosition = camera.ScreenToWorldPoint(new Vector3(ev.mousePosition.x, camera.pixelHeight - ev.mousePosition.y, camera.nearClipPlane));
        var tilemapPosition = Grid.WorldToCell(mouseWorldPosition);

        _selectedGridPosition = new Vector2Int(tilemapPosition.x, tilemapPosition.y) - Origin;

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
                            Selection.activeObject = config;
                        else
                            Selection.activeObject = currentTile;

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
            return;
        
        var cameraMinPos = camera.ViewportToWorldPoint(new Vector3(0, 0, 0));
        var cameraMaxPos = camera.ViewportToWorldPoint(new Vector3(1f, 1f, 0));
        var gridMinPos = Grid.WorldToCell(cameraMinPos);
        var gridMaxPos = Grid.WorldToCell(cameraMaxPos);
        var viewSize = gridMaxPos - gridMinPos;

        var ev = Event.current;
        var mouseWorldPosition = 
            camera.ScreenToWorldPoint(new Vector3(ev.mousePosition.x, camera.pixelHeight - ev.mousePosition.y, camera.nearClipPlane));

        var colliderGroups = Selection.objects
            .OfType<GameObject>()
            .Where(go => go.activeInHierarchy)
            .Select(go => go.GetComponent<ColliderGroup>())
            .Where(c => c != null)
            .ToArray();

        // Draw Debug information
        if (DebugCollisions && colliderGroups.Length > 0 || 
            (DebugPassability || DebugLineOfSight || DebugExitAndEntrance) && UnitType != UnitType.None)
        {
            var sortingLayerId = SortingLayer.NameToID(SortingLayerName);

            var xMin = Mathf.Max(gridMinPos.x, Origin.x);
            var yMin = Mathf.Max(gridMinPos.y, Origin.y);
            var xMax = Mathf.Min(gridMinPos.x + viewSize.x, Size.x - 1 + Origin.x);
            var yMax = Mathf.Min(gridMinPos.y + viewSize.y, Size.y - 1 + Origin.y);

            //Debug.Log(xMin + " "  + xMax + " " + yMin + " " + yMax);

            for (var j = yMin; j <= yMax; j++)
            {
                for (var i = xMin; i <= xMax; i++)
                {
                    var pos = new Vector2Int(i, j);// + Origin;

                    if (DebugCollisions)
                    {
                        foreach (var colliderGroup in colliderGroups)
                        {
                            var correctedPos = pos - Origin;
                            if (colliderGroup.Contains(correctedPos))
                            {
                                var color = Color.red;
                                color.a = 0.35f;
                                Handles.color = color;
                                var center = Grid.GetCellCenterWorld((Vector3Int) pos);
                                Handles.DrawSolidRectangleWithOutline(
                                    new Rect(center.x - 0.5f, center.y - 0.5f, 1f, 1f),
                                    color, color);
                                break;
                            }
                        }
                    }

                    var gridPos = pos - Origin;
                    var cell = WorldGridArray[gridPos.x, gridPos.y];

                    if (DebugPassability)
                    {
                        var isPassable = cell.IsPassable(sortingLayerId, UnitType);
                        var color = isPassable ? Color.green : Color.red;
                        color.a = isPassable ? 0.3f : 0.4f;
                        Handles.color = color;
                        var center = Grid.GetCellCenterWorld((Vector3Int) pos);
                        Handles.DrawSolidRectangleWithOutline(new Rect(center.x - 0.5f, center.y - 0.5f, 1f, 1f),
                            color, color);
                    }

                    if (DebugLineOfSight)
                    {
                        var color = cell.HasLineOfSight(sortingLayerId) ? Color.green : Color.red;
                        color.a = cell.HasLineOfSight(sortingLayerId) ? 0.3f : 0.4f;
                        Handles.color = color;
                        var center = Grid.GetCellCenterWorld((Vector3Int)pos);
                        Handles.DrawSolidRectangleWithOutline(new Rect(center.x - 0.5f, center.y - 0.5f, 1f, 1f),
                            color, color);
                    }
                    
                    if (DebugExitAndEntrance)
                    {
                        var color = Color.red;
                        var offset = 0.04f;
                        var width = 0.12f;

                        var leftCell = gridPos.x > 0 ? WorldGridArray[gridPos.x - 1, gridPos.y] : null;
                        if (leftCell != null && 
                            (!cell.CanExit(Direction.Left, sortingLayerId, UnitType) || 
                                                           !leftCell.CanEnter(Direction.Right, sortingLayerId, UnitType)))
                        {
                            var cellPos = Grid.CellToWorld((Vector3Int)pos);
                            Handles.color = color;
                            Handles.DrawSolidRectangleWithOutline(
                                new Rect(cellPos.x + offset, cellPos.y + offset, width, 1f - 2 * offset),
                                color, color);
                        }

                        var rightCell = gridPos.x < Size.x - 1 ? WorldGridArray[gridPos.x + 1, gridPos.y] : null;
                        if (rightCell != null && 
                            (!cell.CanExit(Direction.Right, sortingLayerId, UnitType) || 
                             !rightCell.CanEnter(Direction.Left, sortingLayerId, UnitType)))
                        {
                            var cellPos = Grid.CellToWorld((Vector3Int)pos);
                            Handles.color = color;
                            Handles.DrawSolidRectangleWithOutline(
                                new Rect(cellPos.x + 1.0f - offset - width, cellPos.y + offset, width, 1f - 2 * offset),
                                color, color);
                        }

                        var upTile = gridPos.y < Size.y - 1 ? WorldGridArray[gridPos.x, gridPos.y + 1] : null;
                        if (upTile != null &&
                            (!cell.CanExit(Direction.Up, sortingLayerId, UnitType) || 
                             !upTile.CanEnter(Direction.Down, sortingLayerId, UnitType)))
                        {
                            var cellPos = Grid.CellToWorld((Vector3Int)pos);
                            Handles.color = color;
                            Handles.DrawSolidRectangleWithOutline(
                                new Rect(cellPos.x + offset, cellPos.y + 1.0f - offset - width, 1f - 2 * offset, width),
                                color, color);
                        }

                        var downTile = gridPos.y > 0 ? WorldGridArray[gridPos.x, gridPos.y - 1] : null;
                        if (downTile != null &&
                            (!cell.CanExit(Direction.Down, sortingLayerId, UnitType) || 
                             !downTile.CanEnter(Direction.Up, sortingLayerId, UnitType)))
                        {
                            var cellPos = Grid.CellToWorld((Vector3Int)pos);
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
                            continue;

                        var hasConfig = tile.Config != null;
                        var color = hasConfig ? Color.green : Color.red;
                        color.a = hasConfig ? 0.3f : 0.4f;
                        Handles.color = color;
                        var center = Grid.GetCellCenterWorld(pos);
                        Handles.DrawSolidRectangleWithOutline(new Rect(center.x - 0.5f, center.y - 0.5f, 1f, 1f),
                            color, color);
                    }
                }
            }

            var tilemapPosition = Grid.WorldToCell(mouseWorldPosition);
            Handles.color = Color.yellow;
            Handles.DrawWireCube(Grid.GetCellCenterWorld(tilemapPosition), Vector3.one);
        }


        if (DebugCollisions && colliderGroups.Length > 0)
        {
            var tilemapPosition = Grid.WorldToCell(mouseWorldPosition);
            Handles.color = Color.blue;
            Handles.DrawWireCube(Grid.GetCellCenterWorld(tilemapPosition), Vector3.one);
        }
    }
    #endif
}
