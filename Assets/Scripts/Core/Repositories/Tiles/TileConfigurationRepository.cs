using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using Newtonsoft.Json;

public class TileConfigurationRepository
{

    public static void Write(TileConfiguration tileConfig)
    {
        string jsonPath = $"{Application.dataPath}/ScriptableObjects/TileConfigurations/_JSON";
        
        var data = TileConfigToData(tileConfig);

        string json = JsonConvert.SerializeObject(data, Formatting.Indented);
        
        File.WriteAllText($"{jsonPath}/{data.Filename}.json", json);
    }

    public static TileConfigurationData TileConfigToData(TileConfiguration tileConfig)
    {
        var data = new TileConfigurationData();

        data.Filename = tileConfig.name;
        data.TerrainName = tileConfig.TerrainName;
        data.HasLightOfSight = tileConfig.HasLineOfSight;
        data.IsStairs = tileConfig.IsStairs;

        data.SurfaceType = tileConfig.SurfaceType.ToString();
        data.StairsOrientation = tileConfig.StairsOrientation.ToString();

        foreach(KeyValuePair<UnitType, int> entry in tileConfig.TravelCost)
            data.TravelCost.Add(entry.Key.ToString(), entry.Value);
        
        foreach(KeyValuePair<Direction, UnitType> entry in tileConfig.BlockEntrance)
            data.BlockEntrance.Add(entry.Key.ToString(), entry.Value.ToString());

        foreach(KeyValuePair<Direction, UnitType> entry in tileConfig.BlockExit)
            data.BlockExit.Add(entry.Key.ToString(), entry.Value.ToString());
        
        return data;
    }

    public static TileConfiguration FromJSON(string tileConfigName)
    {
        var tileConfig = new TileConfiguration();
        string jsonPath     = $"{Application.dataPath}/ScriptableObjects/TileConfigurations/_JSON/{tileConfigName}.json";
        string assetPath    = $"Assets/ScriptableObjects/TileConfigurations/_Generated/{tileConfigName}FromJSON.asset";
        
        Debug.Log(assetPath);

        if (File.Exists(jsonPath))
        {
            string json = File.ReadAllText(jsonPath);
            var data = JsonConvert.DeserializeObject<TileConfigurationData>(json);

            if (File.Exists(assetPath))
            {
                var existingtileConfig = AssetDatabase.LoadAssetAtPath<TileConfiguration>(assetPath);

                if (data.IsSyncedWith(existingtileConfig))
                    return existingtileConfig;
                else
                    tileConfig = existingtileConfig;
            }
            else
                AssetDatabase.CreateAsset(tileConfig, assetPath);

            tileConfig.TerrainName      = data.TerrainName;
            tileConfig.HasLineOfSight   = data.HasLightOfSight;
            tileConfig.IsStairs         = data.IsStairs;
            tileConfig.SurfaceType = (SurfaceType)Enum.Parse(typeof(SurfaceType), data.SurfaceType);
            tileConfig.StairsOrientation = (StairsOrientation)Enum.Parse(typeof(StairsOrientation), data.StairsOrientation);

            foreach(KeyValuePair<string, int> entry in data.TravelCost)
            {
                var unitType = (UnitType)Enum.Parse(typeof(UnitType), entry.Key);
                
                tileConfig.TravelCost.Add(unitType, entry.Value);
            }

            foreach(KeyValuePair<string, string> entry in data.BlockExit)
            {
                var direction   = (Direction)Enum.Parse(typeof(Direction), entry.Key);
                var unitType    = (UnitType)Enum.Parse(typeof(UnitType), entry.Value);
                
                tileConfig.BlockExit.Add(direction, unitType);
            }

            foreach(KeyValuePair<string, string> entry in data.BlockEntrance)
            {
                var direction   = (Direction)Enum.Parse(typeof(Direction), entry.Key);
                var unitType    = (UnitType)Enum.Parse(typeof(UnitType), entry.Value);
                
                tileConfig.BlockEntrance.Add(direction, unitType);
            }

            AssetDatabase.SaveAssets();

            return AssetDatabase.LoadAssetAtPath<TileConfiguration>(assetPath);
        } else {
            throw new Exception($"File: {tileConfigName}.json | Could not be found at: #{jsonPath}");
        }

    }
}


[Serializable]
public class TileConfigurationData
{
    public string Filename;
    public string TerrainName;
    public string SurfaceType;
    public Dictionary<string, int> TravelCost = new Dictionary<string, int>();
    public bool HasLightOfSight;
    public bool IsStairs;
    public string StairsOrientation;
    public Dictionary<string, string> BlockExit = new Dictionary<string, string>();
    public Dictionary<string, string> BlockEntrance = new Dictionary<string, string>();


    public bool IsSyncedWith(TileConfiguration tileConfig)
    {
        var targetData = TileConfigurationRepository.TileConfigToData(tileConfig);

        string targetJson = JsonConvert.SerializeObject(targetData, Formatting.Indented);
        string currentJson = JsonConvert.SerializeObject(this, Formatting.Indented);


        return targetJson == currentJson;
    }
}
