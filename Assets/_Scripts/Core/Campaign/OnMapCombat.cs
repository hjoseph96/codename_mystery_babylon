using System;
using System.Linq;

using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

using Sirenix.OdinInspector;

public class OnMapCombat : MonoBehaviour
{
    [HideInInspector] public static Action OnCombatComplete;

    public static async void BeginCombat(Unit attacker, Unit defender)
    {
        attacker.UponAttackComplete += async delegate ()
        {
            if (defender != null && defender.IsAlive)
            {
                defender.UponAttackComplete += OnCombatComplete;

                attacker.UponDodgeComplete += defender.UponAttackComplete;

                attacker.UponDamageCalcComplete += defender.UponAttackComplete;

                await ProcessAttack(defender, attacker);

                defender.AttackOnMap(attacker);
            }
            else if (OnCombatComplete != null)
            {
                attacker.ClearOnMapBattlEvents();
                defender.ClearOnMapBattlEvents();

                OnCombatComplete.Invoke();
                OnCombatComplete = null;
            }
        };

        defender.UponDodgeComplete += attacker.UponAttackComplete;

        defender.UponDamageCalcComplete += attacker.UponAttackComplete;

        await ProcessAttack(attacker, defender);
        

        attacker.AttackOnMap(defender);
    }

    private static async Task<bool> ProcessAttack(Unit attacker, Unit defender)
    {
        var attackerPreview = await BattleResults(attacker, defender);
        var atkCount = 1;
        attacker.UponAttackAnimationEnd += delegate ()
        {
            if (attackerPreview["DOUBLE_ATTACK"])
                atkCount++;
            else
                attacker.SetIdle();
        };
        attacker.UponAttackLaunched += delegate ()
        {
            if (atkCount == 1)
            {
                var atkLanded = attackerPreview["HIT"];

                if (!atkLanded)
                {
                    defender.DodgeAttack(attacker);
                    return;
                }

                var isCritical = attackerPreview["CRITICAL"];
                var attack = new Attack(attacker, atkLanded, isCritical);

                defender.TakeDamage(attack.Damage(defender));
            }

            if (atkCount == 2)
            {
                var atkLanded = attackerPreview["SECOND_HIT"];

                if (!atkLanded)
                {
                    defender.DodgeAttack(attacker);
                    return;
                }

                var isCritical = attackerPreview["CRIT_SECOND_HIT"];
                var attack = new Attack(attacker, atkLanded, isCritical);

                defender.TakeDamage(attack.Damage(defender));
            }
        };

        return true;
    }

    public static async Task<Dictionary<string, bool>> BattleResults(Unit attacker, Unit defender)
    {
        Dictionary<string, bool> battleResults = new Dictionary<string, bool>();

        // If the attacking cannot defend himself, return empty
        if (!attacker.CanDefend())
            return battleResults;

        var hitResults = await TrueRandomUtility.HitResults(attacker, defender);
        var critResults = await TrueRandomUtility.CriticalHitResults(attacker, defender);

        // Merge the dictionaries
        return hitResults.Concat(critResults)
            .ToLookup(x => x.Key, x => x.Value)
            .ToDictionary(x => x.Key, g => g.First());
    }
}
