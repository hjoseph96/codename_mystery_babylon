using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplyNormalHeal : MonoBehaviour
{
    public static void HealUnit(Unit unit, int amount)
    {
        var healthBarPrefab = Resources.Load("GUI/MiniHealthBar") as GameObject;
        var healEffectPrefab = Resources.Load("Particle_Effects/Heal/NormalHealEffect") as GameObject;

        var healthBarPosition = unit.transform.position;
        healthBarPosition.y -= 0.489f;

        var miniHealthBar = unit.HealthBar;

        var healEffectPosition = healthBarPosition;
        healEffectPosition.z = healEffectPrefab.transform.position.z;
        healEffectPosition.y -= 0.554f;
        
        Instantiate(healEffectPrefab, healEffectPosition, healEffectPrefab.transform.rotation);
        
        
        float startPercentage = (float)unit.CurrentHealth / unit.MaxHealth;
        float newHealthAmount = Mathf.Clamp(unit.CurrentHealth + amount, 1, unit.MaxHealth);
        float endPercentage = newHealthAmount / unit.MaxHealth;
        
        // Trigger health change in unit, to also update the combatText Display
        unit.IncreaseHealth(amount);

        miniHealthBar.OnComplete = null; // Clear Other Events
        // Actually increase the health after all the graphical displays
        miniHealthBar.OnComplete += delegate ()
        {
            if (unit.UponHealComplete != null)
                unit.UponHealComplete.Invoke();
        };
        miniHealthBar.Tween(endPercentage);
    }
}
