using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Sirenix.OdinInspector;
using Articy.Codename_Mysterybabylon;

public class NonPlayableCharacter : Entity
{
    public readonly EntityType EntityType = EntityType.NonPlayableCharacter;


    [FoldoutGroup("Game Data"), ShowInInspector]
    public AnimatedPortrait Portrait { get; protected set; }

    // Articy Template Properties

    [FoldoutGroup("Basic Information"), ShowInInspector]
    public string Name { get; protected set; }

    [FoldoutGroup("Basic Information"), ShowInInspector]
    public int Age { get; protected set; }

    [FoldoutGroup("Basic Information"), ShowInInspector]
    public string Appearance { get; protected set; }

    [FoldoutGroup("Basic Information"), ShowInInspector]
    public string BornIn { get; protected set; }

    [FoldoutGroup("Basic Information"), ShowInInspector]
    public string Occupation { get; protected set; }

    [FoldoutGroup("Basic Information"), ShowInInspector]
    public string Personality { get; protected set; }

    [FoldoutGroup("Basic Information"), ShowInInspector]
    public Sex Gender { get; protected set; }

    [FoldoutGroup("Basic Information"), ShowInInspector]
    public Species Species { get; protected set; }

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
