using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueAttributes
{
    public static EntityReference entity_id(int id)
    {
        return DialogueManager.Instance.CurrentMapDialog.GetEntityByID(id);
    }
}
