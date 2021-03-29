using System.Collections.Generic;
using UnityEngine;

using Sirenix.OdinInspector;
using DarkTonic.MasterAudio;



[CreateAssetMenu(fileName = "NewGear", menuName = "ScriptableObjects/Gear", order = 7)]
public class ScriptableGear : ScriptableItem
{
    public GearType Type;

    [FoldoutGroup("Descriptions"), MultiLineProperty(5)]
    public string DescriptionBroken;


    [FoldoutGroup("Stats"), WeaponStats]
    public Dictionary<GearStat, EditorGearStat> GearStats = new Dictionary<GearStat, EditorGearStat>();

    [FoldoutGroup("Stats")]
    public int Weight;

    [FoldoutGroup("Active Ability")]
    public bool IsUsable;

    [FoldoutGroup("Active Ability"), ShowIf("IsUsable")]
    public ScriptableAction Action;

#if UNITY_EDITOR
    [Button("Save As JSON")]
    private void WriteToJSON() => GearRepository.Write(this);

    [Button("Create From JSON")]
    private void CreateFromJSON() => GearRepository.FromJSON(name);
#endif



    public override Item GetItem() => new Gear(this);
}