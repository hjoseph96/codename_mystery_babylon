using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;


[ExecuteInEditMode]
[RequireComponent(typeof(Grid))]
[RequireComponent(typeof(Tilemap))]
[RequireComponent(typeof(TilemapRenderer))]
public class ColliderGroup : SerializedMonoBehaviour
{
    private int _sortingLayerId;
    [NonSerialized] private Dictionary<Vector2Int, WorldCellTile> _collisions;

    private WorldGridEditor Editor
    {
        get
        {
            if (_editor == null)
                _editor = FindObjectOfType<WorldGridEditor>();

            return _editor;
        }
    }

    private WorldGridEditor _editor;
    private readonly List<Collider2D> _overlapBoxResults = new List<Collider2D>();

    private void Update()
    {
        GetComponent<TilemapRenderer>().enabled = Selection.activeGameObject == gameObject;
    }

    [Button(ButtonSizes.Large), GUIColor(0, 1, 0)]
    private void ClearAll()
    {
        GetComponent<Tilemap>().ClearAllTiles();
    }

    public void UpdateCollisions(TileBase tile)
    {
        if (tile == null)
        {
            Debug.LogError("Tile was not selected!");
            return;
        }

        var tilemap = GetComponent<Tilemap>();
        tilemap.color = new Color(1f, 0, 0, 0.65f);
        tilemap.ClearAllTiles();

        var filter = new ContactFilter2D
        {
            useLayerMask = true, 
            layerMask = LayerMask.GetMask("Tilemap Colliders")
        };

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
                    var pos = new Vector3Int(x, y, 0);
                    var worldPos = Editor.Grid.GetCellCenterWorld(pos);
                    //Debug.Log(tilemap.WorldToCell(worldPos));

                    _overlapBoxResults.Clear();
                    if (Physics2D.OverlapBox(worldPos, Vector2.one, 0,
                        filter, _overlapBoxResults) > 0 && _overlapBoxResults.IndexOf(currentCollider) >= 0)
                        tilemap.SetTile(tilemap.WorldToCell(worldPos), tile);
                }
            }
        }

        EditorUtility.SetDirty(gameObject);
    }

    public void Apply()
    {
        LazyInit();

        foreach (var pos in _collisions.Keys)
        {
            var tile = Application.isPlaying ? WorldGrid.Instance[pos] : Editor.WorldGridArray[pos.x, pos.y];
            tile.OverrideTile(_sortingLayerId, _collisions[pos]);
        }

        this.Show();
    }

    public void Revert()
    {
        LazyInit();

        foreach (var pos in _collisions.Keys)
        {
            var tile = Application.isPlaying ? WorldGrid.Instance[pos] : Editor.WorldGridArray[pos.x, pos.y];
            tile.ClearOverride(_sortingLayerId);
        }

        this.Hide();
    }

    private void LazyInit()
    {
        if (_collisions != null)
            return;

        _collisions = new Dictionary<Vector2Int, WorldCellTile>();
        var tilemap = GetComponent<Tilemap>();
        tilemap.CompressBounds();
        foreach (var pos in tilemap.cellBounds.allPositionsWithin)
        {
            if (tilemap.HasTile(pos))
            {
                var tile = tilemap.GetTile<NavigationTile>(pos);
                var scale = tilemap.GetTransformMatrix(pos).lossyScale;
                var worldPos = tilemap.GetCellCenterWorld(pos);
                var gridPos = WorldGrid.Instance.Grid.WorldToCell(worldPos);
                _collisions.Add((Vector2Int) gridPos, new WorldCellTile(tile.Config, scale));
            }
        }

        var tilemapRenderer = GetComponent<TilemapRenderer>();
        _sortingLayerId = tilemapRenderer.sortingLayerID;
        tilemapRenderer.enabled = false;
    }
}
