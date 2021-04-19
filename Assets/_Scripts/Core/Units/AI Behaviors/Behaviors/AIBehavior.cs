using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Sirenix.OdinInspector;

public enum AIBehaviorState
{
    NotStarted,
    Executing,
    Failed,
    Complete
}

public enum AIActionType
{
    Attack,
    Defend,
    Retreat,
    Heal,
    Group,
    Default
}

public class AttackBehavior : AIBehavior
{
    [ReadOnly] private readonly AIActionType _actionType = AIActionType.Attack;

    public override AIActionType ActionType() => _actionType;
}


public class DefendBehavior : AIBehavior
{
    [ReadOnly] private readonly AIActionType _actionType = AIActionType.Defend;

    public override AIActionType ActionType() => _actionType;
}

public class RetreatBehavior : AIBehavior
{
    [ReadOnly] private readonly AIActionType _actionType = AIActionType.Retreat;

    public override AIActionType ActionType() => _actionType;
}

public class HealBehavior : AIBehavior
{
    [ReadOnly] private readonly AIActionType _actionType = AIActionType.Heal;

    public override AIActionType ActionType() => _actionType;
}

public class GroupBehavior : AIBehavior
{
    [ReadOnly] private readonly AIActionType _actionType = AIActionType.Group;

    public override AIActionType ActionType() => _actionType;
}

public class AIBehavior : MonoBehaviour
{
    protected AIBehaviorState executionState = AIBehaviorState.NotStarted;
    [ShowInInspector] public AIBehaviorState ExecutionState { get => executionState; }
    protected AIUnit AIAgent;

    public virtual AIActionType ActionType() => throw new System.Exception("You haven't set your ActionType() METHOD!");

    /// <summary>
    /// This will begin to execution of this AIBehavior at runtime
    /// </summary>
    public virtual void Execute() => throw new System.Exception("You didn't implement Execute() for this AIBehavior!");

    public void SetTargetAgent(AIUnit agent) => AIAgent = agent;
}
