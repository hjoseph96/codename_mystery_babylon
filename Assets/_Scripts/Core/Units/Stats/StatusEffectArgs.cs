using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StatusEffectArgs
{
    [SerializeField] public float BonusValue;
    [SerializeField] public UnitStat OtherStat;
    [SerializeField] public float Ratio;

    public StatusEffectArgs(float bonusValue)
    {
        BonusValue = bonusValue;
    }

    public StatusEffectArgs(UnitStat otherStat, float ratio)
    {
        OtherStat = otherStat;
        Ratio = ratio;
    }

    public StatusEffectArgs(float bonusValue, UnitStat otherStat, float ratio)
    {
        BonusValue = bonusValue;
        OtherStat = otherStat;
        Ratio = ratio;
    }

    public StatusEffectArgs Copy()
    {
        return new StatusEffectArgs(BonusValue, OtherStat, Ratio);
    }
}
