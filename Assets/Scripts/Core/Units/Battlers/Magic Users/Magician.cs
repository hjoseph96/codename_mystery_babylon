using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Sirenix.OdinInspector;

public class Magician : Battler
{
    [FoldoutGroup("Magician Specific")]
    public Transform spellCircleSpawnPoint;
    protected override void ProcessAttackingState()
    {
        if (!currentlyAttacking)
        {
            var attackType = GetAttackType();
            string chosenAnimation;

            switch(attackType)
            {
                case AttackType.Normal:
                    chosenAnimation = GetAnimVariation(NormalAttackAnims());
                    PlayAnimation(chosenAnimation);
                    currentlyAttacking = true;

                    break;
                case AttackType.Critical:
                    chosenAnimation = GetAnimVariation(CriticalAttackAnims());
                    PlayAnimation(chosenAnimation);
                    currentlyAttacking = true;

                    break;
                case AttackType.Multiple:
                    if (!IsMultiAttacking())
                    {
                        chosenAnimation = GetAnimVariation(DoubleAttackAnims());
                        PlayAnimation(chosenAnimation);
                        currentlyAttacking = true;
                    }

                    break;
            }
        }
    }

    // Animation Event Handlers

    private void SpawnSpellCircle()
    {
        // When charging, spawn circle effect
        
    }

    private void SpawnSpellEffect()
    {

    }
}
