using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TenPN.DecisionFlex;

public class AttackAction : MonoBehaviour, IAction
{
    public void Perform(IContext context)
    {
        Debug.Log("I AM CHOOSING TO ATTACK");
    }
}
