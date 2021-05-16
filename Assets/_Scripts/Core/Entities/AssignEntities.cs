using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssignEntities : MonoBehaviour, IInitializable
{
    public void Init()
    {
        foreach(var entityRef in FindObjectsOfType<EntityReference>())
        {
            if (entityRef.AssignedEntity == null)
                entityRef.SetEntityReference();

            EntityManager.Instance.AddEntityReference(entityRef);
        }
    }
}
