
#if UNITY_EDITOR

using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using Newtonsoft.Json;

public class WeaponRepository
{
    public static void Write(ScriptableWeapon weapon)
   {
       string jsonPath = $"{Application.dataPath}/ScriptableObjects/Items/_JSON/Weapons";
        
        var data = WeaponToData(weapon);

        string json = JsonConvert.SerializeObject(data, Formatting.Indented);
        
        File.WriteAllText($"{jsonPath}/{data.Filename}.json", json);
   }

    public static WeaponData WeaponToData(ScriptableWeapon weapon)
    {
        var data = new WeaponData();
        
        data.AssignItemFields(weapon);
        data.WeaponType = weapon.Type.ToString();
        data.DescriptionBroken = weapon.DescriptionBroken;
        data.MeleeSound = weapon.meleeSound;
        data.RequiredRank = weapon.RequiredRank.ToString();
        data.AttackRange.Add("Minimum", weapon.AttackRange.x);
        data.AttackRange.Add("Maximum", weapon.AttackRange.y);
        data.Weight = weapon.Weight;
        data.MaxDurability = weapon.MaxDurability;

        foreach(KeyValuePair<WeaponStat, EditorWeaponStat> entry in weapon.WeaponStats)
        {
            var statName = entry.Key.ToString();
            data.Stats.Add(statName, entry.Value.BaseValue);
            data.BrokenStats.Add(statName, entry.Value.BrokenValue);
        }

        return data;
    }

    public static ScriptableWeapon FromJSON(string weaponName)
    {
        var weapon = new ScriptableWeapon();
        string jsonPath     = $"{Application.dataPath}/ScriptableObjects/Items/_JSON/Weapons/{weaponName}.json";
        string assetPath    = $"Assets/ScriptableObjects/Items/_Generated/Weapons/{weaponName}FromJSON.asset";
        
        Debug.Log(assetPath);

        if (File.Exists(jsonPath))
        {
            string json = File.ReadAllText(jsonPath);
            var data = JsonConvert.DeserializeObject<WeaponData>(json);

            if (File.Exists(assetPath))
            {
                var existingWeapon = AssetDatabase.LoadAssetAtPath<ScriptableWeapon>(assetPath);

                if (data.IsSyncedWith(existingWeapon))
                    return existingWeapon;
                else
                    weapon = existingWeapon;
            }
            else
                AssetDatabase.CreateAsset(weapon, assetPath);

            weapon.SetFromItemData(data as ItemData);
            weapon.DescriptionBroken = data.DescriptionBroken;
            weapon.meleeSound = data.MeleeSound;
            weapon.Weight = data.Weight;
            weapon.MaxDurability = data.MaxDurability;

            weapon.Type = (WeaponType)Enum.Parse(typeof(WeaponType), data.WeaponType);
            weapon.RequiredRank = (WeaponRank)Enum.Parse(typeof(WeaponRank), data.RequiredRank);
            weapon.AttackRange = new Vector2Int(data.AttackRange["Minimum"], data.AttackRange["Maximum"]);

            var statTypes = new List<string>(data.Stats.Keys);
            var baseStats = new List<int>(data.Stats.Values);
            var brokenStats = new List<int>(data.BrokenStats.Values);

            for (int i = 0; i < statTypes.Count; i ++)
            {
                var statType = (WeaponStat) Enum.Parse(typeof(WeaponStat), statTypes[i]);

                var weaponStat = new EditorWeaponStat();
                weaponStat.BaseValue = baseStats[i];
                weaponStat.BrokenValue = brokenStats[i];

                weapon.WeaponStats.Add(statType, weaponStat);
            }

            AssetDatabase.SaveAssets();

            return AssetDatabase.LoadAssetAtPath<ScriptableWeapon>(assetPath);
        } else {
            throw new Exception($"File: {weaponName}.json | Could not be found at: #{jsonPath}");
        }        
    }
}

[Serializable]
public class WeaponData : ItemData
{
    public string WeaponType;
    public string DescriptionBroken;
    public string MeleeSound;
    public string RequiredRank;
    public Dictionary<string, int> AttackRange = new Dictionary<string, int>();
    public Dictionary<string, int> Stats = new Dictionary<string, int>();
    public Dictionary<string, int> BrokenStats = new Dictionary<string, int>();
    public int Weight;
    public int MaxDurability;

    public bool IsSyncedWith(ScriptableWeapon weapon)
    {
        var targetData = WeaponRepository.WeaponToData(weapon);

        string targetJson = JsonConvert.SerializeObject(targetData, Formatting.Indented);
        string currentJson = JsonConvert.SerializeObject(this, Formatting.Indented);

        return targetJson == currentJson;
    }
}

#endif