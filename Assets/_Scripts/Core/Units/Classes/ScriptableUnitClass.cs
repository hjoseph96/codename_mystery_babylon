using System.IO;
using System.Collections.Generic;
using UnityEngine;

 #if UNITY_EDITOR
using UnityEditor;
#endif
using Sirenix.OdinInspector;
using Sirenix.Serialization;

[CreateAssetMenu(fileName = "NewClass", menuName = "ScriptableObjects/Classes", order = 3)]
public class ScriptableUnitClass : SerializedScriptableObject
{
    public string Title;
    [FoldoutGroup("Stats & Growth")]
    [Header("Relative Power")]
    public int RelativePower;

    [FoldoutGroup("Stats & Growth")]
    [Header("Base Stats")]
    [InfoBox("Note: Base Stats & Growth Rates on Classes only affect AIUnits. Recruitable & Player Units have their own Base Stats and Growth Rates.")]
    
    [UnitStats, OdinSerialize]
    public Dictionary<UnitStat, EditorStat> BaseStats = new Dictionary<UnitStat, EditorStat>();
    
    [FoldoutGroup("Stats & Growth")]
    [Header("Maximum Stats")]
    [UnitStats, OdinSerialize]
    public Dictionary<UnitStat, EditorStat> MaxStats = new Dictionary<UnitStat, EditorStat>();

    [FoldoutGroup("Promotions")]
    [Header("Promoted Bonus")]
    public int PromotedBonus;

    [FoldoutGroup("Promotions")]
    [Header("Promotion Gains")]
    [InfoBox("Stat increases upon promoting into this class.")]
    [UnitStats, OdinSerialize]
    public Dictionary<UnitStat, EditorStat> PromotionGains = new Dictionary<UnitStat, EditorStat>();
    
    [FoldoutGroup("Promotions")]
    [Header("Next Promotions")]
    public List<ScriptableUnitClass> PromotionOptions = new List<ScriptableUnitClass>();

    [FoldoutGroup("Weaponry")]
    [Header("Usable Weapons")]
    public List<WeaponType> UsableWeapons = new List<WeaponType>();

    [FoldoutGroup("Weaponry")]
    [Header("Usable Magic")]
    public List<MagicType> UsableMagic = new List<MagicType>();

    [FoldoutGroup("Status Effects")]
    [SerializeField]
    public List<StatusEffect> StatusEffects = new List<StatusEffect>();

#if UNITY_EDITOR
    [Button("Save As JSON")]
    private void WriteToJSON() => UnitClassRepository.Write(this);
    #endif

    public UnitClass GetUnitClass() => new UnitClass(this);

    #if UNITY_EDITOR
    private void OnValidate()
    {
        var assetPath = AssetDatabase.GetAssetPath(GetInstanceID());
        var fileName = Path.GetFileNameWithoutExtension(assetPath);

        if (string.IsNullOrEmpty(Title))
            Title = fileName;
    }
    #endif
}
