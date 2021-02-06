using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

public enum UnitStat
{
    MaxHealth,
    Movement,
    Strength,
    Speed,
    Skill,
    Magic,
    Luck,
    Resistance,
    Defense
}

[CreateAssetMenu(fileName = "NewUnitStats", menuName = "ScriptableObjects/UnitStats", order = 2)]
public class UnitStats : SerializedScriptableObject
{
    [Header("Base Stats")]
    [UnitStats]
    public Dictionary<UnitStat, int> StatsDictionary;
}


[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class UnitStatsAttribute : Attribute
{ }

public class UnitStatsAttributeDrawer : OdinAttributeDrawer<UnitStatsAttribute, Dictionary<UnitStat, int>>
{
    protected override void DrawPropertyLayout(GUIContent label)
    {
        //CallNextDrawer(label);

        //var rect = EditorGUILayout.GetControlRect();
        var dict = ValueEntry.SmartValue;
        //dict.Clear();

        var names = Enum.GetNames(typeof(UnitStat));
        //Debug.Log(names.Length);
        foreach (var name in names)
        {
            Enum.TryParse<UnitStat>(name, out var key);
            dict.TryGetValue(key, out var oldValue);
            dict[key] = EditorGUILayout.IntField(name, oldValue);
        }
    }
}
