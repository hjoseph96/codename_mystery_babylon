using System;
using System.Collections;
using System.Collections.Generic;

using Articy.Codename_Mysterybabylon;

public enum Species
{
    Human,
    DemonHost,
    Demon,
    Seraphim
}

public class PlayableCharacter : Entity
{
    // Used To Serialize Stats & Inventory
    public UnitData UnitData        { get; protected set; }

    // Articy Template Properties
    public string Name              { get; protected set; }
    public int Age                  { get; protected set; }
    public string Appearance        { get; protected set; }
    public string BornIn            { get; protected set; }
    public string Occupation        { get; protected set; }
    public string Personality       { get; protected set; }
    public Sex Gender               { get; protected set; }
    public Species Species          { get; protected set; }
    public List<string> Skills      { get; protected set; }
    public List<string> Fears       { get; protected set; }
    public string InnerConflict     { get; protected set; }
    public string Motivation        { get; protected set; }
    public string FurtherDetails    { get; protected set; }

    public PlayableCharacter(DefaultMainCharacterTemplate template)
    {
        var basicCharacterData      = template.GetFeatureDefaultBasicCharacterFeature();
        var extendedCharacterData   = template.GetFeatureDefaultExtendedCharacterFeature();
        
        Name        = template.DisplayName;
        Age         = (int)basicCharacterData.Age;
        Appearance  = basicCharacterData.Appearance;
        BornIn      = basicCharacterData.BornIn;
        Occupation  = basicCharacterData.Occupation;
        Personality = basicCharacterData.Personality;
        Gender      = basicCharacterData.Sex;

        SetSpecies(basicCharacterData.Species);

        foreach (var fear in extendedCharacterData.Fears.Split(','))
            Fears.Add(fear.Trim(' '));

        foreach (var skill in extendedCharacterData.Skills.Split(','))
            Skills.Add(skill.Trim(' '));

        InnerConflict   = extendedCharacterData.InnerConflict;
        Motivation      = extendedCharacterData.Motivation;
        FurtherDetails  = extendedCharacterData.FurtherDetails;
    }

    private void SetSpecies(string species)
    {
        switch(species)
        {
            case "Human":
                Species = Species.Human;
                break;
            case "DemonHost":
                Species = Species.DemonHost;
                break;
            case "Demon":
                Species = Species.Demon;
                break;
            case "Seraphim":
                Species = Species.Seraphim;
                break;
            default:
                throw new Exception($"Species: {species} is unsupported...");
        }
    }

    public void UpdateUnitData(Unit unit)
    {
        var unitData = UnitData.Populate(unit);
        UnitData = unitData;
    }

}
