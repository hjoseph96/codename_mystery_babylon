using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;


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

    public int Grow(int times = 1)
    {
        var result = 0;
        for (var i = 0; i < times; i++)
        {
            var rate = GrowthRate / 100;
            var guaranteeGrowth = Mathf.FloorToInt(rate);
            RawValue += guaranteeGrowth;
            result += guaranteeGrowth;

            if (Random.value <= Mathf.Repeat(rate, 1.0f))
            {
                RawValue += 1;
                result++;
            }
        }

        return result;
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
    float Apply(Stat stat, float currentValue);
}

public abstract class EffectBase : IEffect
{
    public int Priority { get; protected set; }

    public virtual float Apply(Stat stat, float currentValue)
    {
        throw new NotImplementedException();
    }
}

public class FlatBonusEffect : EffectBase
{
    private readonly float _bonusValue;

    public FlatBonusEffect(float bonusValue)
    {
        Priority = 10;
        _bonusValue = bonusValue;
    }

    public override float Apply(Stat stat, float currentValue)
    {
        return currentValue + _bonusValue;
    }

    public override string ToString()
    {
        return "+" + _bonusValue;
    }
}

public class PercentageBonusEffect : EffectBase
{
    private readonly float _bonusValue;

    public PercentageBonusEffect(float bonusValue)
    {
        Priority = 20;
        _bonusValue = bonusValue;
    }

    public override float Apply(Stat stat, float currentValue)
    {
        return currentValue + stat.RawValue * _bonusValue;
    }

    public override string ToString()
    {
        return "+" + _bonusValue * 100 + "% of RawValue";
    }
}

public class DependOnOtherStatEffect : EffectBase
{
    private readonly Stat _otherStat;
    private readonly float _ratio;

    public DependOnOtherStatEffect(Stat otherStat, float ratio)
    {
        Priority = 0;
        _otherStat = otherStat;
        _ratio = ratio;
    }

    public override float Apply(Stat stat, float currentValue)
    {
        return currentValue + _otherStat.Value * _ratio;
    }

    public override string ToString()
    {
        return "+" + _ratio * 100 + "% of " + _otherStat.Name;
    }
}
