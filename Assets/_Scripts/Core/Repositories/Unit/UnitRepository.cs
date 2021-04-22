
using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

using Newtonsoft.Json;

#if UNITY_EDITOR
public class UnitRepository
{
    public static void Write(Unit unit)
    {
        var parentFolder = $"{Application.dataPath}/SerializedData/Entities/Playable Characters/";
        string jsonPath = $"{parentFolder}/{unit.Name}";

        if (!AssetDatabase.IsValidFolder(jsonPath))
            AssetDatabase.CreateFolder(parentFolder, unit.Name);

        var unitData = UnitData.Populate(unit);

        string json = JsonConvert.SerializeObject(unitData, Formatting.Indented);

        File.WriteAllText($"{jsonPath}/UnitData.json", json);
    }
}
#endif


[Serializable]
public class UnitData
{
    public string UnitType;
    public float WalkSpeed;
    public float RunSpeed;
    public float MoveAnimationSpeed;

    // TODO: Find out if there's a cleaner way to do nested JSON in C#...
    public Dictionary<string, Dictionary<string,int>> Stats = new Dictionary<string, Dictionary<string, int>>();
    public Dictionary<string, string> WeaponProfiency = new Dictionary<string, string>();
    public Dictionary<string, string> MagicProfiency = new Dictionary<string, string>();
    
    public string UnitPrefabPath;

    public static UnitData Populate(Unit unit)
    {
        var unitData = new UnitData();

        unitData.UnitType           = unit.UnitType.ToString();
        unitData.WalkSpeed          = unit.WalkSpeed;
        unitData.RunSpeed           = unit.RunSpeed;
        unitData.MoveAnimationSpeed = unit.MoveAnimationSpeed;

        if (unit.IsPlaying)
        {
            foreach(var entry in unit.Stats)
            {
                var baseStatAndGrowthRate = new Dictionary<string, int>();
                baseStatAndGrowthRate.Add("Base Value", entry.Value.RawValueInt);
                baseStatAndGrowthRate.Add("Growth Rate", (int)entry.Value.GrowthRate);

                unitData.Stats.Add(entry.Key.ToString(), baseStatAndGrowthRate);
            }
        }
        else
        {
            foreach (var entry in unit.EditorStats)
            {
                var baseStatAndGrowthRate = new Dictionary<string, int>();
                baseStatAndGrowthRate.Add("Base Value", entry.Value.Value);
                baseStatAndGrowthRate.Add("Growth Rate", (int)entry.Value.GrowthRate);

                unitData.Stats.Add(entry.Key.ToString(), baseStatAndGrowthRate);
            }
        }

        if (unit.WeaponProfiency.Count > 0)
        {
            foreach (var entry in unit.WeaponProfiency)
                unitData.WeaponProfiency.Add(entry.Key.ToString(), entry.Value.ToString());
        }
        else
        {
            foreach (var weaponType in unit.UnitClass.UsableWeapons)
                unitData.WeaponProfiency.Add(weaponType.ToString(), WeaponRank.D.ToString());
        }

        if (unit.MagicProfiency.Count > 0)
        {
            foreach (var entry in unit.MagicProfiency)
                unitData.MagicProfiency.Add(entry.Key.ToString(), entry.Value.ToString());
        }
        else
        {
            foreach (var weaponType in unit.UnitClass.UsableWeapons)
                unitData.MagicProfiency.Add(weaponType.ToString(), WeaponRank.D.ToString());
        }

        return unitData;
    }
}
