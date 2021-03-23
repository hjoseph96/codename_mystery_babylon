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

        float currentThreatLevel = _aiAgent.ThreatLevel();
        context.SetContext("Threat Level", currentThreatLevel);

        float urgencyToHeal = _aiAgent.NeedToHeal();
        context.SetContext("Need To Heal", urgencyToHeal);

        float needToRetreat = _aiAgent.NeedToRetreat();
        context.SetContext("Need To Retreat", needToRetreat);

        Debug.Log($"Threat Level: {currentThreatLevel}");
        Debug.Log($"Need To Heal: {urgencyToHeal}");
        Debug.Log($"Need To Retreat: {needToRetreat}");

        return context;
    }
}
