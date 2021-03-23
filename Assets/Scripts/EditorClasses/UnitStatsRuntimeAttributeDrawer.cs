 #if UNITY_EDITOR

using System.Collections.Generic;
using System.Reflection;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

public class UnitStatsRuntimeAttributeDrawer : OdinAttributeDrawer<UnitStatsAttribute, Dictionary<UnitStat, Stat>>
{
    private readonly Dictionary<UnitStat, bool> _foldoutStates = new Dictionary<UnitStat, bool>();

    protected override void DrawPropertyLayout(GUIContent label)
    {
        var dict = ValueEntry.SmartValue;
        foreach (var key in dict.Keys)
        {
            if (!_foldoutStates.ContainsKey(key))
            {
                _foldoutStates[key] = false;
            }

            _foldoutStates[key] = SirenixEditorGUI.Foldout(_foldoutStates[key], key.ToString());
            if (_foldoutStates[key])
            {
                var stat = dict[key];
                var field = typeof(Stat).GetField("_effects", BindingFlags.NonPublic | BindingFlags.Instance);
                var effects = field.GetValue(stat) as List<IEffect>;

                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.FloatField("Raw Value", stat.RawValue);
                EditorGUILayout.FloatField("Final Value", stat.Value);
                EditorGUI.EndDisabledGroup();

                if (effects.Count > 0)
                {
                    EditorGUI.indentLevel += 1;
                    var value = stat.RawValue;
                    for (var i = 0; i < effects.Count; i++)
                    {
                        value = effects[i].Apply(stat, value);
                        EditorGUILayout.LabelField(effects[i].GetType().Name + ": " + effects[i] + " = " + value);
                    }

                    EditorGUI.indentLevel -= 1;
                }
            }
        }
    }
}

#endif