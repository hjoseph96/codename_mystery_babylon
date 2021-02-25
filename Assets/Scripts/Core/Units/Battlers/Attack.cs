using System;
using System.Collections.Generic;
using UnityEngine;

public enum AttackType
{
    Normal,
    Critical,
    Multiple
}

public class Attack
{
    public int Damage { get; private set; }
    public bool Landed { get; private set; }
    public bool IsCritical { get; private set; }

    public Attack(int attackDamage, bool landed, bool isCritical)
    {
        Landed = landed;
        IsCritical = isCritical;
        Damage = attackDamage;

        if (IsCritical)
            Damage *= 3;
    }
}
