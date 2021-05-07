using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StatusEffectFactory
{
    public static IEffect GetEffect(StatCalculationMode calculationMode, StatusEffectArgs values)
    {
        switch (calculationMode)
        {
            case StatCalculationMode.FlatEffect:
                return new FlatBonusEffect(values);
            case StatCalculationMode.Percentage:
                return new PercentageBonusEffect(values);
            case StatCalculationMode.DependOnOtherStat:
                return new DependOnOtherStatEffect(values);
            default:
                return null;
        }
    }

}
