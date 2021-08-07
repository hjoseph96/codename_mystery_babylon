using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Sirenix.OdinInspector;

public class OnMapCombat : MonoBehaviour
{
    [ShowInInspector] public static Action OnCombatComplete;

    public static IEnumerator BeginCombat(Unit attacker, Unit defender)
    {

        defender.UponAttackComplete += delegate ()
        {
            defender.WaitForReaction(attacker, delegate ()
            {
                attacker.ClearOnMapBattleEvents();
                defender.ClearOnMapBattleEvents();

                attacker.TookAction();
                
                OnCombatComplete?.Invoke();
                OnCombatComplete = null;
            });
        };

        attacker.UponDodgeComplete += defender.UponAttackComplete;


        attacker.UponAttackComplete += delegate ()
        {
            if (!defender.IsDying)
            {
                Debug.Log("Defender is attacking!");
                Debug.Log("Defender is attacking and processing attack");
                defender.StartCoroutine(OnMapCombat.ProcessAttack(defender, attacker));
            }
            else
            {
                attacker.WaitForReaction(defender, delegate()
                {
                    defender.ClearOnMapBattleEvents();
                    attacker.ClearOnMapBattleEvents();

                    attacker.TookAction();
                });
            }

        };

        yield return ProcessAttack(attacker, defender);
    }

    private static IEnumerator ProcessAttack(Unit attacker, Unit defender)
    {
        Debug.Log("Before getting battle results!");

        yield return BattleUtility.CalculateBattleResults(attacker, defender);

        var attackerPreview = BattleUtility.BattleResults;

        var atkCount = 1;

        Debug.Log("Processing Attack.");
        attacker.UponAttackLaunched += delegate ()
        {
            Debug.Log($"Attacker: [{attacker.gameObject.name}] launched an attack! ATK Count: {atkCount}");


            if (atkCount == 1)
            {
                var atkLanded = attackerPreview["HIT"];

                if (!atkLanded)
                {
                    defender.DodgeAttack(attacker);
                    Debug.Log($"Defender: [{defender.gameObject.name}] dodged!");
                    return;
                }

                var isCritical = attackerPreview["CRITICAL"];
                var attack = new Attack(attacker, atkLanded, isCritical);
                defender.TakeDamage(attack.Damage(defender), isCritical);

                Debug.Log($"Defender: [{defender.gameObject.name}] took damage...");

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

                defender.TakeDamage(attack.Damage(defender), isCritical);
            }
        };

        attacker.UponAttackAnimationEnd += delegate ()
        {
            if (attackerPreview["DOUBLE_ATTACK"])
                atkCount++;
            else
            {
                attacker.SetIdle();

                attacker.UponAttackComplete?.Invoke();
            }

        };

        attacker.AttackOnMap(defender);
    }
}
