using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StatCalculationMode
{
    FlatEffect,
    Percentage,
    DependOnOtherStat
}
public class Stat
{
    public readonly string Name;
    public float GrowthRate { get; }

    public float RawValue { get; set; }
    public float Value
    {
        get
        {
            var result = RawValue;
            foreach (var effect in _effects)
            {
                result = effect.Apply(this, result);
            }

            return result;
        }

        set => RawValue = value;
    }

    public int RawValueInt
    {
        get => Mathf.RoundToInt(RawValue);
        set => RawValue = value;
    }
    public int ValueInt
    {
        get => Mathf.RoundToInt(Value);
        set => Value = value;
    }

    private readonly List<IEffect> _effects = new List<IEffect>();

    public Stat(string name, float value, float growthRate = -1)
    {
        Name = name;
        RawValue = value;
        GrowthRate = growthRate;
    }

    public Stat(string name, int value, float growthRate = -1)
    {
        Name = name;
        RawValue = value;
        GrowthRate = growthRate;
    }

    public IEnumerator Grow(int times = 1)
    {
        for (var i = 0; i < times; i++)
        {
            var rate = GrowthRate / 100;
            var guaranteeGrowth = Mathf.FloorToInt(rate);
            RawValue += guaranteeGrowth;

            yield return BattleUtility.RollDice();
            float chance = BattleUtility.RNGNumber;

            if (chance <= Mathf.Repeat(rate, 1.0f))
            {
                RawValue += 1;
            }
        }
        yield return null;
    }

    public void GrowPlain(int times = 1)
    {
        for (var i = 0; i < times; i++)
        {
            var rate = GrowthRate / 100;
            var guaranteeGrowth = Mathf.FloorToInt(rate);
            RawValue += guaranteeGrowth;

            //yield return BattleUtility.RollDice();
            //float chance = BattleUtility.RNGNumber;
            float chance = UnityEngine.Random.Range(0, 100);
            chance /= 100.0f;

            if (chance <= Mathf.Repeat(rate, 1.0f))
            {
                RawValue += 1;
            }
        }
    }

    public Stat AddEffect(IEffect effect)
    {
        for (var i = 0; i < _effects.Count; i++)
        {
            if (effect.Priority >= _effects[i].Priority)
            {
                continue;
            }

            _effects.Insert(i, effect);
            return this;
        }

        _effects.Add(effect);
        return this;
    }

    public bool RemoveEffect(IEffect effect)
    {
        return _effects.Remove(effect);
    }
}

public interface IEffect
{
    int Priority { get; }
    string OwnerName { get; set; }
    float Apply(Stat stat, float currentValue);
}


public abstract class EffectBase : IEffect
{
    public int Priority { get; protected set; }
    public string OwnerName { get; set; }

    public virtual float Apply(Stat stat, float currentValue)
    {
        throw new NotImplementedException();
    }
}

[Serializable]
public class FlatBonusEffect : EffectBase
{
    private readonly float _bonusValue;

    public FlatBonusEffect(float bonusValue)
    {
        Priority = 10;
        _bonusValue = bonusValue;
    }

    public FlatBonusEffect(StatusEffectArgs values)
    {
        Priority = 10;
        _bonusValue = values.BonusValue;
    }

    public override float Apply(Stat stat, float currentValue)
    {
        return currentValue + _bonusValue;
    }

    public override string ToString()
    {
        return " From " + OwnerName + " +" + _bonusValue;
    }
}

[Serializable]
public class PercentageBonusEffect : EffectBase
{
    private readonly float _bonusValue;

    public PercentageBonusEffect(float bonusValue)
    {
        Priority = 20;
        _bonusValue = bonusValue;
    }

    public PercentageBonusEffect(StatusEffectArgs values)
    {
        Priority = 20;
        _bonusValue = values.BonusValue;
    }

    public override float Apply(Stat stat, float currentValue)
    {
        return currentValue + stat.RawValue * _bonusValue;
    }

    public override string ToString()
    {
        return " From " + OwnerName + " +" + _bonusValue * 100 + "% of RawValue";
    }
}

[Serializable]
public class DependOnOtherStatEffect : EffectBase
{
    private readonly UnitStat _otherStat;
    private readonly float _ratio;

    public DependOnOtherStatEffect(UnitStat otherStat, float ratio)
    {
        Priority = 0;
        _otherStat = otherStat;
        _ratio = ratio;
    }

    public DependOnOtherStatEffect(StatusEffectArgs values)
    {
        Priority = 0;
        _otherStat = values.OtherStat;
        _ratio = values.Ratio;
    }

    public override float Apply(Stat stat, float currentValue)
    {
        return currentValue + stat.Value * _ratio;
    }

    public override string ToString()
    {
        return " From " + OwnerName + " +" + _ratio * 100 + "% of " + _otherStat.ToString();
    }
}

