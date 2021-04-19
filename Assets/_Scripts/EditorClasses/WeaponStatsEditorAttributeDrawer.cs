
#if UNITY_EDITOR
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

public class WeaponStatsEditorAttributeDrawer : OdinAttributeDrawer<WeaponStatsAttribute, Dictionary<WeaponStat, EditorWeaponStat>>
{
    // These stats will appear in Inspector
    public static List<WeaponStat> ConfigurableStats = new List<WeaponStat>
    {
        WeaponStat.Damage,
        WeaponStat.Hit,
        WeaponStat.CriticalHit
    };

    // These stats will have GrowthRate field in Inspector
    public static List<WeaponStat> ReducibleStats = new List<WeaponStat>
    {
        WeaponStat.Damage,
        WeaponStat.Hit
    };

    private readonly Dictionary<WeaponStat, bool> _foldoutStates = new Dictionary<WeaponStat, bool>();

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
                if (ReducibleStats.Contains(key))
                    stat.BrokenValue = EditorGUILayout.IntSlider("Broken Value", stat.BrokenValue, 0, 99);
                else
                    stat.BrokenValue = stat.BaseValue;
                EditorGUI.indentLevel -= 1;
            }

            dict[key] = stat;
        }

        if (EditorGUI.EndChangeCheck())
        {
            ValueEntry.SmartValue = new Dictionary<WeaponStat, EditorWeaponStat>();
            foreach (var key in keys)
                ValueEntry.SmartValue[key] = dict[key];
        }
    }
}

#endif
