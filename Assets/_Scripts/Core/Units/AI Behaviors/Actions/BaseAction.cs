using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TenPN.DecisionFlex;
using System;


public class BaseAction : MonoBehaviour, IAction
{
    private BehaviorList _behaviorList;
    protected AIUnit AIAgent;
    public BehaviorList BehaviorList { get => _behaviorList; }

    public virtual void Perform(IContext context) => throw new System.Exception("You didn't implement Perform() for this IAction!!");

    // Start is called before the first frame update
    void Awake()
    {
        AIAgent = GetComponentInParent<AIUnit>();
        _behaviorList = GetComponentInParent<BehaviorList>();
    }
}
