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
    public bool Landed { get; private set; }
    public bool IsCritical { get; private set; }

    private int _baseAtkDamage;
    private Weapon _attackingWithWeapon;


    public Attack(Unit attacker, bool landed, bool isCritical)
    {
        Landed = landed;
        IsCritical = isCritical;

        _baseAtkDamage = attacker.AttackDamage();
        _attackingWithWeapon = attacker.EquippedWeapon;
    }

    public int Damage(Unit targetUnit)
    {
        int damageDealt = _baseAtkDamage;

        if (IsCritical)
            damageDealt *= 3;

        var defenseBuffer = targetUnit.Stats[UnitStat.Defense].ValueInt;
        if (_attackingWithWeapon.Type == WeaponType.Grimiore)
            defenseBuffer = targetUnit.Stats[UnitStat.Resistance].ValueInt;
        
        damageDealt -= defenseBuffer;
        if (damageDealt < 0)
            damageDealt = 0;
        
        return damageDealt;
    }
}
