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

    [OdinSerialize, ShowInInspector]
    public List<PlayableCharacter> PlayableCharacters { get; private set; }

    public void Init()
    {
        Instance = this;

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
