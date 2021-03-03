using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
public class TileConfigurationRepository
{

    public static void Write(TileConfiguration tileConfig)
    {
        
        string jsonPath = $"{Application.dataPath}/ScriptableObjects/TileConfigurations/_JSON";
        
        var data = new TileConfigurationData();

        data.TerrainName = tileConfig.TerrainName;
        data.HasLightOfSight = tileConfig.HasLineOfSight;
        data.IsStairs = tileConfig.IsStairs;

        data.UnitType = tileConfig.UnitType.ToString();
        data.SurfaceType = tileConfig.SurfaceType.ToString();
        data.StairsOrientation = tileConfig.StairsOrientation.ToString();

        foreach(KeyValuePair<UnitType, int> entry in tileConfig.TravelCost)
            data.TravelCost.Add(entry.Key.ToString(), entry.Value);
        
        foreach(KeyValuePair<Direction, UnitType> entry in tileConfig.BlockEntrance)
            data.BlockEntrance.Add(entry.Key.ToString(), entry.Value.ToString());

        foreach(KeyValuePair<Direction, UnitType> entry in tileConfig.BlockExit)
            data.BlockExit.Add(entry.Key.ToString(), entry.Value.ToString());

        string json = JsonConvert.SerializeObject(data, Formatting.Indented);
        

        File.WriteAllText($"{jsonPath}/{tileConfig.name}.json", json);
    }

    // public TileConfiguration FromJSON(string tileConfigName)
    // {

    // }
}


[Serializable]
class TileConfigurationData
{
    public string TerrainName;
    public string SurfaceType;
    public Dictionary<string, int> TravelCost = new Dictionary<string, int>();
    public bool HasLightOfSight;
    public bool IsStairs;
    public string StairsOrientation;
    public Dictionary<string, string> BlockExit = new Dictionary<string, string>();
    public Dictionary<string, string> BlockEntrance = new Dictionary<string, string>();
    public string UnitType;
}