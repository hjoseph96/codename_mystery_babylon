
#if UNITY_EDITOR

using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using Newtonsoft.Json;

public class GearRepository
{
    public static void Write(ScriptableGear gear)
    {
        string jsonPath = $"{Application.dataPath}/ScriptableObjects/Items/_JSON/Gear";

        var data = GearToData(gear);

        string json = JsonConvert.SerializeObject(data, Formatting.Indented);

        File.WriteAllText($"{jsonPath}/{data.Filename}.json", json);
    }

    public static GearData GearToData(ScriptableGear gear)
    {
        var data = new GearData();

        data.AssignItemFields(gear);
        data.GearType = gear.Type.ToString();
        data.DescriptionBroken = gear.DescriptionBroken;
        data.RequiredRank = gear.RequiredRank.ToString();
        data.Weight = gear.Weight;
        data.MaxDurability = gear.MaxDurability;

        foreach (KeyValuePair<GearStat, EditorGearStat> entry in gear.GearStats)
        {
            var statName = entry.Key.ToString();
            data.Stats.Add(statName, entry.Value.BaseValue);
            
        }

        return data;
    }

    public static ScriptableGear FromJSON(string gearName)
    {
        var gear = new ScriptableGear();
        string jsonPath = $"{Application.dataPath}/ScriptableObjects/Items/_JSON/Gear/{gearName}.json";
        string assetPath = $"Assets/ScriptableObjects/Items/_Generated/Gear/{gearName}FromJSON.asset";

        Debug.Log(assetPath);

        if (File.Exists(jsonPath))
        {
            string json = File.ReadAllText(jsonPath);
            var data = JsonConvert.DeserializeObject<GearData>(json);

            if (File.Exists(assetPath))
            {
                var existingGear = AssetDatabase.LoadAssetAtPath<ScriptableGear>(assetPath);

                if (data.IsSyncedWith(existingGear))
                    return existingGear;
                else
                    gear = existingGear;
            }
            else
                AssetDatabase.CreateAsset(gear, assetPath);

            gear.SetFromItemData(data as ItemData);
            gear.DescriptionBroken = data.DescriptionBroken;
            gear.Weight = data.Weight;
            gear.MaxDurability = data.MaxDurability;

            gear.Type = (GearType)Enum.Parse(typeof(GearType), data.GearType);
            gear.RequiredRank = (GearRank)Enum.Parse(typeof(GearRank), data.RequiredRank);

            var statTypes = new List<string>(data.Stats.Keys);
            var baseStats = new List<int>(data.Stats.Values);

            for (int i = 0; i < statTypes.Count; i++)
            {
                var statType = (GearStat)Enum.Parse(typeof(GearStat), statTypes[i]);

                var gearStat = new EditorGearStat();
                gearStat.BaseValue = baseStats[i];
                gearStat.BrokenValue = 0;

                gear.GearStats.Add(statType, gearStat);
            }

            AssetDatabase.SaveAssets();

            return AssetDatabase.LoadAssetAtPath<ScriptableGear>(assetPath);
        }
        else
        {
            throw new Exception($"File: {gearName}.json | Could not be found at: #{jsonPath}");
        }
    }
}

[Serializable]
public class GearData : ItemData
{
    public string GearType;
    public string DescriptionBroken;
    public string RequiredRank;
    public Dictionary<string, int> Stats = new Dictionary<string, int>();
    public int Weight;
    public int MaxDurability;

    public bool IsSyncedWith(ScriptableGear gear)
    {
        var targetData = GearRepository.GearToData(gear);

        string targetJson = JsonConvert.SerializeObject(targetData, Formatting.Indented);
        string currentJson = JsonConvert.SerializeObject(this, Formatting.Indented);

        return targetJson == currentJson;
    }
}

#endif