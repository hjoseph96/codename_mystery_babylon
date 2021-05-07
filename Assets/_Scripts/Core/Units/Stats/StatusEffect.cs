using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StatusEffectType
{
    Aura,
    LeaderAura
}

[Serializable]
public class StatusEffect
{
    [SerializeField] public StatusEffectType Type;
    public bool ToAlly;
    public int Radius;
    [SerializeField] protected UnitStat stat;


    private StatCalculationMode _calculationMode;
    [ShowInInspector]
    [SerializeField]
    public StatCalculationMode CalculationMode
    {
        get { return _calculationMode; }
        set
        {
            _calculationMode = value;
            effect = StatusEffectFactory.GetEffect(_calculationMode, values);
        }
    }

    [SerializeField] public StatusEffectArgs values;

    protected IEffect effect;

    public StatusEffect(StatusEffectType _type, UnitStat _stat, bool _toAlly, int _radius, StatCalculationMode _calcMode, StatusEffectArgs _args)
    {
        Type = _type;
        stat = _stat;
        ToAlly = _toAlly;
        Radius = _radius;
        values = _args;
        CalculationMode = _calcMode;
        effect = StatusEffectFactory.GetEffect(_calculationMode, values);
    }

    public void Add(Unit unit)
    {
        unit.Stats[stat].AddEffect(effect);
    }

    public void Remove(Unit unit)
    {
        unit.Stats[stat].RemoveEffect(effect);
    }

    public StatusEffect Copy()
    {
        var statusEffect = new StatusEffect(Type, stat, ToAlly, Radius, CalculationMode, values.Copy());
        statusEffect.effect = StatusEffectFactory.GetEffect(_calculationMode, values);

        return statusEffect;
    }

    public void SetOwnerName(string name)
    {
        effect.OwnerName = name;
    }
}
