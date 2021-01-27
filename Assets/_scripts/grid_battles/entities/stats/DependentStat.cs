using System.Collections.Generic;

public class DependentStat : Stat
{
    protected List<Stat> _otherStats;

    public DependentStat(int startingValue) : base(startingValue) {
        _otherStats = new List<Stat>();
    }

    public void AddStat(Stat stat) {
        _otherStats.Add(stat);
    }

    public void RemoveStat(Stat stat) {
        if (_otherStats.Contains(stat))
            _otherStats.Remove(stat);
    }

    public override int CalculateValue()
    {
        // Specific attribute code goes somewhere in here
            
        finalValue = BaseValue;
            
        ApplyRawBonuses();       
        ApplyFinalBonuses();
            
        return finalValue;
    }
}