
#if UNITY_EDITOR
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

public class GearStatsEditorAttributeDrawer : OdinAttributeDrawer<WeaponStatsAttribute, Dictionary<GearStat, EditorGearStat>>
{
    // These stats will appear in Inspector
    public static List<GearStat> ConfigurableStats = new List<GearStat>
    {
        GearStat.Defense,
        GearStat.Resistance,
    };


    private readonly Dictionary<GearStat, bool> _foldoutStates = new Dictionary<GearStat, bool>();

    protected override void DrawPropertyLayout(GUIContent label)
    {
        EditorGUI.BeginChangeCheck();
        var dict = ValueEntry.SmartValue;
        var keys = ConfigurableStats;

        foreach (var key in keys)
        {
            dict.TryGetValue(key, out var stat);

            if (!_foldoutStates.ContainsKey(key))
                _foldoutStates[key] = false;

            _foldoutStates[key] = SirenixEditorGUI.Foldout(_foldoutStates[key], key.ToString());
            if (_foldoutStates[key])
            {
                EditorGUI.indentLevel += 1;
                stat.BaseValue = EditorGUILayout.IntSlider("Value", stat.BaseValue, 0, 99);
                stat.BrokenValue = 0;
                EditorGUI.indentLevel -= 1;
            }

            dict[key] = stat;
        }

        if (EditorGUI.EndChangeCheck())
        {
            ValueEntry.SmartValue = new Dictionary<GearStat, EditorGearStat>();
            foreach (var key in keys)
                ValueEntry.SmartValue[key] = dict[key];
        }
    }
}

#endif
