using System.Linq;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Crosstales.TrueRandom;

public static class BattleUtility
{
    public static int RNGNumber;
    private static Dictionary<string, bool> _hitResults = new Dictionary<string, bool>();
    private static Dictionary<string, bool> _critResults = new Dictionary<string, bool>();

    private static Dictionary<string, bool> _battleResults = new Dictionary<string, bool>();

    public static Dictionary<string, bool> BattleResults { get => _battleResults;  }

    /// <summary>
    /// performs a roll with the true random utility
    /// </summary>
    public static IEnumerator RollDice()
    {
        var diceRolled = false;

        void OnGenerateIntegerFinished(List<int> results, string key)
        {
            RNGNumber = results[0];

            if (RNGNumber >= 0) diceRolled = true;
        }


        yield return new WaitUntil(() => TRManager.Instance.isGenerating == false);

        TRManager.Instance.OnGenerateIntegerFinished += OnGenerateIntegerFinished;
        TRManager.Instance.GenerateInteger(0, 100);

        yield return new WaitUntil(() => diceRolled);

        TRManager.Instance.OnGenerateIntegerFinished -= OnGenerateIntegerFinished;

        if (RNGNumber == -1)
            throw new System.Exception("Invalid Dice Roll!");


        Debug.Log($"[BattleUtility] RNG: {RNGNumber}");
    }

    /// <summary>
    /// Returns true or false for hits 
    /// </summary>
    public static IEnumerator HitResults(Unit attacker, Unit defender)
    {
        var attackPreview = attacker.PreviewAttack(defender, attacker.EquippedWeapon);

        yield return RollDice();
        var chance = RNGNumber;

        var hitChance = attackPreview["ACCURACY"];

        if (WithinRange(chance, hitChance))
            _hitResults["HIT"] = true;
        else
            _hitResults["HIT"] = false;

        if (attacker.CanDoubleAttack(defender, attacker.EquippedWeapon))
        {
            _hitResults["DOUBLE_ATTACK"] = true;

            yield return RollDice();
            chance = RNGNumber;

            if (WithinRange(chance, hitChance))
                _hitResults["SECOND_HIT"] = true;
            else
                _hitResults["SECOND_HIT"] = false;
        }
        else
        {
            _hitResults["DOUBLE_ATTACK"] = false;
        }
    }

    /// <summary>
    /// Returns the results of criticals calculated
    /// </summary>
    public static IEnumerator CriticalHitResults(Unit attacker, Unit defender)
    {
        var critResults = new Dictionary<string, bool>();
        var attackPreview = attacker.PreviewAttack(defender, attacker.EquippedWeapon);

        yield return RollDice();
        var chance = RNGNumber;

        var critChance = attackPreview["CRIT_RATE"];

        if (WithinRange(chance, critChance))
            _critResults["CRITICAL"] = true;
        else
            _critResults["CRITICAL"] = false;

        if (attacker.CanDoubleAttack(defender, attacker.EquippedWeapon))
        {
            yield return RollDice();

            chance = RNGNumber;

            if (WithinRange(chance, critChance))
                _critResults["CRIT_SECOND_HIT"] = true;
            else
                _critResults["CRIT_SECOND_HIT"] = false;
        }
    }

    /// <summary>
    /// Gets a Collection of results for battle 
    /// </summary>
    /// <returns>returns results, empty if attacker can't defend</returns>
    public static IEnumerator CalculateBattleResults(Unit attacker, Unit defender)
    {
        yield return HitResults(attacker, defender);
        yield return CriticalHitResults(attacker, defender);

        Debug.Log("Battle results are in!");

        // Merge the dictionaries
        _battleResults = _hitResults.Concat(_critResults)
            .ToLookup(x => x.Key, x => x.Value)
            .ToDictionary(x => x.Key, g => g.First());
    }

    /// <summary>
    /// Returns amount of Exp Gained for attack results
    /// </summary>
    public static int CalculateEXPGain(Battler battlerToGain, Battler foughtAgainstBattler)
    {

        var hostileUnit = foughtAgainstBattler.Unit;
        var unitToGain = battlerToGain.Unit;

        var damageDealt = battlerToGain.DamageDealt;

        int expGained = ((31 + hostileUnit.Level + hostileUnit.Class.PromotedBonus) - (unitToGain.Level - unitToGain.Class.PromotedBonus)) / unitToGain.Class.RelativePower;
        if (damageDealt <= 0)
            expGained = 1;


        // [EXP from doing damage] + [Silencer factor] × [Enemy's level × Enemy Class relative power + Enemy class bonus - 
        // ([Player's level × Player Class relative power + Player class bonus] / Mode divisor) + 20 + Thief bonus + Boss bonus + Entombed bonus
        if (hostileUnit.CurrentHealth == 0)
        {
            var enemyExpCalc = (hostileUnit.Level * hostileUnit.Class.RelativePower + hostileUnit.Class.PromotedBonus);
            var playerExpCalc = (unitToGain.Level * unitToGain.Class.RelativePower + unitToGain.Class.PromotedBonus);

            // TODO: replace 2 with mode divisor, add boss units
            expGained += ((enemyExpCalc - playerExpCalc) / 2) + 20;
        }

        return expGained;
    }
    private static bool WithinRange(int chance, int range) => chance >= 1 && chance <= range;
}
