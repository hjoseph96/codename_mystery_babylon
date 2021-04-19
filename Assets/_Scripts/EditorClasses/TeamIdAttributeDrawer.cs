#if UNITY_EDITOR


using System;
using System.Linq;
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

public sealed class TeamIdAttributeDrawer : OdinAttributeDrawer<TeamIdAttribute, int>
{
    private Dictionary<string, int> _teams = new Dictionary<string, int> {
        { "Player",  0 },
        { "Ally", 1 },
        { "Enemy", 2 },
        { "OtherEnemy", 3 },
        { "Neutral", 4 }
};

    protected override void DrawPropertyLayout(GUIContent label)
    {
        var rect = EditorGUILayout.GetControlRect();
        var options = _teams.Keys.ToArray();

        ValueEntry.SmartValue = EditorGUI.Popup(rect, Property.Name, ValueEntry.SmartValue, options);
    }
}

#endif