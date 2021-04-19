using UnityEditor;
using UnityEngine;


[CustomPropertyDrawer(typeof(SortingLayerAttribute))]
public class SortingLayerAttributeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var sortingLayerNames = new string[SortingLayer.layers.Length];
        for (var i = 0; i < SortingLayer.layers.Length; i++)
            sortingLayerNames[i] = SortingLayer.layers[i].name;


        EditorGUI.BeginProperty(position, label, property);

        var oldName = property.stringValue;
        var oldLayerIndex = -1;
        for (var i = 0; i < sortingLayerNames.Length; i++)
        {
            if (sortingLayerNames[i].Equals(oldName))
                oldLayerIndex = i;
        }

        var newLayerIndex = EditorGUI.Popup(position, label.text, oldLayerIndex, sortingLayerNames);
        if (newLayerIndex != oldLayerIndex)
            property.stringValue = sortingLayerNames[newLayerIndex];

        EditorGUI.EndProperty();
    }
}