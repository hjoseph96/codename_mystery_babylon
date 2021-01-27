using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Base:
// Health
// Strength
// Speed
// Skill
// Magic
// Resistance
// Defense

// Follow-up Crit Multiplier
// on 2nd attack of Double Attack, Crit chance is multiplied

// Constitution
// Weapon Weight - Constitution = Speed penalty

// Move (radius)

// Leadership (MC ONLY)

// -----------
// Biorhythm
// -----------
// 
// Biorhythm status is indicated by either the red or green arrow next 
// to a character's portrait or the sine wave in the character's status screen.
// 
// Status effects
// Best: +7 Hit, +7 Avoid
// Good: +5 Hit, +5 Avoid
// Normal: No effect
// Bad: -5 Hit, -5 Avoid
// Worst: -7 Hit, -7 Avoid
// 
// ________
// Derived:
// --------
// --------------
// Attack Damage
// --------------
// 
// Attack = Strength + Weapon's Might 
// 
// Damage = Attack - Defense (if physical) or Resistance (if magic)
// 
// ------------
// Attack Speed
// ------------
// AS = (Speed) - (Burden)
// Burden = (Wt) - (Str); negative Burden values are set to 0
// Speed = value of the given unit's speed
// Wt = value of unit's equipped weapon's weight
// Str = value of the given unit's strength

// 3 HOUSES
// AS = (Speed) - (Burden)
// Burden = (Wt) - [(Str)/5]; negative Burden values are set to 0
//  Speed = value of the given unit's speed
// Wt = value of unit's equipped weapon's weight
// Str = value of the given unit's strength

// --------------
// Support Bonus
// --------------
// 
// Bonus from Support grade/level of allies within 3 spaces

// Supports were originally a hidden feature that gave a boost to 
// Accuracy, Avoid, and Critical if the supporting characters were 
// within three spaces of each other.

// Rank	Bonus (Adjacent)	Bonus (One Space Away)	Support points required
// C	RES +2	RES +1	Starting Rank
// B	DEF/RES +2	DEF/RES +1	6 points
// A	SPD/DEF/RES +2	SPD/DEF/RES +1	18 points (24 total)
// S	ATK/SPD/DEF/RES +2	ATK/SPD/DEF/RES +1	36 points (60 total)


// --------------
// Double Attack
// --------------
// Global Base: Speed must be X amount grater than other Entity
// Default: 4-5. play with it
// Weapons have Double ATK modifiers. +/-
// Second ATK gets Crit Rate multiplied by FCM

// ------------------
// Critical Hit Rate 
// ------------------
// Entitiy's (Skill / 2) + 5% (If S-rank) + the weapon critical chance 
// + the support bonus (if any) + the class critical bonus (if any) 
// + character's skills (if any) - the enemy's Luck

// In FIRE EMBLEM
// Critical hits can be completely negated by a unit carrying an
// Iron Rune (or Hoplon Guard) or with the skill Fortune

// IF crit attack misses, 50/50 chance of landing 
// (or some percentange determined by the Luck of two entities)

// -------------
// Dodge Chance
// -------------

// FE: Radiant Dawn
// Evade Rate = (Aff) + (AS ×2) + (Lck) + (Bio) + (Terr) + (Lead) + (Supp)
// Aff = +5 if defending unit shares affinity with the map
// AS = Defending unit's attack speed
// Lck = Defending unit's luck
// Bio = Modifier from defending unit's biorhythm
// Terr = Modifier from defending unit's terrain tile
// Lead = (Allies only) Bonus from Ike's leadership stars
// Supp = Bonuses from any supportive allies within 3 spaces

// ---------
// Accuracy
// ---------
//
// PHYSICAL & MAGIC 
// Accuracy = (HRatk) - (Evddfn) + (W△)
// HRatk = Attacking unit's hit rate
// Evddfn = Defending unit's evade
// W△ = Weapon triangle effects

// Staffs (aside from healing ones)
// Accuracy = [(Magcst - Resdfn) ×5] + (Sklcst) + 30 - (Dist ×2)
// Magcst = Casting unit's magic
// Resdfn = Defending unit's resistance
// Skl = Casting unit's skill
// Dist = Distance between caster and defender

// ---------
// Hit Rate
// ---------

// Physical and magical hit rate

// Hit Rate = (Aff) + (Skl ×2) + (Lck) + (Bio) + (SpA)
// + (Ht) + (Hit) + (Rnge) + (Lead) + (Supp)

// Aff = +5 if attacking unit shares affinity with the map
// Skl = Attacking unit's skill
// Lck = Attacking unit's luck
// Bio = Modifier from attacking unit's biorhythm
// SpA = Modifier from attacking unit's skills (special abilities)
// Ht = +50 with height advantage; -50 with disadvantage
// Hit = Attacking unit's weapon accuracy
// Rnge = (Bow users only) -30 if attacking an opponent 1 or 3 spaces away
// Lead = Bonus from the armies commander's leadership stars
// Supp = Bonuses from any supportive allies within 3 spaces
// ※ On Hard Mode, Aff = 0 regardless of shared affinity

// -----------
// Aid/Rescue
// -----------

// Aid stat is the maximum constitution or 
// build that another unit can have for the first one 
// to be able to Rescue them.

// Aid for mounted player units is set at (25-Con) for male units 
// and (20-Con) for female units. 
// However, all generic mounted enemies have an Aid of (25-Con)

// The Rescue Command is a command that allows a playable unit to "pick up" another 
// adjacent, allied unit who has a lower constitution than the user's aid. 
// The rescuee will occupy the same space as the rescuer, but they cannot 
// perform any action in that state and they must move in unison with the 
// rescuer. They also cannot be harmed in that state, but the rescuing unit 
// will suffer decreased skill and speed. This command is useful in levels 
// where if certain units survive, the player gains a special item. 
// It can also be used by mounted units to transport slower units around.
 
// mounted units cannot be rescued.
public class Stat : BaseAttribute
{
    protected List<RawBonus> rawBonuses;
    protected List<FinalBonus> finalBonuses;
    
    protected int finalValue;
    public int FinalValue {
        get { return CalculateValue(); }
    }

    public Stat(int startingValue) : base(startingValue) {
        rawBonuses    = new List<RawBonus>();
        finalBonuses  = new List<FinalBonus>();

        finalValue = BaseValue;
    }

    public void AddRawBonus(RawBonus bonus) {
        if (!rawBonuses.Contains(bonus))
            rawBonuses.Add(bonus);
    }

    public void AddFinalBonus(FinalBonus bonus) {
        if (!finalBonuses.Contains(bonus)) {
            finalBonuses.Add(bonus);
            
            bonus.Activate(this);   // Begin countdown upon addition
        }
    }

    public void RemoveRawBonus(RawBonus bonus) {
        if (rawBonuses.Contains(bonus))
            rawBonuses.Remove(bonus);
    }

    public void RemoveFinalBonus(FinalBonus bonus) {
        if (finalBonuses.Contains(bonus))
            finalBonuses.Remove(bonus);
    }

    public virtual int CalculateValue() {
        finalValue = BaseValue;

        ApplyRawBonuses();      // Adding value from raw
        ApplyFinalBonuses();    // Adding value from final
        
        return finalValue;
    }

    protected void ApplyRawBonuses() {
        int rawBonusValue = 0;
        float rawBonusMultiplier = 0;

        foreach(RawBonus bonus in rawBonuses) {
            rawBonusValue += bonus.BaseValue;
            rawBonusMultiplier += bonus.BaseMultiplier;
        }

        finalValue += rawBonusValue;
        finalValue = (int)(finalValue * (1 + rawBonusMultiplier));
    }

    protected void ApplyFinalBonuses() {
        int finalBonusValue = 0;
        float finalBonusMultiplier = 0f;

        foreach(FinalBonus bonus in finalBonuses) {
            finalBonusValue += bonus.BaseValue;
            finalBonusMultiplier += bonus.BaseMultiplier;
        }

        finalValue += finalBonusValue;
        finalValue = (int)(finalValue * (1 + finalBonusMultiplier));
    }
}