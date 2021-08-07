using System.Linq;
using System.Collections.Generic;
using UnityEngine;

using Sirenix.OdinInspector;

public class Barrier : SerializedMonoBehaviour
{
    public SpriteRenderer _renderer;
    public List<Collider2D> barriers = new List<Collider2D>();
    public List<WorldCell> affectedTiles = new List<WorldCell>();
    public List<WorldCell> targetedTiles = new List<WorldCell>();

    public TileConfiguration _nullTile;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        _renderer = GetComponent<SpriteRenderer>();

        _nullTile = Resources.Load<TileConfiguration>("NullTile");

        PopulateColliders();
    }

    public void Scan()
    {
        var worldGrid = WorldGrid.Instance;

        targetedTiles = new List<WorldCell>();

        foreach (var barrier in barriers)
        {
            var barrierBounds = barrier.bounds;

            // Checking every tiles in WorldGrid
            for (var j = 0; j < worldGrid.Height; j++)
            {
                for (var i = 0; i < worldGrid.Width; i++)
                {
                    var cellGridPosition = new Vector2Int(i, j);
                    var worldCellCenter = worldGrid.Grid.GetCellCenterWorld((Vector3Int)cellGridPosition);

                    // If the barrier is covering the center of this tile
                    if (barrierBounds.Contains(worldCellCenter))
                    {
                        var worldCell = worldGrid[i, j];

                        if (!targetedTiles.Contains(worldCell))
                            targetedTiles.Add(worldCell);

                        // Override Tile
                        var overrideTile = new WorldCellTile(_nullTile, Vector3.one);

                        if (_renderer != null)
                            worldCell.OverrideTile(_renderer.sortingLayerID, overrideTile);
                        else
                            worldCell.OverrideTile(0, overrideTile);
                    }
                }
            }
        }

        foreach(var noLongerAffectedTile in affectedTiles.ToArray().Except(targetedTiles).ToArray())
        {
            if (_renderer != null)
                noLongerAffectedTile.ClearOverride(_renderer.sortingLayerID);
            else
                noLongerAffectedTile.ClearOverride(0);

            affectedTiles.Remove(noLongerAffectedTile);
        }

    }

    protected virtual void PopulateColliders()
    {
        foreach (var col in GetComponents<Collider2D>())
            if (!col.isTrigger && !barriers.Contains(col))
                barriers.Add(col);

        foreach (var col in GetComponentsInChildren<Collider2D>())
            if (!col.isTrigger && !barriers.Contains(col))
                barriers.Add(col);
    }

    private void OnDrawGizmos()
    {
        if (Application.IsPlaying(this))
            foreach(var cell in targetedTiles)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawCube(WorldGrid.Instance.Grid.GetCellCenterWorld((Vector3Int)cell.Position), Vector3.one);
            }
    }
}
