using System;
using System.IO;
using System.Collections.Generic;
 
#if UNITY_EDITOR
using UnityEditor;
#endif

using Newtonsoft.Json;


[Serializable]
public class ItemData
{
    public string Filename;
    public string ItemName;
    public string ItemType;

    public string Description;
    public Dictionary<string, uint> Pricing = new Dictionary<string, uint>();
    public Dictionary<string, string> Icon  = new Dictionary<string, string>();

    public void AssignItemFields(ScriptableItem source)
    {
        Filename    = source.name;
        ItemName    = source.Name;
        Description = source.Description;
        ItemType    = source.ItemType.ToString();
        
        Pricing.Add("Cost", source.Cost);
        Pricing.Add("SaleValue", source.SaleValue);

        #if UNITY_EDITOR
            var path    = AssetDatabase.GetAssetPath(source.Icon);
            path        = Path.ChangeExtension(path, ".spriteatlas");
        
            Icon.Add("AtlasPath", path);
            Icon.Add("SpriteName", source.Icon.name);
        #endif
    }

    public void AssignItemFields(Item source)
    {
        ItemName    = source.Name;
        Description = source.Description;
        ItemType    = source.ItemType.ToString();

        Pricing.Add("Cost", source.Cost);
        Pricing.Add("SaleValue", source.SaleValue);

        #if UNITY_EDITOR
            var path = AssetDatabase.GetAssetPath(source.Icon);
            path = Path.ChangeExtension(path, ".spriteatlas");

            Icon.Add("AtlasPath", path);
            Icon.Add("SpriteName", source.Icon.name);
        #endif
    }
}


[Serializable]
public class WeaponData : ItemData
{
    public string WeaponType;
    public string DescriptionBroken;
    public string MeleeSound;
    public string RequiredRank;
    public int Weight;
    public int MaxDurability;
    public Dictionary<string, int> AttackRange  = new Dictionary<string, int>();
    public Dictionary<string, int> Stats        = new Dictionary<string, int>();
    public Dictionary<string, int> BrokenStats  = new Dictionary<string, int>();

    public bool IsSyncedWith(ScriptableWeapon weapon)
    {
        var targetData = WeaponRepository.WeaponToData(weapon);

        string targetJson = JsonConvert.SerializeObject(targetData, Formatting.Indented);
        string currentJson = JsonConvert.SerializeObject(this, Formatting.Indented);

        return targetJson == currentJson;
    }

    public static WeaponData Populate(Weapon weapon)
    {
        var weaponData = new WeaponData();

        weaponData.AssignItemFields(weapon);

        weaponData.WeaponType           = weapon.Type.ToString();
        weaponData.DescriptionBroken    = weapon.DescriptionBroken;
        weaponData.MeleeSound           = weapon.MeleeSound;
        weaponData.RequiredRank         = weapon.RequiredRank.ToString();
        weaponData.Weight               = weapon.Weight;
        weaponData.MaxDurability        = weapon.MaxDurability;

        weaponData.AttackRange = new Dictionary<string, int>
        {
            { "MinRange", weapon.MinRange },
            { "MaxRange", weapon.MaxRange }
        };

        foreach (var entry in weaponData.Stats)
            weaponData.Stats.Add(entry.Key.ToString(), entry.Value);

        foreach (var entry in weaponData.BrokenStats)
            weaponData.BrokenStats.Add(entry.Key.ToString(), entry.Value);

        return weaponData;
    }
}

[Serializable]
public class ConsumableData : ItemData
{
    public bool CanHeal;
    public string ActionToExecute;

    public static ConsumableData Populate(Consumable consumable)
    {
        var consumableData = new ConsumableData();

        consumableData.AssignItemFields(consumable);

        consumableData.CanHeal = consumable.CanHeal;

        return consumableData;
    }
}

[Serializable]
public class ValuableData : ItemData { }