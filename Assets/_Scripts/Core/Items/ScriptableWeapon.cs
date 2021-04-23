using System.Collections.Generic;
using UnityEngine;

using Sirenix.OdinInspector;
using DarkTonic.MasterAudio;



[CreateAssetMenu(fileName = "NewWeapon", menuName = "ScriptableObjects/Weapon", order = 3)]
public class ScriptableWeapon : ScriptableItem
{
    public WeaponType Type;

    // Grimiore Specific Fields
    [ShowIf("Type", WeaponType.Grimiore)] public MagicType MagicType;

    [FoldoutGroup("Magic Effects"), ShowIf("Type", WeaponType.Grimiore)]
    public GameObject magicCirclePrefab;
    [FoldoutGroup("Magic Effects"), ShowIf("Type", WeaponType.Grimiore)]
    public MagicEffect magicEffect;


    [FoldoutGroup("Descriptions"), MultiLineProperty(5)] 
    public string DescriptionBroken;

    [FoldoutGroup("Audio")] 
    [SoundGroup, ShowIf("Type", WeaponType.Sword), ShowIf("Type", WeaponType.Lance), ShowIf("Type", WeaponType.Axe)]
    public string meleeSound;

    [FoldoutGroup("Audio")] 
    [SoundGroup, ShowIf("Type", WeaponType.Grimiore)]
    public string castingSound;

    [FoldoutGroup("Stats"), WeaponStats]
    public Dictionary<WeaponStat, EditorWeaponStat> WeaponStats = new Dictionary<WeaponStat, EditorWeaponStat>();

    [FoldoutGroup("Stats"), MinMaxSlider(1, 4)]
    public Vector2Int AttackRange = Vector2Int.one;

    [FoldoutGroup("Stats")] 
    public WeaponRank RequiredRank;

    [FoldoutGroup("Stats")]
    public int Weight, MaxDurability;

    [FoldoutGroup("Active Ability")]
    public bool IsUsable;

    [FoldoutGroup("Active Ability"), ShowIf("IsUsable")]
    public ScriptableAction Action;

    #if UNITY_EDITOR
    [Button("Save As JSON")]
    private void WriteToJSON() => WeaponRepository.Write(this);

    [Button("Create From JSON")]
    private void CreateFromJSON() => WeaponRepository.FromJSON(name);
    #endif
    


    public override Item GetItem() => new Weapon(this);
}