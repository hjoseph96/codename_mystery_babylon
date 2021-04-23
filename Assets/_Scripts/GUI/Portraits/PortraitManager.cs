using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

using Sirenix.OdinInspector;
using Sirenix.Serialization;

using Articy.Unity;
using Articy.Codename_Mysterybabylon;

public class PortraitManager : MonoBehaviour, IInitializable
{
    public static PortraitManager Instance;

    [FoldoutGroup("Portrait Holders")]
    [SerializeField] private GameObject _PlayableCharacterHolder;


    [SerializeField] private EntityType _entityType;
    [SerializeField, ValueDropdown("EntityNames")] private string _name;
    [SerializeField] private AnimatedPortrait _animatedPortrait;

    private List<string> EntityNames()
    {
        switch(_entityType)
        {
            case EntityType.PlayableCharacter:
                List<string> entityNames = new List<string>();

                var playableCharacters = ArticyDatabase.GetAllOfType<DefaultMainCharacterTemplate>();

                foreach (var character in playableCharacters)
                    entityNames.Add(character.DisplayName);

                return entityNames;
            default:
                throw new Exception("[PortraitManager] You've somehow set an incorrect EntityType...");
        }
    }

    [Button("Assign Portrait")]
    private void AssignPortrait()
    {
        var entityType = (EntityType)Enum.Parse(typeof(EntityType), _entityType.ToString());

        var alreadyAdded = AnimatedPortraitsForEntities.Any(delegate (AnimatedPortraitForEntity portraitContainer)
        {
            var alreadySet = false;

            var sameEntityType = portraitContainer.EntityType == _entityType;
            var nameAlreadySet = portraitContainer.Name == _name;
            if (sameEntityType && nameAlreadySet)
                alreadySet = true;

            return alreadySet;
        });

        if (alreadyAdded)
            return;

        var parent = GetEntityTypeObject();
        var portraitObj = new GameObject($"{_name} [Portrait]");
        var animatedPortraitForEntity = portraitObj.AddComponent<AnimatedPortraitForEntity>();
        animatedPortraitForEntity.Setup(_entityType, _name, _animatedPortrait);

        portraitObj.transform.SetParent(parent.transform);

        AnimatedPortraitsForEntities.Add(animatedPortraitForEntity);
    }

    private GameObject GetEntityTypeObject()
    {
        switch (_entityType)
        {
            case EntityType.PlayableCharacter:
                return _PlayableCharacterHolder;
            default:
                throw new Exception($"[PortraitManager] Somehow, we could not find a Portrait Holder GameObject for EntityType: {_entityType}");
        }
    }

    [OdinSerialize]
    public List<AnimatedPortraitForEntity> AnimatedPortraitsForEntities;

    public void Init()
    {
        Instance = this;
    }
}
