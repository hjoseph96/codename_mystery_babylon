using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveStatusEffect 
{
    private StatusEffect statusEffect;
    private Action<Unit> action;

    public ActiveStatusEffect(StatusEffect _statusEffect, Action<Unit> _action)
    {
        statusEffect = _statusEffect;
        action = _action;
    }

}
