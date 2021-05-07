using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[CreateAssetMenu(fileName = "AI Traits", menuName = "ScriptableObjects/AI")]
public class TraitsDB : SerializedScriptableObject
{

    public List<TraitObject> Traits = new List<TraitObject>();
}
