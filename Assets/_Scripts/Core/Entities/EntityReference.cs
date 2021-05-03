using System;
using System.Collections.Generic;
using UnityEngine;

using Sirenix.OdinInspector;

public class EntityReference : SerializedMonoBehaviour
{
    public readonly Guid GUID = Guid.NewGuid();

    [SerializeField] private EntityType _EntityType;
    [SerializeField, ShowIf("EntityTypeAssigned"), ValueDropdown("GetEntityNames")] private string _EntityName;
    private bool EntityTypeAssigned() => _EntityType != EntityType.None;
    private List<string> GetEntityNames()
    {
        switch (_EntityType)
        {
            case EntityType.PlayableCharacter:
                return EntityManager.GetAllPlayableCharacterNames();
            case EntityType.NonPlayableCharacter:
                return EntityManager.GetAllNPCNames();
        }

        throw new Exception($"[EntityReference] No list of names for EntityType: {_EntityType}");
    }

    private Entity _entity;
    [ShowInInspector] public Entity AssignedEntity { get => _entity; }

    // Start is called before the first frame update
    void Start()
    {
        SetEntityReference();
    }

    private void SetEntityReference()
    {
        switch (_EntityType)
        {
            case EntityType.PlayableCharacter:
                _entity = EntityManager.Instance.GetPlayableCharacterByName(_EntityName);
                break;
            case EntityType.NonPlayableCharacter:
                _entity = EntityManager.Instance.GetNPCByName(_EntityName);
                break;
        }
    }
}
