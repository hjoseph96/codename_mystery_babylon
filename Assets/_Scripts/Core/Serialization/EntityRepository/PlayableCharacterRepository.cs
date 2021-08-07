using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;



public class PlayableCharacterRepository
{
    public static void Write(PlayableCharacter playableCharacter)
    {
        var parentFolder = $"{Application.dataPath}/SerializedData/Entities/Playable Characters/";
        string jsonPath = $"{parentFolder}/{playableCharacter.Name}";

        if (!Directory.Exists(jsonPath))
            Directory.CreateDirectory(jsonPath);

        var playerCharacterData = PlayableCharacterData.Populate(playableCharacter);

        string json = JsonConvert.SerializeObject(playerCharacterData, Formatting.Indented);

        File.WriteAllText($"{jsonPath}/{playableCharacter.Name}.json", json);
    }
}

[Serializable]
public class PlayableCharacterData
{

    public string Name              { get; protected set; }
    public int Age                  { get; protected set; }
    public string Appearance        { get; protected set; }
    public string BornIn            { get; protected set; }
    public string Occupation        { get; protected set; }
    public string Personality       { get; protected set; }
    public string Gender            { get; protected set; }
    public string Species           { get; protected set; }
    public List<string> Skills      { get; protected set; }
    public List<string> Fears       { get; protected set; }
    public string InnerConflict     { get; protected set; }
    public string Motivation        { get; protected set; }
    public string FurtherDetails    { get; protected set; }
    public UnitData UnitData { get; protected set; }

    public static PlayableCharacterData Populate(PlayableCharacter playableCharacter)
    {
        var playableCharacterData = new PlayableCharacterData();

        playableCharacterData.UnitData      = playableCharacter.UnitData;

        playableCharacterData.Name          = playableCharacter.Name;
        playableCharacterData.Age           = playableCharacter.Age;
        playableCharacterData.Appearance    = playableCharacter.Appearance;
        playableCharacterData.BornIn        = playableCharacter.BornIn;
        playableCharacterData.Occupation    = playableCharacter.Occupation;
        playableCharacterData.Personality   = playableCharacter.Personality;
        playableCharacterData.Species       = playableCharacter.Species.ToString();
        playableCharacterData.Skills        = playableCharacter.Skills;
        playableCharacterData.Fears         = playableCharacter.Fears;
        playableCharacterData.InnerConflict = playableCharacter.InnerConflict;
        playableCharacterData.Motivation    = playableCharacter.Motivation;
        playableCharacterData.FurtherDetails = playableCharacterData.FurtherDetails;

        return playableCharacterData;
    }
}