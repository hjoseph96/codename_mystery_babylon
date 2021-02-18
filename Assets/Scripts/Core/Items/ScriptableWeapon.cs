using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "NewWeapon", menuName = "ScriptableObjects/Weapon", order = 3)]
public class ScriptableWeapon : ScriptableItem
{
    [PreviewField]
    public Sprite Icon;

    public string Name;
    public WeaponType Type;

    [FoldoutGroup("Descriptions"), MultiLineProperty(5)] public string Description, DescriptionBroken;

    [FoldoutGroup("Stats"), WeaponStats]
    public Dictionary<WeaponStat, EditorWeaponStat> WeaponStats = new Dictionary<WeaponStat, EditorWeaponStat>();

    [FoldoutGroup("Stats"), MinMaxSlider(1, 4)]
    public Vector2Int AttackRange = Vector2Int.one;

    [FoldoutGroup("Stats")] 
    public WeaponRank RequiredRank;

    [FoldoutGroup("Stats")]
    public int Weight, MaxDurability;

    public override Item GetItem()
    {
        return new Weapon(this);
    }
}