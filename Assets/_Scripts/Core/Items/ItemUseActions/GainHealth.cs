using UnityEngine;

[CreateAssetMenu(fileName = "NewTestAction", menuName = "ScriptableObjects/Actions/TestAction", order = 1)]
public class GainHealth : ScriptableAction
{
    public int Value;

    public override void Use(Unit unit) => CampaignManager.Instance.HealUnit(unit, Value);
}