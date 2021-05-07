using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class TraitObject
{
    public AIUnitTrait Trait;
    
    [OdinSerialize]
    public List<ActionObject> Actions = new List<ActionObject>();
    
}
