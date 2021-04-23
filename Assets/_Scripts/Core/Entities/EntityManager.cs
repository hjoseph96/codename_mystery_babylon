using System.Linq;
using System.Collections.Generic;
using UnityEngine;

using Articy.Unity;
using Articy.Codename_Mysterybabylon;

using Sirenix.Serialization;

public class EntityManager : MonoBehaviour, IInitializable
{
    public static EntityManager Instance;

    [OdinSerialize]
    public List<PlayableCharacter> PlayableCharacters { get; private set; }

    public void Init()
    {
        Instance = this;

        var playableCharacters = ArticyDatabase.GetAllOfType<DefaultMainCharacterTemplate>();

        foreach (var character in playableCharacters)
            PlayableCharacters.Add(new PlayableCharacter(character));


    }

    public PlayableCharacter GetPlayableCharacterByName(string name)
    {
        var matchingCharacter = PlayableCharacters.Where((character) => character.Name == name).First();

        if (matchingCharacter == null)
            throw new System.Exception($"[EntityManager] There's no PlayableCharacter named: {name}");

        return matchingCharacter;
    }

    public List<string> GetPlayableCharacterNames()
    {
        var names = new List<string>();

        foreach (var character in PlayableCharacters)
            names.Add(character.Name);

        return names;
    }
}
