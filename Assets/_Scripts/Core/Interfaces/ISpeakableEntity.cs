using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISpeakableEntity
{
    /// <summary>
    /// This Method should create a new EntityInfo based on the class's current set up, all variables should be set
    /// </summary>
    public BubblePositionController.EntityInfo GetEntityInfo();
}


