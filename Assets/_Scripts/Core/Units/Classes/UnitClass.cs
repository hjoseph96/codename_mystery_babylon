using System.Collections.Generic;
using System.Linq;

public class UnitClass
{
    public readonly string Title;
    public readonly int RelativePower = 1;
    public readonly int PromotedBonus = 0;
    public readonly Dictionary<UnitStat, Stat> Stats = new Dictionary<UnitStat, Stat>();
    public readonly Dictionary<UnitStat, int> MaxStats = new Dictionary<UnitStat, int>();
    public readonly Dictionary<UnitStat, int> PromotionGains = new Dictionary<UnitStat, int>();
    public readonly List<UnitClass> PromotionOptions = new List<UnitClass>();
    public readonly List<WeaponType> UsableWeapons = new List<WeaponType>();
    public readonly List<MagicType> UsableMagic = new List<MagicType>();
    public readonly List<StatusEffect> StatusEffects = new List<StatusEffect>();

    public UnitClass(ScriptableUnitClass source)
    {
        Title = source.Title;
        RelativePower = source.RelativePower;
        PromotedBonus = source.PromotedBonus;
        UsableWeapons = source.UsableWeapons;
        UsableMagic = source.UsableMagic;

        foreach (KeyValuePair<UnitStat, EditorStat> entry in source.BaseStats)
            Stats.Add(entry.Key, new Stat(entry.Key.ToString(), entry.Value.Value, entry.Value.GrowthRate));

        foreach (KeyValuePair<UnitStat, EditorStat> entry in source.MaxStats)
            MaxStats.Add(entry.Key, entry.Value.Value);

        foreach (KeyValuePair<UnitStat, EditorStat> entry in source.PromotionGains)
            PromotionGains.Add(entry.Key, entry.Value.Value);

        if (source.PromotionOptions.Count > 0)
            foreach (ScriptableUnitClass nextPromotion in source.PromotionOptions)
                PromotionOptions.Add(nextPromotion.GetUnitClass());

        StatusEffects = source.StatusEffects.Select(se => se.Copy()).ToList();
        
    }
}