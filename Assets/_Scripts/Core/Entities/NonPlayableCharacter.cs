using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Sirenix.OdinInspector;
using Articy.Codename_Mysterybabylon;

public class NonPlayableCharacter : Entity
{
    public readonly EntityType EntityType = EntityType.NonPlayableCharacter;


    public void Setup(NonPlayableCharacterTemplate template)
    {
        var basicCharacterData = template.GetFeatureDefaultBasicCharacterFeature();

        Name        = template.DisplayName;
        Age         = (int)basicCharacterData.Age;
        Appearance  = basicCharacterData.Appearance;
        BornIn      = basicCharacterData.BornIn;
        Occupation  = basicCharacterData.Occupation;
        Personality = basicCharacterData.Personality;
        Gender      = basicCharacterData.Sex;

        SetSpecies(basicCharacterData.Species.ToString());
    }

    private void SetSpecies(string species)
    {
        switch (species)
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
                throw new System.Exception($"Species: {species} is unsupported...");
        }
    }
}
