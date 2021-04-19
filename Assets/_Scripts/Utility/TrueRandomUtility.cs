using System.Collections.Generic;
using Crosstales.TrueRandom;
using UnityEngine;
using System.Threading.Tasks;

public static class TrueRandomUtility
{
    public static async Task<int> RollDice()
    {
        var rollResult = -1;
        var diceRolled = false;

        void OnGenerateIntegerFinished(List<int> results, string key)
        {
            rollResult = results[0];

            if (rollResult > 0) diceRolled = true;
        }

        TRManager.Instance.OnGenerateIntegerFinished += OnGenerateIntegerFinished;
        TRManager.Instance.GenerateInteger(0, 100);

        await new WaitUntil(() => diceRolled);

        TRManager.Instance.OnGenerateIntegerFinished -= OnGenerateIntegerFinished;

        if (rollResult == -1)
            throw new System.Exception("Invalid Dice Roll!");

        return rollResult;
    }

    public static async Task<Dictionary<string, bool>> HitResults(Unit attacker, Unit defender)
    {
        var hitResults = new Dictionary<string, bool>();
        var attackPreview = attacker.PreviewAttack(defender, attacker.EquippedWeapon);

        var chance = await RollDice();
        var hitChance = attackPreview["ACCURACY"];

        if (WithinRange(chance, hitChance))
            hitResults["HIT"] = true;
        else
            hitResults["HIT"] = false;

        if (attacker.CanDoubleAttack(defender, attacker.EquippedWeapon))
        {
            hitResults["DOUBLE_ATTACK"] = true;
            chance = await RollDice();

            if (WithinRange(chance, hitChance))
                hitResults["SECOND_HIT"] = true;
            else
                hitResults["SECOND_HIT"] = false;
        }
        else
        {
            hitResults["DOUBLE_ATTACK"] = false;
        }

        return hitResults;
    }

    public static async Task<Dictionary<string, bool>> CriticalHitResults(Unit attacker, Unit defender)
    {
        var critResults = new Dictionary<string, bool>();
        var attackPreview = attacker.PreviewAttack(defender, attacker.EquippedWeapon);

        var chance = await RollDice();
        var critChance = attackPreview["CRIT_RATE"];

        if (WithinRange(chance, critChance))
            critResults["CRITICAL"] = true;
        else
            critResults["CRITICAL"] = false;

        if (attacker.CanDoubleAttack(defender, attacker.EquippedWeapon))
        {
            chance = await RollDice();

            if (WithinRange(chance, critChance))
                critResults["CRIT_SECOND_HIT"] = true;
            else
                critResults["CRIT_SECOND_HIT"] = false;
        }

        return critResults;
    }

    private static bool WithinRange(int chance, int range) => chance >= 1 && chance <= range;
}
