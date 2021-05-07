using UnityEngine;

public class InitializeUnits : MonoBehaviour, IInitializable
{
    public void Init()
    {
        foreach (var unit in FindObjectsOfType<Unit>())
        {
            unit.Init();
            CampaignManager.Instance.AddUnit(unit);
        }

        foreach (var unit in CampaignManager.Instance.AllUnits)
            unit.ApplyAuras(unit.GridPosition);
    }
}
