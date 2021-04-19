using Animancer.Editor;
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
            var path = 
                EditorUtility.SaveFilePanelInProject("Choose path", "NewTileConfiguration.asset",
                    "asset",
                    "Please enter a file name");

            if (path.Length != 0)
            {
                var asset = CreateInstance<TileConfiguration>();
                AssetDatabase.CreateAsset(asset, path);
                _configProperty.SetValue(this, asset);
                EditorUtility.SetDirty(_tile);
            }
        }

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(_tile);
        }

        serializedObject.ApplyModifiedProperties();
    }
}