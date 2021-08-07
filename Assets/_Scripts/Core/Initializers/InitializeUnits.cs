using UnityEngine;

public class InitializeUnits : MonoBehaviour, IInitializable
{
    public void Init()
    {
        var campaignManager = CampaignManager.Instance;
        
        foreach (var unit in FindObjectsOfType<Unit>())
        {
            unit.Init();

            if (campaignManager != null)
                campaignManager.AddUnit(unit);
        }

        if (campaignManager != null)
            foreach (var unit in CampaignManager.Instance.AllUnits)
                unit.ApplyAuras(unit.GridPosition);
    }
}
