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
        float needToRetreat = _aiAgent.NeedToRetreat();

        float currentThreatLevel = _aiAgent.ThreatLevel();
        if (needToRetreat > 0.85f)
            currentThreatLevel -= 0.1f;

        if (needToRetreat > 0.7f)
            currentThreatLevel -= 0.1f;

        context.SetContext("Threat Level", currentThreatLevel);

        context.SetContext("Need To Heal", urgencyToHeal);

        context.SetContext("Need To Retreat", needToRetreat);

        Debug.Log($"Threat Level: {currentThreatLevel}");
        Debug.Log($"Need To Heal: {urgencyToHeal}");
        Debug.Log($"Need To Retreat: {needToRetreat}");

        return context;
    }
}
