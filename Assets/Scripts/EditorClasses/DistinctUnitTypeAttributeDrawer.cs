using System;
using System.Linq;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

public sealed class DistinctUnitTypeAttributeDrawer : OdinAttributeDrawer<DistinctUnitTypeAttribute, UnitType>
{
    protected override void DrawPropertyLayout(GUIContent label)
    {
        var rect = EditorGUILayout.GetControlRect();
        var value = ValueEntry.SmartValue;
        var options = Enum.GetValues(typeof(UnitType))
            .Cast<UnitType>()
            .Where(val => Mathf.IsPowerOfTwo((int) val) && (int) val > 0)
            .Select(val => val.ToString())
            .ToArray();

        var selected = Array.IndexOf(options, ValueEntry.SmartValue.ToString());
        if (selected < 0)
        {
            selected = 0;
        }

        ValueEntry.SmartValue = (UnitType) Enum.Parse(typeof(UnitType), options[EditorGUI.Popup(rect, Property.Name, selected, options)]);
    }
}