using System.Collections.Generic;
using UnityEngine;


using Sirenix.OdinInspector;

public class BehaviorList : MonoBehaviour
{
    private AIUnit _aiAgent;

    [FoldoutGroup("Attack Behaviors")]
    public List<AIBehavior> AttackBehaviors;

    [FoldoutGroup("Defend Behaviors")]
    public List<AIBehavior> DefendBehaviors;

    [FoldoutGroup("Retreat Behaviors")]
    public List<AIBehavior> RetreatBehaviors;

    [FoldoutGroup("Heal Behaviors")]
    public List<AIBehavior> HealBehaviors;

    [FoldoutGroup("Heal Allies Behaviors")]
    public List<AIBehavior> HealAlliesBehaviors;

    [FoldoutGroup("Group Behaviors")]
    public List<AIBehavior> GroupBehaviors;

    [FoldoutGroup("Default Behaviors")]
    public List<AIBehavior> DefaultBehaviors;

    private void Awake()
    {
        _aiAgent = GetComponentInParent<AIUnit>();

        // Ensure Correct AIBehaviors are in the correct list...
        ValidateActionList(HealBehaviors, AIActionType.Heal);
        ValidateActionList(HealAlliesBehaviors, AIActionType.Heal);
        ValidateActionList(AttackBehaviors, AIActionType.Attack);
        ValidateActionList(DefendBehaviors, AIActionType.Defend);
        ValidateActionList(RetreatBehaviors, AIActionType.Retreat);
        ValidateActionList(GroupBehaviors, AIActionType.Group);
        ValidateActionList(DefaultBehaviors, AIActionType.Default);
    }

    private void ValidateActionList(List<AIBehavior> listToValidate, AIActionType requiredType)
    {
        foreach (AIBehavior behavior in listToValidate)
            //if (behavior.ActionType() != requiredType)
            //    throw new System.Exception($"AIBehavior: {behavior.GetType().ToString()} is a {behavior.ActionType()} behavior included in {requiredType.ToString()} Behaviors...");
            //else
                behavior.SetTargetAgent(_aiAgent);
    }
}
