using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DarkTonic.MasterAudio;

public class Magician : Battler
{
    private Transform _spellCircleSpawnPoint;
    private bool _attackingWithMagic = false;

    private bool _circleSpawned = false;
    private GameObject _spellCircleInstance;
    private ParticleSystem _spellCircle;
    private bool _effectSpawned = false;
    private MagicEffect _magicEffect;

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

            switch(attackType)
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
    }

    // Animation Event Handlers

    protected override void NextAttack()
    {
        base.NextAttack();
        
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
        // TODO: Dynamically play the correct release animation by currently playing animation
        // Or use Animator trigger, paramaters?
        var releaseAnimation = $"Attack 0{currentAttackIndex + 1} - Release";
        PlayAnimation(releaseAnimation);
    }

    private void SpawnSpellEffect()
    {
        var effect = Unit.EquippedWeapon.magicEffect;

        if (!_effectSpawned)
        {
            _magicEffect = Instantiate(
                effect, _spellCircleSpawnPoint.position, effect.transform.rotation
            ).GetComponent<MagicEffect>();

            _effectSpawned = true;

            _magicEffect.OnHitTarget += delegate() {
                ProcessAttack();
            };

            var targetPoint = targetBattler.GetComponent<Collider>().bounds.center;
            _magicEffect.StartMoving(targetPoint);
        }
    }
}
