using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Sirenix.OdinInspector;
using Sirenix.Serialization;

[CreateAssetMenu(fileName = "NewClass", menuName = "ScriptableObjects/Classes", order = 3)]
public class EditorUnitClass : SerializedScriptableObject
{
    [FoldoutGroup("Stats & Growth")]
    [Header("Base Stats")]
    [InfoBox("Note: Base Stats & Growth Rates on Classes only affect AIUnits. Recruitable & Player Units have their own Base Stats and Growth Rates.")]
    
    [UnitStats, OdinSerialize, HideIf("IsPlaying")]
    public Dictionary<UnitStat, EditorStat> BaseStats = new Dictionary<UnitStat, EditorStat>();
    
    [FoldoutGroup("Base Stats")]
    [Header("Maximum Stats")]
    public Dictionary<UnitStat, int> MaxStats = new Dictionary<UnitStat, int>();

    [FoldoutGroup("Base Stats")]
    [Header("Promotion Gains")]
    public Dictionary<UnitStat, int> PromotionGains = new Dictionary<UnitStat, int>();
}
