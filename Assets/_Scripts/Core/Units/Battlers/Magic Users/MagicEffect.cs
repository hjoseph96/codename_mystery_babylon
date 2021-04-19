using System;
using System.Collections;
using UnityEngine;

using DarkTonic.MasterAudio;
using Sirenix.OdinInspector;

public enum MagicEffectType {
    Projectile,
    Area
}

public class MagicEffect : MonoBehaviour
{
    public MagicEffectType EffectType;
    [ShowIf("IsProjectile")] public float moveSpeed = 5f;
    [ShowIf("IsProjectile")] public GameObject hitEffect;
    [ShowIf("IsArea")] public float areaEffectWaitTime;
    [SoundGroupAttribute] public string movingSound;
    [SoundGroupAttribute, ShowIf("IsProjectile")] public string hitSound;
    [HideInInspector] public Action OnHitTarget;

    private bool _isActive = false;
    public bool IsActive => _isActive;

    private Vector3 _target;
    private Collider _collider;
    private ParticleSystem _effect;
    private bool _startedMoving = false;
    private bool _startedWaiting = false;
    private bool _cooldown = false;

    void Start() 
    {
        _collider = GetComponent<Collider>();
        _effect   = GetComponent<ParticleSystem>();
    
        // play spell sfx on spawn for area spells.
        if (EffectType == MagicEffectType.Area)
            MasterAudio.PlaySound3DFollowTransform(movingSound, CampaignManager.AudioListenerTransform);

        _isActive = true;
    }

    public void StartMoving(Vector3 target)
    {
        _target = target;
        _startedMoving = true;
        MasterAudio.PlaySound3DFollowTransform(movingSound, CampaignManager.AudioListenerTransform);
    }
    public void TravelToTarget()
    {
        this.transform.LookAt(_target, Vector3.up);
        this.transform.position = Vector3.Slerp(
            this.transform.position,
            _target,
            moveSpeed * Time.deltaTime
        );
    }

    private void Update()
    {
        if (_startedMoving)
            TravelToTarget();

        if (!_startedWaiting && EffectType == MagicEffectType.Area)
            StartCoroutine(WaitBeforeHittingTarget());

        if (_cooldown)
            CooldownAndDestroy();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (EffectType == MagicEffectType.Area)
            _cooldown = true;
        
        _collider.enabled = false;
        Destroy(this.gameObject);
        _isActive = false;
        
        if (EffectType == MagicEffectType.Projectile)
        {
            Instantiate(hitEffect, this.transform.position, hitEffect.transform.rotation);
            MasterAudio.PlaySound3DFollowTransform(hitSound, CampaignManager.AudioListenerTransform);
        }

        OnHitTarget.Invoke();
    }

    private void CooldownAndDestroy()
    {
        _cooldown = false;

        _effect.Stop();

        if (_effect.particleCount == 0)
        {
            Destroy(this.gameObject);
            _isActive = false;
        }
    }

    private IEnumerator WaitBeforeHittingTarget()
    {
        yield return new WaitForSeconds(areaEffectWaitTime);

        _collider.enabled = true;
    }

    private bool IsProjectile() => EffectType == MagicEffectType.Projectile;
    private bool IsArea() => EffectType == MagicEffectType.Area;
}
