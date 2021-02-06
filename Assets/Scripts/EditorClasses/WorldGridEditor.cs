using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;


[RequireComponent(typeof(Grid))]
[ExecuteAlways]
public class WorldGridEditor : SerializedMonoBehaviour
{
    /*public enum DebuggingMode
    {
        Passability,
        LineOfSight,
        ExitAndEntrance
    }*/

    [PropertyOrder(0)]
    public bool EnableSelection;

    [FoldoutGroup("Settings"), ShowIf("EnableSelection"), PropertyOrder(1)]
    public bool SingleTilemapMode = true;

    [FoldoutGroup("Settings"), ShowIf("EnableSelection"), PropertyOrder(2), ShowIf("SingleTilemapMode")]
    [ValueDropdown("GetTilemaps")]
    public Tilemap ActiveTilemap;

    [FoldoutGroup("Settings"), ShowIf("EnableSelection"), PropertyOrder(3)]
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

    //public bool EnableDebugging;

    //[FoldoutGroup("Debug"), ShowIf("EnableDebugging"), PropertyOrder(12)]
    //public DebuggingMode DebugMode;

    [HideInInspector] public Grid Grid;
    [HideInInspector] public List<Tilemap> Tilemaps = new List<Tilemap>();

    [HideInInspector] public Vector2Int Size, Origin;


    private void Awake()
    {
        Grid = GetComponent<Grid>();
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
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
        if (/*EnableDebugging &&*/ UnitType != UnitType.None)
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
                    if (tile == null)
                    {
                        continue;
                    }

                    if (DebugPassability)
                        //(DebugMode == DebuggingMode.Passability)
                    {
                        var isPassable = tile.Config.TravelCost.TryGetValueExt(UnitType, out var cost) && cost >= 0;
                        var color = isPassable ? Color.green : Color.red;
                        color.a = isPassable ? 0.3f : 0.4f;
                        Handles.color = color;
                        var center = ActiveTilemap.GetCellCenterWorld((Vector3Int)pos);
                        //Gizmos.color = color;
                        //Gizmos.DrawCube(new Vector2(center.x - 0.5f, center.y - 0.5f), Vector2.one);
                        Handles.DrawSolidRectangleWithOutline(new Rect(center.x - 0.5f, center.y - 0.5f, 1f, 1f),
                            color, color);
                    }

                    if (DebugLineOfSight)
                        //(DebugMode == DebuggingMode.LineOfSight)
                    {
                        var color = tile.Config.HasLineOfSight ? Color.green : Color.red;
                        color.a = tile.Config.HasLineOfSight ? 0.3f : 0.4f;
                        Handles.color = color;
                        var center = ActiveTilemap.GetCellCenterWorld((Vector3Int)pos);
                        Handles.DrawSolidRectangleWithOutline(new Rect(center.x - 0.5f, center.y - 0.5f, 1f, 1f),
                            color, color);
                    }
                    
                    if (DebugExitAndEntrance)
                        //(DebugMode == DebuggingMode.ExitAndEntrance)
                    {
                        var color = Color.red;
                        var offset = 0.04f;
                        var width = 0.12f;

                        var leftTile = Tilemaps.GetTileAtPosition(pos + Vector2Int.left);
                        if (leftTile != null &&
                            (tile.Config.BlockExit.TryGetValue(Direction.Left, out var type) &&
                             (type & UnitType) == UnitType ||
                             leftTile.Config.BlockEntrance.TryGetValue(Direction.Right, out type) &&
                             (type & UnitType) == UnitType))
                        {
                            var cellPos = ActiveTilemap.CellToWorld((Vector3Int)pos);
                            Handles.color = color;
                            Handles.DrawSolidRectangleWithOutline(
                                new Rect(cellPos.x + offset, cellPos.y + offset, width, 1f - 2 * offset),
                                color, color);
                        }

                        var rightTile = Tilemaps.GetTileAtPosition(pos + Vector2Int.right);
                        if (rightTile != null &&
                            (tile.Config.BlockExit.TryGetValue(Direction.Right, out type) &&
                             (type & UnitType) == UnitType ||
                             rightTile.Config.BlockEntrance.TryGetValue(Direction.Left, out type) &&
                             (type & UnitType) == UnitType))
                        {
                            var cellPos = ActiveTilemap.CellToWorld((Vector3Int)pos);
                            Handles.color = color;
                            Handles.DrawSolidRectangleWithOutline(
                                new Rect(cellPos.x + 1.0f - offset - width, cellPos.y + offset, width, 1f - 2 * offset),
                                color, color);
                        }

                        var upTile = Tilemaps.GetTileAtPosition(pos + Vector2Int.up);
                        if (upTile != null &&
                            (tile.Config.BlockExit.TryGetValue(Direction.Up, out type) &&
                             (type & UnitType) == UnitType ||
                             upTile.Config.BlockEntrance.TryGetValue(Direction.Down, out type) &&
                             (type & UnitType) == UnitType))
                        {
                            var cellPos = ActiveTilemap.CellToWorld((Vector3Int)pos);
                            Handles.color = color;
                            Handles.DrawSolidRectangleWithOutline(
                                new Rect(cellPos.x + offset, cellPos.y + 1.0f - offset - width, 1f - 2 * offset, width),
                                color, color);
                        }

                        var downTile = Tilemaps.GetTileAtPosition(pos + Vector2Int.down);
                        if (downTile != null &&
                            (tile.Config.BlockExit.TryGetValue(Direction.Down, out type) &&
                             (type & UnitType) == UnitType ||
                             downTile.Config.BlockEntrance.TryGetValue(Direction.Up, out type) &&
                             (type & UnitType) == UnitType))
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
            //var _pos = ActiveTilemap.GetCellCenterWorld(tilemapPosition);
            //Handles.DrawSolidRectangleWithOutline(new Rect(_pos.x - 0.5f, _pos.y - 0.5f, 1f, 1f),
            //    Color.yellow, Color.yellow);
        }
    }
}
