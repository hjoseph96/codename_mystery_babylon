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

            if (entityRef.tag == "Player" || entityRef.tag == "Main Player")
            {
                entityRef.transform.parent = null;
                DontDestroyOnLoad(entityRef);
            }

            EntityManager.Instance.AddEntityReference(entityRef);
        }
    }
}
