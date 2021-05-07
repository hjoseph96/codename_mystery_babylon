using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TenPN.DecisionFlex;
using UnityEngine;

public static class TraitUtility
{
    public static void ApplyTrait(Unit unit, AIUnitTrait trait)
    {
        List<ContextValueConsideration> Traits = unit.GetComponentsInChildren<ContextValueConsideration>().ToList();
        var traitsDB = Resources.Load<TraitsDB>("AI Traits");
        foreach (var item in Traits)
        {
            var traitObj = traitsDB.Traits.Find(t => t.Trait == trait);

            if (traitObj != null)
            {
                var actionObj = traitObj.Actions.Find(a => a.ContextName == item.GetContextName());

                if (actionObj != null)
                    item.UpdateCurve(actionObj.ResponseCurve);
                else
                    Debug.LogWarning("The Action " + trait + ">" + item.GetContextName() + " is not present in AI Traits!");
            }
            else
            {
                Debug.LogWarning("The Trait " + trait + " is not present in AI Traits!");
            }
           
        }
    }
}
