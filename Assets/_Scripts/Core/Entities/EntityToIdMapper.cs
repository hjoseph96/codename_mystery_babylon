using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityToIdMapper : SerializedMonoBehaviour
{

    public static EntityToIdMapper Instance;
    [OdinSerialize]
    public Dictionary<int, EntityReference> EntityById;

    public EntityReference GetEntity(int id) => EntityById[id];

    private void Awake()
    {
        Instance = Instance == null ? this : Instance;
    }

}
