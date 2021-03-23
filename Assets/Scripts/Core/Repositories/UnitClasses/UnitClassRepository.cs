#if UNITY_EDITOR
using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;
using Newtonsoft.Json;

public class UnitClassRepository
{

    public static void Write(ScriptableUnitClass unitClass)
    {
        string jsonPath = $"{Application.dataPath}/ScriptableObjects/Classes/_JSON";
        
        var data = UnitClassToData(unitClass);

        string json = JsonConvert.SerializeObject(data, Formatting.Indented);
        
        File.WriteAllText($"{jsonPath}/{data.Filename}.json", json);
    }

    public static UnitClassData UnitClassToData(ScriptableUnitClass unitClass)
    {
        var data = new UnitClassData();

        data.Filename = unitClass.name;
        data.Title = unitClass.Title;
        data.RelativePower = unitClass.RelativePower;
        data.PromotedBonus = unitClass.PromotedBonus;

        foreach (KeyValuePair<UnitStat, EditorStat> entry in unitClass.BaseStats)
        {
            var baseAndGrowthRate = new Dictionary<string, int>();
            baseAndGrowthRate.Add("Base", entry.Value.Value);
            baseAndGrowthRate.Add("Growth Rate", Mathf.RoundToInt(entry.Value.GrowthRate)); 
            
            data.BaseStats.Add(entry.Key.ToString(), baseAndGrowthRate);
        }

        foreach (KeyValuePair<UnitStat, EditorStat> entry in unitClass.MaxStats)
            data.MaxStats.Add(entry.Key.ToString(), entry.Value.Value);
        
        foreach (KeyValuePair<UnitStat, EditorStat> entry in unitClass.PromotionGains)
            data.PromotionGains.Add(entry.Key.ToString(), entry.Value.Value);
        
        foreach (ScriptableUnitClass promotionClass in unitClass.PromotionOptions)
            data.PromotionOptions.Add(promotionClass.name);
        
        foreach (WeaponType weaponType in unitClass.UsableWeapons)
            data.UsableWeapons.Add(weaponType.ToString());
        
        foreach (MagicType magicType in unitClass.UsableMagic)
            data.UsableMagic.Add(magicType.ToString());

        return data;
    }

    public static ScriptableUnitClass FromJSON(string unitClassName)
    {
        var unitClass = new ScriptableUnitClass();
        string jsonPath     = $"{Application.dataPath}/ScriptableObjects/Classes/_JSON/{unitClassName}.json";
        string assetPath    = $"Assets/ScriptableObjects/Classes/_Generated/{unitClassName}FromJSON.asset";
        
        Debug.Log(assetPath);

        if (File.Exists(jsonPath))
        {
            string json = File.ReadAllText(jsonPath);
            var data = JsonConvert.DeserializeObject<UnitClassData>(json);

            if (File.Exists(assetPath))
            {
                var existingUnitClass = AssetDatabase.LoadAssetAtPath<ScriptableUnitClass>(assetPath);

                if (data.IsSyncedWith(existingUnitClass))
                    return existingUnitClass;
                else
                {
                    existingUnitClass = new ScriptableUnitClass();
                    unitClass = existingUnitClass;
                }
            }
            else
                AssetDatabase.CreateAsset(unitClass, assetPath);

            

            AssetDatabase.SaveAssets();

            return AssetDatabase.LoadAssetAtPath<ScriptableUnitClass>(assetPath);
        } else {
            throw new Exception($"File: {unitClassName}.json | Could not be found at: #{jsonPath}");
        }

    }
}


[Serializable]
public class UnitClassData
{
    public string Filename;
    public string Title;
    public int RelativePower;
    public Dictionary<string, Dictionary<string, int>> BaseStats = new Dictionary<string, Dictionary<string, int>>(); 
    public Dictionary<string, int> MaxStats = new Dictionary<string, int>(); 
    public int PromotedBonus;
    public Dictionary<string, int> PromotionGains = new Dictionary<string, int>();
    public List<string> PromotionOptions = new List<string>();
    public List<string> UsableWeapons = new List<string>();
    public List<string> UsableMagic = new List<string>();


    public bool IsSyncedWith(ScriptableUnitClass unitClass)
    {
        var targetData = UnitClassRepository.UnitClassToData(unitClass);

        string targetJson = JsonConvert.SerializeObject(targetData, Formatting.Indented);
        string currentJson = JsonConvert.SerializeObject(this, Formatting.Indented);

        return targetJson == currentJson;
    }
}

#endif