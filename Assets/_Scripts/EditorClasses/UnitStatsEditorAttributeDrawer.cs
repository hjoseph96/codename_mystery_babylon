 #if UNITY_EDITOR

using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;


public class UnitStatsEditorAttributeDrawer : OdinAttributeDrawer<UnitStatsAttribute, Dictionary<UnitStat, EditorStat>>
{
    // These stats will appear in Inspector
    public static List<UnitStat> ConfigurableStats = new List<UnitStat>
    {
        UnitStat.Movement,
        UnitStat.MaxHealth,
        UnitStat.Strength,
        UnitStat.Speed,
        UnitStat.Skill,
        UnitStat.Magic,
        UnitStat.Luck,
        UnitStat.Resistance,
        UnitStat.Defense,
        UnitStat.Weight,
        UnitStat.Constitution,
        UnitStat.Morale
    };

    // These stats will have GrowthRate field in Inspector
    public static List<UnitStat> NotGrowingStats = new List<UnitStat>
    {
        UnitStat.Movement,
        UnitStat.Weight,
        UnitStat.Constitution,
        UnitStat.Morale
    };

    private readonly Dictionary<UnitStat, bool> _foldoutStates = new Dictionary<UnitStat, bool>();

    protected override void DrawPropertyLayout(GUIContent label)
    {
        EditorGUI.BeginChangeCheck();
        var dict = ValueEntry.SmartValue;
        var keys = ConfigurableStats;

        foreach (var key in keys)
        {
            dict.TryGetValue(key, out var stat);

            if (!_foldoutStates.ContainsKey(key))
            {
                _foldoutStates[key] = false;
            }

            _foldoutStates[key] = SirenixEditorGUI.Foldout(_foldoutStates[key], key.ToString());
            if (_foldoutStates[key])
            {
                EditorGUI.indentLevel += 1;
                stat.Value = EditorGUILayout.IntSlider("Value", stat.Value, 0, 99);
                if (!NotGrowingStats.Contains(key))
                    stat.GrowthRate = Mathf.Clamp(EditorGUILayout.FloatField("Growth Rate", stat.GrowthRate), 0, 1000f);
                EditorGUI.indentLevel -= 1;
            }

            dict[key] = stat;
        }
        
        if (EditorGUI.EndChangeCheck())
        {
            ValueEntry.SmartValue = new Dictionary<UnitStat, EditorStat>();
            foreach (var key in keys)
            {
                ValueEntry.SmartValue[key] = dict[key];
            }
        }
    }
}

#endif