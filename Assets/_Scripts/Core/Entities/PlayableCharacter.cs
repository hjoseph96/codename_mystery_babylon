using System;
using System.Collections;
using System.Collections.Generic;

using Sirenix.OdinInspector;
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
    [FoldoutGroup("Game Data"), ShowInInspector]
    public UnitData UnitData         { get; protected set; }


    [FoldoutGroup("Extended Details"), ShowInInspector]
    public List<string> Skills      { get; protected set; }

    [FoldoutGroup("Extended Details"), ShowInInspector]
    public List<string> Fears       { get; protected set; }

    [FoldoutGroup("Extended Details"), ShowInInspector]
    public string InnerConflict     { get; protected set; }

    [FoldoutGroup("Extended Details"), ShowInInspector]
    public string Motivation        { get; protected set; }

    [FoldoutGroup("Extended Details"), ShowInInspector]
    public string FurtherDetails    { get; protected set; }

    public void Setup(DefaultMainCharacterTemplate template)
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

        SetSpecies(basicCharacterData.Species.ToString());

        Skills  = new List<string>();
        Fears   = new List<string>();

        foreach (var fear in extendedCharacterData.Fears.Split(','))
            if (fear != "")
                Fears.Add(fear.Trim(' '));

        foreach (var skill in extendedCharacterData.Skills.Split(','))
            if (skill != "")
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

    public void SetPortrait(AnimatedPortrait portrait) => Portrait = portrait;
}
