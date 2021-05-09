using System.Linq;
using System.Collections.Generic;
using UnityEngine;

using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Animancer;

public struct UnitAttackAnimation
{
    public DirectionalAnimationSet AttackAnimation;
    public WeaponType WeaponType;

    public UnitAttackAnimation(DirectionalAnimationSet attackAnimation, WeaponType weaponType)
    {
        WeaponType = weaponType;
        AttackAnimation = attackAnimation;
    }
}

public class UnitAttackAnimations : SerializedMonoBehaviour
{
    [Header("Add Attack Animations")]

    [SerializeField] private WeaponType _weaponType;
    [SerializeField] private DirectionalAnimationSet _attackAnimation;

    [Button("Add Animation")]
    private void AddAttackAniamtion()
    {
        var unitAttackAnimation = new UnitAttackAnimation(_attackAnimation, _weaponType);
        _unitAttackAnimations.Add(unitAttackAnimation);
    }

    [OdinSerialize]
    private List<UnitAttackAnimation> _unitAttackAnimations = new List<UnitAttackAnimation>();

    private Unit _unit;

    private void Start() => _unit = GetComponent<Unit>();

    public DirectionalAnimationSet CurrentAnimation()
    {
        var wieldedWeapon = _unit.EquippedWeapon;

        if (wieldedWeapon == null)
            throw new System.Exception($"[UnitAttackAnimations] Unit#{_unit.Name} is unarmed...");

        var animationforWeapon = _unitAttackAnimations.Where((anim) => anim.WeaponType == wieldedWeapon.Type);
        if (animationforWeapon.Count() == 0)
            throw new System.Exception($"[UnitAttackAnimations] Unit#{_unit.Name} doesn't have an assigned animation for WeaponType: #{wieldedWeapon.Type}");

        var attackAnimation = animationforWeapon.First().AttackAnimation;

        return attackAnimation;
    }
}
