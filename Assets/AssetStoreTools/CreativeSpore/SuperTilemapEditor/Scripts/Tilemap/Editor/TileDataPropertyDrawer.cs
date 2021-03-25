using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections;
using System.Reflection;

namespace CreativeSpore.SuperTilemapEditor
{
    /// <summary>
    /// Displays a preview of a tile using the int property as tile data and finding the tileset in a parent STETilemap component
    /// </summary>
    [CustomPropertyDrawer(typeof(TileDataAttribute))]
    public class TileDataPropertyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            TileDataAttribute tileDataAttrib = attribute as TileDataAttribute;
            return EditorGUIUtility.singleLineHeight 
                + (tileDataAttrib.displayProperties && property.isExpanded ? Mathf.Max(tileDataAttrib.tileSize, 114f) : tileDataAttrib.tileSize + 4f);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            TileDataAttribute tileDataAttrib = attribute as TileDataAttribute;
            if (property.propertyType != SerializedPropertyType.Integer)
            {
                Debug.LogError("SortedLayer property should be an integer");
            }
            else
            {
                Tileset tileset = null;
                if (property.serializedObject.targetObject is MonoBehaviour)
                {
                    STETilemap parentTilemap = (property.serializedObject.targetObject as MonoBehaviour).GetComponentInParent<STETilemap>();
                    if (parentTilemap) tileset = parentTilemap.Tileset;
                }
                else if (property.serializedObject.targetObject is TilesetBrush)
                {
                    tileset = (property.serializedObject.targetObject as TilesetBrush).Tileset;
                }                
                Rect rIntLabel = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
                Rect rContent = new Rect(position.x, position.y + rIntLabel.height + 2f, position.width, position.height - rIntLabel.height);
                EditorGUI.PropertyField(rIntLabel, property, label);
                if (tileset)
                {
                    Rect rVisualTile = EditorGUI.PrefixLabel(rContent, new GUIContent("Preview"));
                    rVisualTile.width = rVisualTile.height = tileDataAttrib.tileSize;
                    TileDataField(rVisualTile, label, property, tileset);

                    if (tileDataAttrib.displayProperties)
                    {
                        Rect rTileProperties = new Rect(rVisualTile.xMax + 4f, rContent.y, rContent.width - rVisualTile.xMax - 4f, EditorGUIUtility.singleLineHeight);
                        property.isExpanded = EditorGUI.Foldout(rTileProperties, property.isExpanded, "Tile Properties");
                        if (property.isExpanded)
                        {
                            rTileProperties.y += rTileProperties.height + 2f;
                            rTileProperties.height = 90f;
                            property.intValue = (int)TileGridControl.DoTileDataProperties(rTileProperties, (uint)property.intValue, tileset);
                        }
                    }                    
                }
                else
                {
                    EditorGUI.HelpBox(rContent, "Tileset was not found!", MessageType.Warning);
                }
            }
        }

        public static void TileDataField(Rect position, GUIContent label, SerializedProperty property, Tileset tileset)
        {
            Event e = Event.current;
            bool isLeftMouseReleased = e.type == EventType.MouseUp && e.button == 0;
            //NOTE: there is a bug with DrawTextureWithTexCoords where the texture disappears. It is fixed by overriding the Editor Script with a CustomEditor.
            uint tileData = (uint)property.intValue;
            TilesetBrush brush = tileset.FindBrush(Tileset.GetBrushIdFromTileData(tileData));
            if (brush)
                tileData = brush.PreviewTileData();

            TilesetEditor.DoGUIDrawTileFromTileData(position, tileData, tileset);
            if (isLeftMouseReleased && position.Contains(e.mousePosition))
            {
                EditorWindow wnd = EditorWindow.focusedWindow;
                TileSelectionWindow.Show(tileset, property);
                TileSelectionWindow.Instance.Ping();
                wnd.Focus();
                GUI.FocusControl("");
            }            
        }  
    }
}
