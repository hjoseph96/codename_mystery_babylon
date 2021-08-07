using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Sirenix.OdinInspector;
using Sirenix.Serialization;

public class GridPathComponent : SerializedMonoBehaviour
{
    public List<Vector2Int> Path;
    public GridPath GridPath { get; private set; }

    private void Start()
    {
        GridPath = new GridPath(Path, 0);
    }

    private void OnDrawGizmos()
    {
        if (Application.IsPlaying(this))
            foreach(var cell in Path)
                Gizmos.DrawSphere(WorldGrid.Instance.Grid.GetCellCenterWorld((Vector3Int)cell), 0.5f);
        else
        {
            var worldGridEditor = FindObjectOfType<WorldGridEditor>();
            foreach (var pos in Path)
            {
                Gizmos.DrawSphere(worldGridEditor.Grid.GetCellCenterWorld((Vector3Int)(pos + worldGridEditor.Origin)), 0.5f);
            }
        }
    }
}
