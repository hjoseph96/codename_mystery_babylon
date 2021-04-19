using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DarkTonic.MasterAudio;

public class Magician : Battler
{
    private Transform _spellCircleSpawnPoint;

    private GameObject _spellCircleInstance;
    private ParticleSystem _spellCircle;
    private MagicEffect _magicEffect;

    private bool _effectSpawned = false;
    private bool _circleSpawned = false;
    private bool _nextAttackQueued = false;
    private bool _attackingWithMagic = false;

    public override void Setup(Unit unit, BattleHUD hud, Dictionary<string, bool> battleResults, PostEffectMask pixelShaderMask)
    {
        base.Setup(unit, hud, battleResults, pixelShaderMask);

        if (Unit.EquippedWeapon.Type == WeaponType.Grimiore)
            _attackingWithMagic = true;
    }

    public void SetMagicCircleSpawnPoint(Transform spawnPoint) => _spellCircleSpawnPoint = spawnPoint;

    protected override void ProcessAttackingState()
    {
        if (!currentlyAttacking)
        {
            var attackType = GetAttackType();
            string chosenAnimation;

            switch (attackType)
            {
                case AttackType.Normal:
                    chosenAnimation = $"Attack 0{currentAttackIndex + 1}";
                    PlayAnimation(chosenAnimation);
                    currentlyAttacking = true;

                    break;
                case AttackType.Critical:
                    chosenAnimation = GetAnimVariation(CriticalAttackAnims());
                    PlayAnimation(chosenAnimation);
                    currentlyAttacking = true;

                    break;
                case AttackType.Multiple:
                    // if (!IsMultiAttacking())
                    // {
                    //     chosenAnimation = GetAnimVariation(DoubleAttackAnims());
                    //     PlayAnimation(chosenAnimation);
                    //     currentlyAttacking = true;
                    // }

                    chosenAnimation = $"Attack 0{currentAttackIndex + 1}";
                    PlayAnimation(chosenAnimation);
                    currentlyAttacking = true;

                    break;
            }
        }
    }

    protected override void Update()
    {
        base.Update();

        if (_circleSpawned && _spellCircle != null && _spellCircle.time > 0.97f)
        {
            _spellCircle = null;
            Destroy(_spellCircleInstance);
            ReleaseSpell();
        }

        if (_nextAttackQueued && !_magicEffect.IsActive)
        {
            _nextAttackQueued = false;
            GoToNextAttack();
        }

    }

    // Animation Event Handlers

    protected override void NextAttack()
    {
        if (_magicEffect.IsActive)
            _nextAttackQueued = true;
        else
            GoToNextAttack();

    }

    private void GoToNextAttack()
    {
        bool targetDead = CurrentAttack.Landed && CurrentAttack.Damage(targetBattler.Unit) > targetBattler.Unit.CurrentHealth;

        if (targetDead || currentAttackIndex == Attacks.Count - 1)
            FinishFighting();
        else
            currentAttackIndex += 1;

        currentlyAttacking = false;
        _circleSpawned = false;
        _effectSpawned = false;
    }

    private void SpawnSpellCircle()
    {
        if (!_circleSpawned)
        {
            var magicCircle = Unit.EquippedWeapon.magicCirclePrefab;

            var spellCircleObj = Instantiate(
                magicCircle, _spellCircleSpawnPoint.position, magicCircle.transform.rotation
            );
            _spellCircleInstance = spellCircleObj;
            _spellCircle = spellCircleObj.GetComponentInChildren<ParticleSystem>();

            MasterAudio.PlaySound3DFollowTransform(Unit.EquippedWeapon.castingSound, CampaignManager.AudioListenerTransform);
            _circleSpawned = true;
        }
    }


    private void ReleaseSpell()
    {
        var releaseAnimation = $"Attack 0{currentAttackIndex + 1} - Release";
        PlayAnimation(releaseAnimation);
    }

    private void SpawnSpellEffect()
    {
        var effect = Unit.EquippedWeapon.magicEffect;

        if (!_effectSpawned)
        {
            var targetPoint = targetBattler.GetComponent<Collider>().bounds.center;
            var spawnPoint = _spellCircleSpawnPoint.position;
            if (effect.EffectType == MagicEffectType.Area)
                spawnPoint = targetPoint;

            _magicEffect = Instantiate(effect, spawnPoint, effect.transform.rotation).GetComponent<MagicEffect>();

            _effectSpawned = true;

            _magicEffect.OnHitTarget += delegate () {
                ProcessAttack();
            };

            if (_magicEffect.EffectType == MagicEffectType.Projectile)
                _magicEffect.StartMoving(targetPoint);
        }
    }
}
