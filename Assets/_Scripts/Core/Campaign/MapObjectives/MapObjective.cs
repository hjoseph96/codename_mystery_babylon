using System;
using UnityEngine;

using Sirenix.OdinInspector;

public enum ObjectiveType
{
    Win,
    Loss
};


public class MapObjective : SerializedMonoBehaviour
{
    [ReadOnly] protected ObjectiveType objectiveType;
    public ObjectiveType ObjectiveType { get => objectiveType; }

    protected CampaignManager campaignManager;
    protected WorldGrid worldGrid;

    protected virtual void Start()
    {
        worldGrid = WorldGrid.Instance;
        campaignManager = CampaignManager.Instance;
    }

    public virtual bool CheckConditions()
    {
        throw new Exception("[MapObjective] You  did not override this method to check OBjective Clearance!");
    }
}
