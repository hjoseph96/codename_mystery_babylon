using System.Linq;
using System.Collections.Generic;
using UnityEngine;

using Articy.Unity;
using Articy.Codename_Mysterybabylon;

using Sirenix.Serialization;
using Sirenix.OdinInspector;

public class EntityManager : SerializedMonoBehaviour, IInitializable
{
    public static EntityManager Instance;

    [FoldoutGroup("Entity Parent Objects")]
    [SerializeField] private GameObject _PlayableCharactersObj;
    [SerializeField] private GameObject _NonPlayableCharactersObj;

    [OdinSerialize, ShowInInspector]
    public List<PlayableCharacter> PlayableCharacters { get; private set; }
    public List<NonPlayableCharacter> NonPlayableCharacters { get; private set; }

    public void Init()
    {
        Instance = this;

        PopulatePlayableCharacters();
        PopulateNonPlayableCharacters();
    }


    public PlayableCharacter GetPlayableCharacterByName(string name)
    {
        var matchingCharacter = PlayableCharacters.Where((character) => character.Name == name).First();

        if (matchingCharacter == null)
            throw new System.Exception($"[EntityManager] There's no PlayableCharacter named: {name}");

        return matchingCharacter;
    }

    public static List<string> GetAllPlayableCharacterNames()
    {
        var names = new List<string>();

        foreach (var character in ArticyDatabase.GetAllOfType<DefaultMainCharacterTemplate>())
            names.Add(character.DisplayName);

        return names;
    }

    public List<string> GetPlayableCharacterNames()
    {
        var names = new List<string>();

        foreach (var character in PlayableCharacters)
            names.Add(character.Name);

        return names;
    }

    public static List<string> GetAllNPCNames()
    {
        var names = new List<string>();

        foreach (var character in ArticyDatabase.GetAllOfType<NonPlayableCharacterTemplate>())
            names.Add(character.DisplayName);

        return names;
    }

    public List<string> GetNPCNames()
    {
        var names = new List<string>();

        foreach (var character in NonPlayableCharacters)
            names.Add(character.Name);

        return names;
    }

    public NonPlayableCharacter GetNPCByName(string name)
    {
        var matchingCharacter = NonPlayableCharacters.Where((character) => character.Name == name).First();

        if (matchingCharacter == null)
            throw new System.Exception($"[EntityManager] There's no PlayableCharacter named: {name}");

        return matchingCharacter;
    }


    private void PopulatePlayableCharacters()
    {
        var playableCharacters = ArticyDatabase.GetAllOfType<DefaultMainCharacterTemplate>();

        foreach (var characterTemplate in playableCharacters)
        {
            var playableCharacterObj = new GameObject($"{characterTemplate.DisplayName} [PlayableCharacter]");
            playableCharacterObj.transform.SetParent(_PlayableCharactersObj.transform);

            var playableCharacter = playableCharacterObj.AddComponent<PlayableCharacter>();

            playableCharacter.Setup(characterTemplate);

            PlayableCharacters.Add(playableCharacter);
        }
    }

    private void PopulateNonPlayableCharacters()
    {
        var NPCs = ArticyDatabase.GetAllOfType<NonPlayableCharacterTemplate>();

        foreach (var characterTemplate in NPCs)
        {
            var nonPlayableCharacterObj = new GameObject($"{characterTemplate.DisplayName} [NPC]");
            nonPlayableCharacterObj.transform.SetParent(_NonPlayableCharactersObj.transform);

            var NPC = nonPlayableCharacterObj.AddComponent<NonPlayableCharacter>();

            NPC.Setup(characterTemplate);

            NonPlayableCharacters.Add(NPC);
        }
    }
}
