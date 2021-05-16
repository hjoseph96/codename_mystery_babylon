using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

using Sirenix.OdinInspector;

public class EntityReference : SerializedMonoBehaviour
{
    public readonly Guid GUID = Guid.NewGuid();

    [SerializeField] private EntityType _EntityType;
    public EntityType EntityType { get => _EntityType; }

    [SerializeField, ShowIf("EntityTypeAssigned"), ValueDropdown("GetEntityNames")] private string _EntityName;
    public string EntityName { get => _EntityName; }


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

    public BubblePositionController bubblePositionController;

    // Start is called before the first frame update
    void Start()
    {
        SetEntityReference();
        //TODO: We can add in a loader if it's not on the object by default.            
        bubblePositionController = GetComponentInChildren<BubblePositionController>();
    }

    public void SetEntityReference()
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

    public Vector3 GetSpeechBubblePos(Direction direction = Direction.None)
    {
        if (GetComponent<ISpeakableEntity>() == null)
        {
            Debug.LogWarning("EntityReference has no Speakable components, but is trying to access it! Ensure a component utilizes interface *ISpeakableEntity*");
            return Vector3.zero;
        }
        var entityInfo = GetComponent<ISpeakableEntity>().GetEntityInfo();
        
        if(bubblePositionController == null)
        {
            bubblePositionController = GetComponentInChildren<BubblePositionController>();
            if (bubblePositionController == null)
            {
                Debug.LogWarning("Object is missing BubblePositionController, Verify object is spawned with unit/Set On Prefab");
                return Vector3.zero;
            }
        }
        return bubblePositionController.GetSpeechBubblePos(entityInfo, direction);
    }
}
