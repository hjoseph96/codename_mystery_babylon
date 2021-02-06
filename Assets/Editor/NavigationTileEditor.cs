using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(NavigationTile))]
[CanEditMultipleObjects]
public class NavigationTileEditor : Editor
{
    private SerializedProperty _configProperty;

    private NavigationTile _tile;

    private void OnEnable()
    {
        _tile = target as NavigationTile;
        _configProperty = serializedObject.FindProperty(nameof(_tile.Config));
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.BeginHorizontal();
        _tile.sprite = (Sprite) EditorGUILayout.ObjectField("Sprite", _tile.sprite, typeof(Sprite), true);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.PropertyField(_configProperty);

        if (_tile.Config == null && GUILayout.Button("Create New Tile Configuration"))
        {
            string path =
                UnityEditor.EditorUtility.SaveFilePanelInProject("Choose path", "NewTileConfiguration.asset",
                    "asset",
                    "Please enter a file name");

            if (path.Length != 0)
            {
                TileConfiguration asset = CreateInstance<TileConfiguration>();
                AssetDatabase.CreateAsset(asset, path);
                _tile.Config = asset;
                Repaint();
            }
        }

        if (EditorGUI.EndChangeCheck())
        {
            UnityEditor.EditorUtility.SetDirty(_tile);
        }

        serializedObject.ApplyModifiedProperties();
    }
}