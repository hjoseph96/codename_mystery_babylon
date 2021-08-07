using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEngine;

using Articy.Unity;
using Articy.Codename_Mysterybabylon;

using Sirenix.Serialization;
using Sirenix.OdinInspector;
using System;

public class EntityManager : SerializedMonoBehaviour, IInitializable
{
    public static EntityManager Instance;

    [FoldoutGroup("Entity Parent Objects")]
    [SerializeField] private GameObject _PlayableCharactersObj;
    [SerializeField] private GameObject _NonPlayableCharactersObj;

    [OdinSerialize, ShowInInspector]
    public List<PlayableCharacter> PlayableCharacters { get; private set; }
    [OdinSerialize, ShowInInspector]
    public List<NonPlayableCharacter> NonPlayableCharacters { get; private set; }

    private List<EntityReference> _entityReferences = new List<EntityReference>();
    public List<EntityReference> EntityReferences { get => _entityReferences; }

    public void Init()
    {
        Instance = this;

        PopulatePlayableCharacters();
        PopulateNonPlayableCharacters();
    }

    public List<PlayerUnit> PlayerUnits()
    {
        var players = new List<PlayerUnit>(FindObjectsOfType<PlayerUnit>());
        
        foreach (var player in players)
            if (player.Class == null)
                player.Init();

        return players;
    }

    public PlayableCharacter GetPlayableCharacterByName(string name)
    {
        var matchingCharacter = PlayableCharacters.Where((character) => character.Name == name).First();

        if (matchingCharacter == null)
            throw new Exception($"[EntityManager] There's no PlayableCharacter named: {name}");

        return matchingCharacter;
    }

    public static List<string> GetAllEntityNames()
    {
        var names = new List<string>();

        foreach (var entity in ArticyDatabase.GetAllOfType<Entity>())
            names.Add(entity.Name);

        return names;
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
            throw new Exception($"[EntityManager] There's no PlayableCharacter named: {name}");

        return matchingCharacter;
    }

    public void AddEntityReference(EntityReference entityRef)
    {
        if (!_entityReferences.Contains(entityRef))
            _entityReferences.Add(entityRef);
    }

    public void RemoveEntityReference(EntityReference entityRef)
    {
        if (_entityReferences.Contains(entityRef))
            _entityReferences.Remove(entityRef);
    }

    public EntityReference GetEntityRef(string name, EntityType entityType)
    {
        var matchingEntity = EntityReferences.Where((entityRef) => entityRef.EntityType == entityType && entityRef.EntityName == name);

        if (matchingEntity.Count() > 0)
            return matchingEntity.First();
        else
            throw new Exception($"[EntityManager] There's no {entityType} named {name}");
    }

    public EntityReference GetEntityRef(string techName)
    {
        var result = techName.ToLower();
        if (techName.Count(s => s == '_') > 1)
        {
            var regex = new Regex("_");
            result = regex.Replace(techName, " ", 1).ToLower();
        }

        var matchingEntity = EntityReferences.Where((entityRef) => entityRef.EntityTechnicalName == result);

        if (matchingEntity.Count() > 0)
        {
            var entityRef = matchingEntity.First();
            
            return entityRef;
        }
        else
            throw new Exception($"[EntityManager] There's no entity with technical name {techName}");
    }

    public EntityReference GetEntityRefNullable(string techName)
    {
        var matchingEntity = EntityReferences.Where((entityRef) => entityRef.EntityTechnicalName == techName.ToLower());

       if (matchingEntity.Count() > 0)
            return matchingEntity.FirstOrDefault();
        else
            throw new Exception($"[EntityManager] There's no entity with technical name {techName}");
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
