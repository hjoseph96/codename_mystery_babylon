using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DarkTonic.MasterAudio;

public class Magician : Battler
{
    private Transform _spellCircleSpawnPoint;

    private GameObject _spellCircle;
    private MagicEffect _magicEffect;

    private bool _effectSpawned = false;
    private bool _circleSpawned = false;
    private bool _nextAttackQueued = false;

    private int _timesCharged;
    
    // in case we ever want mages who can use physical weapons!
    private bool _attackingWithMagic = false;

    public override void Setup(Unit unit, BattleHUD hud, Dictionary<string, bool> battleResults)
    {
        base.Setup(unit, hud, battleResults);

        if (Unit.EquippedWeapon.Type == WeaponType.Grimiore)
            _attackingWithMagic = true;
    }

    public void SetMagicCircleSpawnPoint(Transform spawnPoint) => _spellCircleSpawnPoint = spawnPoint;

    protected override void ProcessAttackingState()
    {
        if (!currentlyAttacking)
        {
            var attackType = GetAttackType();

            switch (attackType)
            {
                case AttackType.Normal:
                    PlayAnimation("Attack");
                    currentlyAttacking = true;

                    break;
                case AttackType.Critical:
                    PlayAnimation("Critical Attack");
                    currentlyAttacking = true;

                    break;
            }
        }
    }

    protected override void Update()
    {
        base.Update();

        if (_nextAttackQueued && !_magicEffect.IsActive)
        {
            _nextAttackQueued = false;
            GoToNextAttack();
        }

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

    // Animation Event Handlers

    protected override void NextAttack()
    {
        if (_magicEffect.IsActive)
            _nextAttackQueued = true;
        else
            GoToNextAttack();

    }


    private void SpawnSpellCircle()
    {
        if (!_circleSpawned)
        {
            var magicCircle = Unit.EquippedWeapon.magicCirclePrefab;

            var spellCircleObj = Instantiate(
                magicCircle, _spellCircleSpawnPoint.position, magicCircle.transform.rotation
            );
            _spellCircle = spellCircleObj;

            MasterAudio.PlaySound3DFollowTransform(Unit.EquippedWeapon.castingSound, CampaignManager.AudioListenerTransform);
            _circleSpawned = true;
        }
    }


    private void ReleaseSpell()
    {
        _timesCharged++;

        if (_timesCharged == 4)
        {
            Destroy(_spellCircle);
            _spellCircle = null;
            PlayAnimation("Attack - Release");
        }
    }

    private void SpawnSpellEffect()
    {
        var effect = Unit.EquippedWeapon.magicEffect;

        if (!_effectSpawned)
        {
            var targetPoint = targetBattler.GetComponent<Renderer>().bounds.center;
            var spawnPoint = _spellCircleSpawnPoint.position;
            if (effect.EffectType == MagicEffectType.Area)
                spawnPoint = targetPoint;

            _magicEffect = Instantiate(effect, spawnPoint, effect.transform.rotation).GetComponent<MagicEffect>();

            _magicEffect.OnHitTarget += delegate () {
                ProcessAttack();

                StartCoroutine(WaitForReaction(NextAttack));
            };

            _effectSpawned = true;

            if (_magicEffect.EffectType == MagicEffectType.Projectile)
            {
                _magicEffect.StartMoving(targetPoint);
            }
            else if (_magicEffect.EffectType == MagicEffectType.Area && _magicEffect.ApplyPostProcessing)
            {
                StartCoroutine(_magicEffect.PostProcessWhileActive(targetBattler));
            }
        }
    }

}
