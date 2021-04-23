using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEditor;
using UnityEngine;


public class ColliderGroupBase : SerializedMonoBehaviour
{
    protected WorldGridEditor Editor
    {
        get
        {
            if (_editor == null)
                _editor = FindObjectOfType<WorldGridEditor>();

            return _editor;
        }
    }

    private WorldGridEditor _editor;

    protected readonly List<Collider2D> OverlapBoxResults = new List<Collider2D>();

    public virtual void Apply() 
    { }

    public virtual void Revert()
    { }
}


[ExecuteInEditMode]
public class ColliderGroupSimple : ColliderGroupBase
{
    [SortingLayer] public string SortingLayerName;
    public TileConfiguration Config;

    [HideInInspector] public int SortingLayerId => SortingLayer.GetLayerValueFromName(SortingLayerName);
    [HideInInspector, OdinSerialize]
    private HashSet<Vector2Int> _collisions = new HashSet<Vector2Int>();

    private WorldCellTile _overrideTile;

    [Button(ButtonSizes.Large), GUIColor(0, 1, 0)]
    public void UpdateCollisions()
    {
        var filter = new ContactFilter2D
        {
            useLayerMask = true,
            layerMask = LayerMask.GetMask("Tilemap Colliders")
        };

        _collisions.Clear();

        var colliders = GetComponentsInChildren<Collider2D>();
        foreach (var currentCollider in colliders)
        {
            var bounds = currentCollider.bounds;
            var min = Editor.Grid.WorldToCell(bounds.min);
            var max = Editor.Grid.WorldToCell(bounds.max);

            for (var y = min.y; y <= max.y; y++)
            {
                for (var x = min.x; x <= max.x; x++)
                {
                    OverlapBoxResults.Clear();
                    if (Physics2D.OverlapBox(Editor.Grid.GetCellCenterWorld(new Vector3Int(x, y, 0)), Vector2.one, 0,
                        filter, OverlapBoxResults) > 0 && OverlapBoxResults.IndexOf(currentCollider) >= 0)
                        _collisions.Add(new Vector2Int(x, y) - Editor.Origin);
                }
            }
        }

        EditorUtility.SetDirty(gameObject);
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

    private void OnSceneGUI(SceneView sceneView)
    {
        if (!Selection.objects.Contains(gameObject) || !gameObject.activeInHierarchy || !Editor.DebugCollisions)
            return;

        var ev = Event.current;
        var camera = Camera.current;
        var mouseWorldPosition = camera.ScreenToWorldPoint(new Vector3(ev.mousePosition.x, camera.pixelHeight - ev.mousePosition.y, camera.nearClipPlane));
        var gridPosition = (Vector2Int)Editor.Grid.WorldToCell(mouseWorldPosition) - Editor.Origin;

        var controlID = GUIUtility.GetControlID(FocusType.Passive);
        switch (ev.GetTypeForControl(controlID))
        {
            case EventType.MouseDown:
                if (ev.button == 0 || ev.button == 1)
                {
                    if (ev.button == 0)
                        _collisions.Add(gridPosition);
                    else
                        _collisions.Remove(gridPosition);

                    EditorUtility.SetDirty(gameObject);

                    GUIUtility.hotControl = controlID;
                    ev.Use();
                }

                break;

            case EventType.MouseDrag:
                if (ev.button == 0 || ev.button == 1)
                {
                    if (ev.button == 0)
                        _collisions.Add(gridPosition);
                    else
                        _collisions.Remove(gridPosition);

                    EditorUtility.SetDirty(gameObject);

                    ev.Use();
                }

                break;

            case EventType.MouseUp:
                if (GUIUtility.hotControl == controlID && (ev.button == 0 || ev.button == 1))
                {
                    GUIUtility.hotControl = 0;
                    ev.Use();
                }

                break;
        }

        EditorApplication.QueuePlayerLoopUpdate();
    }

    public bool Contains(Vector2Int position) => _collisions.Contains(position);

    public override void Apply()
    {
        // Lazy init
        _overrideTile ??= new WorldCellTile(Config, Vector3.one);

        Debug.Assert(_collisions.Count > 0);

        foreach (var pos in _collisions)
        {
            var tile = Application.isPlaying ? WorldGrid.Instance[pos] : Editor.WorldGridArray[pos.x, pos.y];
            tile.OverrideTile(SortingLayerId, _overrideTile);
        }

        this.Show();
    }

    public override void Revert()
    {
        foreach (var pos in _collisions)
        {
            var tile = Application.isPlaying ? WorldGrid.Instance[pos] : Editor.WorldGridArray[pos.x, pos.y];
            tile.ClearOverride(SortingLayerId);
        }

        this.Hide();
    }
}
