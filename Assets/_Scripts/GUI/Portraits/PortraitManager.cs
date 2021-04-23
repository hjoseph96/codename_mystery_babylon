using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

using Sirenix.OdinInspector;
using Sirenix.Serialization;

using Articy.Unity;
using Articy.Codename_Mysterybabylon;

public struct PortraitTarget
    {
        public EntityType EntityType;
        public string Name;

        public PortraitTarget(EntityType entityType, string name)
        {
            Name = name;
            EntityType  = entityType;
        }
    }

public class PortraitManager : MonoBehaviour, IInitializable
{
    public static PortraitManager Instance;


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
        var portraitTarget = new PortraitTarget(entityType, _name);

        var animatedPortraitForEntity = new AnimatedPortraitForEntity();
        animatedPortraitForEntity.Setup(portraitTarget, _animatedPortrait);

        var alreadyAdded = AnimatedPortraitsForEntities.Any(delegate (AnimatedPortraitForEntity portraitContainer)
        {
            var alreadySet = false;

            var sameEntityType = portraitContainer.PortraitTarget.EntityType == _entityType;
            var nameAlreadySet = portraitContainer.PortraitTarget.Name == _name;
            if (sameEntityType && nameAlreadySet)
                alreadySet = true;

            return alreadySet;
        });

        if (alreadyAdded)
            return;

        AnimatedPortraitsForEntities.Add(animatedPortraitForEntity);
    }

    [OdinSerialize]
    public List<AnimatedPortraitForEntity> AnimatedPortraitsForEntities;

    public void Init()
    {
        Instance = this;
    }
}
