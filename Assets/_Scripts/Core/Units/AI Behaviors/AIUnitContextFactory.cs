using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TenPN.DecisionFlex;

public class AIUnitContextFactory : SingleContextFactory
{
    //////////////////////////////////////////////////

    private AIUnit _aiAgent;

    //////////////////////////////////////////////////

    private void Awake() => _aiAgent = GetComponentInParent<AIUnit>();

    public override IContext SingleContext(Logging loggingSetting)
    {
        var context = new ContextDictionary();

        float urgencyToHeal = _aiAgent.NeedToHeal();
        float urgencyToHealAlly = _aiAgent.NeedToHealAlly();
        float needToRetreat = _aiAgent.NeedToRetreat();
        float needToBeWithGroup = _aiAgent.NeedToBeWithGroup();
        float needToResortToDefault = _aiAgent.NeedToResortToDefault();

        float currentThreatLevel = _aiAgent.ThreatLevel();
        if (needToRetreat > 0.85f)
            currentThreatLevel -= 0.1f;

        if (needToRetreat > 0.7f)
            currentThreatLevel -= 0.1f;

        context.SetContext("Threat Level", currentThreatLevel);

        context.SetContext("Need To Heal", urgencyToHeal);

        context.SetContext("Need To Heal Ally", urgencyToHealAlly);

        context.SetContext("Need To Retreat", needToRetreat);

        context.SetContext("Need To Be With Group", needToBeWithGroup);

        //context.SetContext("Need To Resort To Default", needToResortToDefault);

        Debug.Log($"Threat Level: {currentThreatLevel}");
        Debug.Log($"Need To Heal: {urgencyToHeal}");
        Debug.Log($"Need To Heal Ally: {urgencyToHealAlly}");
        Debug.Log($"Need To Retreat: {needToRetreat}");
        Debug.Log($"Need To Be With Group: {needToBeWithGroup}");
        //Debug.Log($"Need To Resort To Default: {needToResortToDefault}");

        return context;
    }
}
