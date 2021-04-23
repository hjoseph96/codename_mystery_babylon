using UnityEditor;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.Tilemaps;


[CustomGridBrush(true, false, false, "Advanced Grid Brush")]
public class AdvancedGridBrush : GridBrush
{
    [HideInInspector] public TileBase SelectedTile;

    public override void Pick(GridLayout gridLayout, GameObject brushTarget, BoundsInt position, Vector3Int pickStart)
    {
        base.Pick(gridLayout, brushTarget, position, pickStart);
        SelectedTile = cellCount == 1 ? cells[0].tile : null;
    }
}

[CustomEditor(typeof(AdvancedGridBrush))]
public class AdvancedGridBrushEditor : GridBrushEditor
{
    private AdvancedGridBrush _brush;

    protected override void OnEnable()
    {
        base.OnEnable();

        _brush = target as AdvancedGridBrush;
    }
    public override void OnPaintInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.BeginHorizontal();
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.ObjectField("Selected Tile", _brush.SelectedTile, typeof(TileBase), false);
        EditorGUI.EndDisabledGroup();

        if (GUILayout.Button("Open"))
        {
            AssetDatabase.OpenAsset(_brush.SelectedTile);
            EditorGUIUtility.PingObject(_brush.SelectedTile);
        }

        if (GUILayout.Button("Update Collisions"))
        {
            var colliderGroup = GridPaintingState.scenePaintTarget.GetComponent<ColliderGroupTilemap>();
            colliderGroup.UpdateCollisions(_brush.SelectedTile);
        }

        EditorGUILayout.EndHorizontal();
    }
}