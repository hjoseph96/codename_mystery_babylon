
using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class UnitRepository
{
    public static void Write(Unit unit)
    {
        var parentFolder = $"{Application.dataPath}/SerializedData/Entities/Playable Characters/";
        string jsonPath = $"{parentFolder}/{unit.Name}";

        if (!Directory.Exists(jsonPath))
            Directory.CreateDirectory(jsonPath);

        var unitData = UnitData.Populate(unit);

        string json = JsonConvert.SerializeObject(unitData, Formatting.Indented);

        File.WriteAllText($"{jsonPath}/UnitData.json", json);
    }
}


[Serializable]
public class UnitData
{
    public string UnitType;
    public float WalkSpeed;
    public float RunSpeed;
    public float MoveAnimationSpeed;
    public int Experience;

    // TODO: Find out if there's a cleaner way to do nested JSON in C#...
    public Dictionary<string, Dictionary<string,int>> Stats = new Dictionary<string, Dictionary<string, int>>();
    public List<StatusEffect> StatusEffects = new List<StatusEffect>();
    public Dictionary<string, string> WeaponProfiency       = new Dictionary<string, string>();
    public Dictionary<string, string> MagicProfiency        = new Dictionary<string, string>();

    // Where Key<int> == inventory item slot
    public Dictionary<int, WeaponData> WeaponsInInventory           = new Dictionary<int, WeaponData>();
    public Dictionary<int, ConsumableData> ConsumablesInInventory   = new Dictionary<int, ConsumableData>();
    public Dictionary<int, ValuableData> ValuablesInInventory       = new Dictionary<int, ValuableData>();
    
    // Implement after we add Gear Items
    // public Dictionary<int, GearData> GearInInventory       = new Dictionary<int, GearData>();

    public string UnitPrefabPath;

    public void SerializeItems(Item[] inventoryItems)
    {
        for (var i = 0; i < inventoryItems.Length; i++)
            SerializeItem(inventoryItems[i], i);
    }

    public void SerializeItem(Item inventoryItem, int inventorySlotIndex)
    {
        switch (inventoryItem.ItemType)
        {
            case ItemType.Weapon:
                var weaponData = WeaponData.Populate(inventoryItem as Weapon);
                WeaponsInInventory.Add(inventorySlotIndex, weaponData);
                
                break;
            case ItemType.Consumable:
                var consumableData = ConsumableData.Populate(inventoryItem as Consumable);
                ConsumablesInInventory.Add(inventorySlotIndex, consumableData);
                
                break;
            case ItemType.Valuable:
                var valuableData = new ValuableData();
                valuableData.AssignItemFields(inventoryItem);
                ValuablesInInventory.Add(inventorySlotIndex, valuableData);

                break;
            case ItemType.Gear:
                // Fill in once we add Gear Items
                break;
        }
    }

    public static UnitData Populate(Unit unit)
    {
        var unitData = new UnitData();

        unitData.UnitType           = unit.UnitType.ToString();
        unitData.WalkSpeed          = unit.WalkSpeed;
        unitData.RunSpeed           = unit.RunSpeed;
        unitData.Experience         = unit.Experience;
        unitData.MoveAnimationSpeed = unit.MoveAnimationSpeed;

        if (unit.IsPlaying)
        {
            foreach(var entry in unit.Stats)
            {
                var baseStatAndGrowthRate = new Dictionary<string, int>();
                baseStatAndGrowthRate.Add("Base Value", entry.Value.RawValueInt);
                baseStatAndGrowthRate.Add("Growth Rate", (int)entry.Value.GrowthRate);
                
                unitData.Stats.Add(entry.Key.ToString(), baseStatAndGrowthRate);
            }

            unitData.SerializeItems(unit.Inventory.GetItems<Item>());
        }
        else
        {
            foreach (var entry in unit.EditorStats)
            {
                var baseStatAndGrowthRate = new Dictionary<string, int>();
                baseStatAndGrowthRate.Add("Base Value", entry.Value.Value);
                baseStatAndGrowthRate.Add("Growth Rate", (int)entry.Value.GrowthRate);

                unitData.Stats.Add(entry.Key.ToString(), baseStatAndGrowthRate);
            }
        }

        unitData.StatusEffects = unit.UnitClass.StatusEffects;
        

        if (unit.WeaponProfiency.Count > 0)
            foreach (var entry in unit.WeaponProfiency)
                unitData.WeaponProfiency.Add(entry.Key.ToString(), entry.Value.ToString());
        else
            foreach (var weaponType in unit.UnitClass.UsableWeapons)
                unitData.WeaponProfiency.Add(weaponType.ToString(), WeaponRank.D.ToString());


        if (unit.MagicProfiency.Count > 0)
            foreach (var entry in unit.MagicProfiency)
                unitData.MagicProfiency.Add(entry.Key.ToString(), entry.Value.ToString());
        else
            foreach (var weaponType in unit.UnitClass.UsableMagic)
                unitData.MagicProfiency.Add(weaponType.ToString(), WeaponRank.D.ToString());

        

        return unitData;
    }
}
